using Sitecore.Data;
using Sitecore.Diagnostics;
using System.Collections.Generic;

namespace Cairngorm.Configurations
{
    public class RecommenderSetting
    {
        public RecommenderSetting(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            Name = name;
        }

        public string Name { get; }
        public string SearchField { get; set; } = "_content";
        public List<ID> SearchTemplates { get; set; } = new List<ID>();
        public int StoredItemsCount { get; set; } = 10;
        public float BoostMultiplicand { get; set; } = 1.0f;
        public bool FilterStoredItems { get; set; } = false;
        public bool FilterContextItem { get; set; } = true;
        public CookieInfo CookieInfo { get; set; } = new CookieInfo("cairngorm");
    }
}
