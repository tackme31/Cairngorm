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

        public virtual List<Item> GetRecommendation(int count = 10)
        {
            if (count <= 0)
            {
                throw new ArgumentException($"'{nameof(count)}' should be a positive integer.");
            }

            var index = GetSearchIndex();
            using (var context = index.CreateSearchContext())
            {
                var tagsWeight = GetTagsWeight();
                var boosting = tagsWeight.Keys.Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0.0f)),
                    (acc, tag) => acc.Or(item => item[Setting.SearchField].Equals(tag).Boost(tagsWeight[tag])));

                var query = context.GetQueryable<T>();
                query = ApplyItemsFilter(query);
                query = ApplySettingFilter(query);
                query = query.Where(boosting).OrderByDescending(item => item["score"]).Take(count);

                return query.ToList().Select(doc => doc.GetItem()).ToList();
            }
        }

        protected virtual ISearchIndex GetSearchIndex() => ContentSearchManager.GetIndex((SitecoreIndexableItem)Context.Item);

        protected virtual IQueryable<T> ApplyItemsFilter(IQueryable<T> query) => query.Filter(item => item.Language == Context.Language.Name);

        private IQueryable<T> ApplySettingFilter(IQueryable<T> query)
        {
            if (Setting.SearchTemplates.Any())
            {
                var templatesPred = Setting.SearchTemplates.Aggregate(
                    PredicateBuilder.False<T>(),
                    (acc, template) => ID.TryParse(template, out var id) ? acc.Or(item => item.TemplateId == id) : acc.Or(item => item.TemplateName == template));
                query = query.Filter(templatesPred);
            }

            if (Setting.FilterStoredItems)
            {
                var itemIds = new HashSet<ID>(Setting.ItemsStore.GetItems().Select(item => item.ID));
                query = itemIds.Aggregate(query, (acc, id) => acc.Filter(item => item.ItemId != id));
            }

            if (Setting.FilterContextItem)
            {
                query = query.Filter(item => item.ItemId != Context.Item.ID);
            }

            if (!string.IsNullOrWhiteSpace(Setting.SearchScope))
            {
                var scope = ID.IsID(Setting.SearchScope) ? ID.Parse(Setting.SearchScope) : Context.Database.GetItem(Setting.SearchScope)?.ID;
                if (!ID.IsNullOrEmpty(scope))
                {
                    query = query.Filter(item => item.Paths.Contains(scope));
                }
            }

            return query;
        }

        private IDictionary<string, float> GetTagsWeight()
        {
            var tagsWeight = new Dictionary<string, float>();
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
                    if (tagsWeight.ContainsKey(tag))
                    {
                        tagsWeight[tag] += weight;
                    }
                    else
                    {
                        tagsWeight[tag] = weight;
                    }
                }
            }

            return tagsWeight;
        }
    }
}
