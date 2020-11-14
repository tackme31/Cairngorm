using Cairngorm.Configurations;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cairngorm.Services
{
    public class DefaultItemTagsResolver : IItemTagsResolver
    {
        public List<string> GetItemTags(Item item, RecommenderSetting setting)
        {
            Assert.IsNotNull(item, nameof(item));

            var tags = new List<string>();
            foreach (var info in setting.TagsResolverInfoList)
            {
                var fieldType = info.FieldType.ToLowerInvariant();
                if (fieldType == "text")
                {
                    var values = SplitByDelimiter(item[info.FieldName], info.Delimiter);
                    tags.AddRange(values);

                    continue;
                }

                if (fieldType == "link")
                {
                    var linkField = (LinkField)item.Fields[info.FieldName];
                    var values = SplitByDelimiter(linkField?.TargetItem[info.TargetField], info.Delimiter);
                    tags.AddRange(values);

                    continue;
                }

                if (fieldType == "multilist")
                {
                    var multilistField = (MultilistField)item.Fields[info.FieldName];
                    var values = multilistField?.GetItems()
                        .SelectMany(i=> SplitByDelimiter(i[info.TargetField], info.Delimiter))
                        .Where(i => i != null);
                    if (tags != null)
                    {
                        tags.AddRange(values);
                    }

                    continue;
                }
            }

            return tags;

            string[] SplitByDelimiter(string str, char? delimiter)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new string[0];
                }

                if (delimiter == null)
                {
                    return new[] { str };
                }

                return str.Split(new[] { delimiter.Value }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
