using Sitecore.Data.Items;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Cairngorm.TagResolvers
{
    public class TextFieldTagResolver : TagResolverBase
    {
        protected string FieldName { get; }
        protected char? Delimiter { get; }

        public TextFieldTagResolver(XmlNode node) : base(node)
        {
            var delimiterAttr = XmlUtil.GetAttribute("delimiter", node);
            FieldName = XmlUtil.GetAttribute("fieldName", node);
            Delimiter = string.IsNullOrEmpty(delimiterAttr) ? default(char?) : delimiterAttr[0];
        }

        public override List<string> GetItemTags(Item item)
        {
            if (!Delimiter.HasValue)
            {
                return new List<string> { item[FieldName] };
            }

            return item[FieldName].Split(new[] { Delimiter.Value }, StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Trim()).ToList();
        }
    }
}
