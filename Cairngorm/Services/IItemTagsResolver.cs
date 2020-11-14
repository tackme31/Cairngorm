using Cairngorm.Configurations;
using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Services
{
    public interface IItemTagsResolver
    {
        List<string> GetItemTags(Item item, RecommenderSetting setting);
    }
}
