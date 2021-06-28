using Cairngorm.Configurations;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cairngorm.Services
{
    public class DefaultRecommender<T> : IRecommender where T : SearchResultItem
    {
        protected RecommenderSetting Setting { get; }

        public DefaultRecommender(RecommenderSetting setting)
        {
            Setting = setting;
        }

        public List<Item> GetRecommendation(int count = 10)
        {
            if (count <= 0)
            {
                throw new ArgumentException($"'{nameof(count)}' should be a positive integer.");
            }

            var index = GetSearchIndex();
            using (var context = index.CreateSearchContext())
            {
                var boostDataset = GetBoostDataset();
                var boosting = boostDataset.Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0.0f)),
                    (acc, data) => acc.Or(item => item[Setting.SearchField].Equals(data.Tag).Boost(data.Weight)));

                var query = context.GetQueryable<T>();
                query = ApplyItemsFilter(query);
                query = ApplySearchTemplatesFilter(query);
                query = ApplyStoredItemsFilter(query);
                query = ApplyContextItemFilter(query);
                query = ApplySearchScopeFilter(query);
                query = query.Where(boosting).OrderByDescending(item => item["score"]).Take(count);

                return query.ToList().Select(doc => doc.GetItem()).ToList();
            }
        }

        protected virtual ISearchIndex GetSearchIndex() => ContentSearchManager.GetIndex((SitecoreIndexableItem)Context.Item);

        protected virtual IQueryable<T> ApplyItemsFilter(IQueryable<T> query) => query.Filter(item => item.Language == Context.Language.Name);

        protected virtual IQueryable<T> ApplySearchTemplatesFilter(IQueryable<T> query)
        {
            if (!Setting.SearchTemplates.Any())
            {
                return query;
            }

            var templatesPred = Setting.SearchTemplates.Aggregate(
                PredicateBuilder.False<T>(),
                (acc, template) => ID.TryParse(template, out var id) ? acc.Or(item => item.TemplateId == id) : acc.Or(item => item.TemplateName == template));
            return query.Filter(templatesPred);
        }

        protected virtual IQueryable<T> ApplyStoredItemsFilter(IQueryable<T> query)
        {
            if (!Setting.FilterStoredItems)
            {
                return query;
            }

            var itemIds = new HashSet<ID>(Setting.ItemsStore.GetItems().Select(item => item.ID));
            return itemIds.Aggregate(query, (acc, id) => acc.Filter(item => item.ItemId != id));
        }

        protected virtual IQueryable<T> ApplyContextItemFilter(IQueryable<T> query)
        {
            if (!Setting.FilterContextItem)
            {
                return query;
            }

            return query.Filter(item => item.ItemId != Context.Item.ID);
        }

        protected virtual IQueryable<T> ApplySearchScopeFilter(IQueryable<T> query)
        {
            if (string.IsNullOrWhiteSpace(Setting.SearchScope))
            {
                return query;
            }

            var scope = ID.IsID(Setting.SearchScope) ? ID.Parse(Setting.SearchScope) : Context.Database.GetItem(Setting.SearchScope)?.ID;
            if (ID.IsNullOrEmpty(scope))
            {
                return query;
            }

            return query.Filter(item => item.Paths.Contains(scope));
        }

        private List<BoostData> GetBoostDataset()
        {
            var tagAndWeight = new Dictionary<string, float>();
            var items = Setting.ItemsStore.GetItems();
            foreach (var item in items.Select((item, index) => new { item, index }))
            {
                var tags = Setting.TagResolvers.SelectMany(resolver => resolver.GetItemTags(item.item)).Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
                var weightRatio = Setting.BoostGradually
                    ? 1 - (float) item.index / items.Count
                    : 1;
                foreach (var tag in tags)
                {
                    var weight = Setting.WeightPerMatching * weightRatio;
                    if (tagAndWeight.ContainsKey(tag))
                    {
                        tagAndWeight[tag] += weight;
                    }
                    else
                    {
                        tagAndWeight[tag] = weight;
                    }
                }
            }

            return tagAndWeight.Select(tw => new BoostData(tw.Key, tw.Value)).ToList();
        }

        private class BoostData
        {
            public BoostData(string tag, float weight)
            {
                Tag = tag;
                Weight = weight;
            }

            public string Tag { get; }
            public float Weight { get; }
        }
    }
}
