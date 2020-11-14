using Cairngorm.Abstractions;
using Cairngorm.Configurations;
using Cairngorm.Services;
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
using System.Web;

namespace Cairngorm
{
    public class Recommender<T> : RecommenderBase where T : SearchResultItem
    {
        protected IItemTagsResolver TagsResolver { get; }
        protected RecommenderSetting Setting { get; }

        public Recommender(RecommenderSetting recommenderSetting, IItemTagsResolver tagsResolver)
        {
            TagsResolver = tagsResolver;
            Setting = recommenderSetting;
        }

        public override List<Item> GetRecommedations(int count = 10)
        {
            if (count <= 0)
            {
                throw new InvalidOperationException($"'{nameof(count)}' should be a positive integer.");
            }

            var index = GetSearchIndex();
            using (var context = index.CreateSearchContext())
            {
                var query = context.GetQueryable<T>();

                query = ApplyFilterQuery(query);

                if (!Setting.SearchTemplates.Any())
                {
                    var templatesPred = Setting.SearchTemplates.Aggregate(PredicateBuilder.False<T>(), (acc, id) => acc.Or(item => item.TemplateId == id));
                    query = query.Filter(templatesPred);
                }

                if (Setting.FilterStoredItems)
                {
                    var itemIds = new HashSet<ID>();
                    GetIdsFromCookie(Setting.CookieInfo.Name).ForEach(id => itemIds.Add(id));

                    query = itemIds.Aggregate(query, (acc, id) => acc.Filter(item => item.ItemId != id));
                }

                if (Setting.FilterContextItem)
                {
                    query = query.Filter(item => item.ItemId != Context.Item.ID);
                }

                var tagsWeight = GetTagsWeight();
                var boosting = tagsWeight.Keys.Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0.0f)),
                    (acc, tag) => acc.Or(item => item[Setting.SearchField].Equals(tag).Boost(tagsWeight[tag])));

                query = query.Where(boosting).OrderByDescending(item => item["score"]).Take(count);

                return query.ToList().Select(doc => doc.GetItem()).ToList();
            }
        }

        protected virtual ISearchIndex GetSearchIndex() => ContentSearchManager.GetIndex((SitecoreIndexableItem)Context.Item);

        protected virtual IQueryable<T> ApplyFilterQuery(IQueryable<T> query) => query
            .Filter(item => item.Paths.Contains(ItemIDs.ContentRoot))
            .Filter(item => item.Language == Context.Language.Name);

        private IDictionary<string, float> GetTagsWeight()
        {
            var items = GetIdsFromCookie(Setting.CookieInfo.Name).Select(Context.Database.GetItem).Where(item => item != null);
            var tags = items.SelectMany(item => TagsResolver.GetItemTags(item, Setting)).Where(tag => !string.IsNullOrWhiteSpace(tag));
            var tagsWeight = new Dictionary<string, float>();
            foreach (var tag in tags)
            {
                if (tagsWeight.ContainsKey(tag))
                {
                    tagsWeight[tag] += 1.0f * Setting.WeightPerMatching;
                }
                else
                {
                    tagsWeight[tag] = 1.0f * Setting.WeightPerMatching;
                }
            }

            return tagsWeight;
        }

        private IList<ID> GetIdsFromCookie(string cookieName)
        {
            var cookieValue = HttpContext.Current.Request.Cookies[cookieName]?.Value ?? string.Empty;
            return cookieValue
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr => ID.Parse(idStr, ID.Null))
                .Where(id => !id.IsNull)
                .ToList();
        }
    }
}
