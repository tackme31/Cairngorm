using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cairngorm.Services
{
    public class DefaultRecommender<T> : IRecommender where T : SearchResultItem
    {
        protected IItemTagsResolver TagsResolver { get; }

        public DefaultRecommender(IItemTagsResolver tagsResolver)
        {
            TagsResolver = tagsResolver;
        }

        public List<Item> GetRecommedations(int count = 10)
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

                // Template filtering
                if (!KnownSettings.SearchTemplates.Any())
                {
                    var templatesPred = KnownSettings.SearchTemplates.Aggregate(
                        PredicateBuilder.False<T>(),
                        (acc, id) => acc.Or(item => item.TemplateId == id));
                    query = query.Filter(templatesPred);
                }

                // Stored items filtering
                if (KnownSettings.FilterStoredItems)
                {
                    // Dedupe IDs
                    var itemIds = new HashSet<ID>();
                    GetIdsFromCookie().ForEach(id => itemIds.Add(id));

                    query = itemIds.Aggregate(query, (acc, id) => acc.Filter(item => item.ItemId != id));

                }

                // Context item filtering
                if (KnownSettings.FilterContextItem)
                {
                    query = query.Filter(item => item.ItemId != Context.Item.ID);
                }

                // Boosting predicate
                var tagsWeight = GetTagsWeight();
                var boosting = tagsWeight.Keys.Aggregate(
                    PredicateBuilder.Create<T>(item => item.Name.MatchWildcard("*").Boost(0.0f)),
                    (acc, tag) => acc.Or(item => item[KnownSettings.SearchField].Equals(tag).Boost(tagsWeight[tag])));

                return query
                    .Where(boosting)
                    .Take(count)
                    .OrderByDescending(item => item["score"])
                    .ToList() // remove
                    .Select(doc => doc.GetItem())
                    .ToList();
            }
        }

        protected virtual ISearchIndex GetSearchIndex() => ContentSearchManager.GetIndex((SitecoreIndexableItem)Context.Item);

        protected virtual IQueryable<T> ApplyFilterQuery(IQueryable<T> query) => query
            .Filter(item => item.Paths.Contains(ItemIDs.ContentRoot))
            .Filter(item => item.Language == Context.Language.Name);

        private IDictionary<string, float> GetTagsWeight()
        {
            var tags = GetIdsFromCookie()
                .Select(id => Context.Database.GetItem(id))
                .Where(item => item != null)
                .SelectMany(TagsResolver.GetItemTags)
                .Where(tag => !tag.IsNullOrEmpty());

            var tagsWeight = new Dictionary<string, float>();
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                if (tagsWeight.ContainsKey(tag))
                {
                    tagsWeight[tag] += 1.0f * KnownSettings.BoostMultiplicand;
                }
                else
                {
                    tagsWeight[tag] = 1.0f * KnownSettings.BoostMultiplicand;
                }
            }

            return tagsWeight;
        }

        private IList<ID> GetIdsFromCookie()
        {
            var cookieValue = HttpContext.Current.Request.Cookies[KnownSettings.Cookie.Name]?.Value ?? string.Empty;
            return cookieValue
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(idStr => ID.Parse(idStr, ID.Null))
                .Where(id => !id.IsNull)
                .ToList();
        }
    }
}
