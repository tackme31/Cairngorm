using Cairngorm.TagResolvers;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Cairngorm.Configurations
{
    public class RecommenderSetting
    {
        public RecommenderSetting(string name)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));

            Name = name;
            TagResolvers = new List<TagResolverBase>();
        }

        public string Name { get; }
        public string SearchField { get; set; } = "_content";
        public List<ID> SearchTemplates { get; set; } = new List<ID>();
        public int StoredItemsCount { get; set; } = 10;
        public float WeightPerMatching { get; set; } = 1.0f;
        public bool BoostGradually { get; set; } = false;
        public bool FilterStoredItems { get; set; } = false;
        public bool FilterContextItem { get; set; } = true;
        public string SearchScope { get; set; } = string.Empty;
        public CookieInfo CookieInfo { get; set; } = new CookieInfo("recommend_items");
        public List<TagResolverBase> TagResolvers { get; }

        public void AddSearchTemplate(XmlNode node)
        {
            Assert.ArgumentNotNull(node, nameof(node));

            var templateId = ID.Parse(node.InnerText);

            SearchTemplates.Add(templateId);
        }

        public void AddTagResolver(XmlNode node)
        {
            Assert.ArgumentNotNull(node, nameof(node));

            var typeName = XmlUtil.GetAttribute("type", node);
            var type = Type.GetType(typeName);
            var instance = (TagResolverBase)Activator.CreateInstance(type, node);

            TagResolvers.Add(instance);
        }
    }
}
