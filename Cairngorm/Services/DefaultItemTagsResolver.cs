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
            foreach (var info in setting.TagResolverInfoList)
            {
                var fieldType = info.FieldType.ToLowerInvariant();
                if (fieldType == "text")
                {
                    var values = item[info.FieldName].Split(new[] { info.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    tags.AddRange(values);

                    continue;
                }

                if (fieldType == "link")
                {
                    var linkField = (LinkField)item.Fields[info.FieldName];
                    var value = linkField?.TargetItem[info.TargetField];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        tags.Add(value);
                    }

                    continue;
                }

                if (fieldType == "multilist")
                {
                    var multilistField = (MultilistField)item.Fields[info.FieldName];
                    var values = multilistField?.GetItems().Select(i=> i[info.TargetField]).Where(i => i != null);
                    if (tags != null)
                    {
                        tags.AddRange(values);
                    }

                    continue;
                }
            }

            return tags;
        }
    }
}
