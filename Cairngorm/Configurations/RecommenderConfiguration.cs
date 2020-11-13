using Sitecore.Configuration;
using Sitecore.Diagnostics;
using System.Collections.Generic;

namespace Cairngorm.Settings
{
    public class RecommenderConfiguration
    {
        private readonly IDictionary<string, RecommenderSetting> _settings;
        private readonly List<string> _settingNames;
        public IReadOnlyCollection<string> SettingNames => _settingNames;

        public static RecommenderConfiguration Create(bool assert = false)
        {
            return Factory.CreateObject("cairngorm/recommender", assert) as RecommenderConfiguration;
        }

        public RecommenderConfiguration()
        {
            _settings = new Dictionary<string, RecommenderSetting>();
            _settingNames = new List<string>();
        }

        public void AddRecommender(RecommenderSetting recommender)
        {
            Assert.ArgumentNotNull(recommender, nameof(recommender));

            _settings[recommender.Name] = recommender;
            _settingNames.Add(recommender.Name);
        }

        public RecommenderSetting GetSetting(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            return _settings.TryGetValue(name, out var value) ? value : null;
        }
    }
}
