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
    public class DefaultRecommender<T> : Recommender where T : SearchResultItem
    {
        protected RecommenderSetting Setting { get; }

        public DefaultRecommender(RecommenderSetting setting)
        {
            Setting = setting;
        }

        public override List<Item> GetRecommedation(int count = 10)
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
                var templatesPred = Setting.SearchTemplates.Aggregate(PredicateBuilder.False<T>(), (acc, id) => acc.Or(item => item.TemplateId == id));
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
                if (!scope.IsNull)
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
            foreach (var (item, index) in items.Select((item, index) => (item, index + 1)))
            {
                var tags = Setting.TagResolvers.SelectMany(resolver => resolver.GetItemTags(item)).Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
                foreach (var tag in tags)
                {
                    var weight = Setting.BoostGradually ? Setting.WeightPerMatching / index : Setting.WeightPerMatching;
                    AddOrUpdate(tagsWeight, tag, weight);
                }
            }

            return tagsWeight;

            void AddOrUpdate(Dictionary<string, float> dic, string tag, float value)
            {
                if (tagsWeight.ContainsKey(tag))
                {
                    tagsWeight[tag] += value;
                }
                else
                {
                    tagsWeight[tag] = value;
                }
            }
        }
    }
}
