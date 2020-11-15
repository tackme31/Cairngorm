using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Cairngorm.TagResolvers
{
    public class MultilistFieldTagResolver : TagResolverBase
    {
        protected string FieldName { get; }
        protected string TargetField { get; }
        protected char? Delimiter { get; }

        public MultilistFieldTagResolver(XmlNode node) : base(node)
        {
            var delimiterAttr = XmlUtil.GetAttribute("delimiter", node);
            FieldName = XmlUtil.GetAttribute("fieldName", node);
            Delimiter = string.IsNullOrEmpty(delimiterAttr) ? default(char?) : delimiterAttr[0];
            TargetField = XmlUtil.GetAttribute("TargetField", node);
        }

        public override List<string> GetItemTags(Item item)
        {
            var multilistField = (MultilistField)item.Fields[FieldName];
            var targetItems = multilistField?.GetItems();
            if (targetItems == null)
            {
                return new List<string>();
            }

            var values = targetItems.Select(i => i[TargetField]);
            if (!Delimiter.HasValue)
            {
                return values.ToList();
            }

            return values.SelectMany(value => value.Split(new[] { Delimiter.Value }, StringSplitOptions.RemoveEmptyEntries)).ToList();
        }
    }
}
