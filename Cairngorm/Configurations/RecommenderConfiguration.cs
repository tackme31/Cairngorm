using Sitecore.Configuration;
using Sitecore.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Cairngorm.Configurations
{
    public class RecommenderConfiguration
    {
        private readonly IDictionary<string, RecommenderSetting> _settings;
        public IReadOnlyCollection<string> SettingNames => _settings.Keys.ToList();

        public static RecommenderConfiguration Create(bool assert = false)
        {
            return Factory.CreateObject("cairngorm/configuration", assert) as RecommenderConfiguration;
        }

        public RecommenderConfiguration()
        {
            _settings = new Dictionary<string, RecommenderSetting>();
        }

        public void AddRecommender(RecommenderSetting recommender)
        {
            Assert.ArgumentNotNull(recommender, nameof(recommender));

            _settings[recommender.Name] = recommender;
        }

        public RecommenderSetting GetSetting(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            return _settings.TryGetValue(name, out var value) ? value : null;
        }
    }
}
