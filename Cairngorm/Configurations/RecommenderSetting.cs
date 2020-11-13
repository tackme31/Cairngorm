using Sitecore.Data;
using Sitecore.Diagnostics;
using System.Collections.Generic;

namespace Cairngorm.Settings
{
    public class RecommenderSetting
    {
        public RecommenderSetting(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            Name = name;
        }

        public string Name { get; }
        public string SearchField { get; set; }
        public List<ID> SearchTemplates { get; set; }
        public int StoredItemsCount { get; set; }
        public float BoostMultiplicand { get; set; }
        public bool FilterStoredItems { get; set; }
        public bool FilterContextItem { get; set; }
        public CookieInfo CookieInfo { get; set; }
    }
}
