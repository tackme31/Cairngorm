using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Xml;
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
            TagResolverInfoList = new List<TagResolverInfo>();
        }

        public string Name { get; }
        public string SearchField { get; set; } = "_content";
        public List<ID> SearchTemplates { get; set; } = new List<ID>();
        public int StoredItemsCount { get; set; } = 10;
        public float WeightPerMatching { get; set; } = 1.0f;
        public bool FilterStoredItems { get; set; } = false;
        public bool FilterContextItem { get; set; } = true;
        public string SearchScope { get; set; } = string.Empty;
        public CookieInfo CookieInfo { get; set; } = new CookieInfo("cairngorm");
        public List<TagResolverInfo> TagResolverInfoList { get; }

        public void AddSearchTemplate(XmlNode node)
        {
            Assert.ArgumentNotNull(node, nameof(node));

            var templateId = ID.Parse(node.InnerText);

            SearchTemplates.Add(templateId);
        }

        public void AddTagResolverInfo(XmlNode node)
        {
            Assert.ArgumentNotNull(node, nameof(node));

            var fieldName = XmlUtil.GetAttribute("fieldName", node)?.ToLowerInvariant();
            var fieldType = XmlUtil.GetAttribute("fieldType", node)?.ToLowerInvariant();
            var delimiter = XmlUtil.GetAttribute("delimiter", node)?.ToLowerInvariant();
            var targetField = XmlUtil.GetAttribute("targetField", node)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new System.ArgumentException("The 'fieldName' attribute is required.", "fieldName");
            }

            if (string.IsNullOrWhiteSpace(fieldType))
            {
                throw new System.ArgumentException("The 'fieldType' attribute is required.", "fieldName");
            }

            var info = new TagResolverInfo()
            {
                FieldName = fieldName,
                FieldType = fieldType,
                Delimiter = string.IsNullOrEmpty(delimiter) ? default(char?) : delimiter[0],
                TargetField = targetField,
            };

            TagResolverInfoList.Add(info);
        }
    }
}
