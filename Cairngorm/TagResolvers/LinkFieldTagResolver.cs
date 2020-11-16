using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Cairngorm.TagResolvers
{
    public class LinkFieldTagResolver : TagResolverBase
    {
        protected string FieldName { get; }
        protected string TargetField { get; }
        protected char? Delimiter { get; }

        public LinkFieldTagResolver(XmlNode node) : base(node)
        {
            var delimiterAttr = XmlUtil.GetAttribute("delimiter", node);
            FieldName = XmlUtil.GetAttribute("fieldName", node);
            Delimiter = string.IsNullOrEmpty(delimiterAttr) ? default(char?) : delimiterAttr[0];
            TargetField = XmlUtil.GetAttribute("TargetField", node);
        }

        public override List<string> GetItemTags(Item item)
        {
            var linkField = (LinkField)item.Fields[FieldName];
            if (linkField?.TargetItem == null)
            {
                return new List<string>();
            }

            var value = linkField.TargetItem[TargetField];
            if (!Delimiter.HasValue)
            {
                return new List<string> { value };
            }

            return value.Split(new[] { Delimiter.Value }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}