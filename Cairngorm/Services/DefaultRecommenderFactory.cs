using Cairngorm.Abstractions;
using Cairngorm.Configurations;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Diagnostics;
using System;

namespace Cairngorm.Services
{
    public class DefaultRecommenderFactory<T> : IRecommenderFactory where T : SearchResultItem
    {
        protected IItemTagsResolver ItemTagsResolver { get; }

        public DefaultRecommenderFactory(IItemTagsResolver itemTagsResolver)
        {
            ItemTagsResolver = itemTagsResolver;
        }

        public virtual Recommender GetRecommender(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            var setting = GetRecommenderSetting(name);
            if (setting == null)
            {
                throw new ArgumentException($"The specific recommender does not exist. (Name: {name})");
            }

            return new CairngormRecommender<T>(setting, ItemTagsResolver);
        }

        protected RecommenderSetting GetRecommenderSetting(string name)
        {
            var config = RecommenderConfiguration.Create(assert: true);

            return config.GetSetting(name); ;
        }
    }
}
