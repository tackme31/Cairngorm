using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cairngorm.Services
{
    public class DefaultItemTagsResolver : IItemTagsResolver
    {
        public List<string> GetItemTags(Item item)
        {
            Assert.IsNotNull(item, nameof(item));

            return item["Tags"].Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
