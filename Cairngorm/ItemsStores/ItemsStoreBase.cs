using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.ItemsStores
{
    public abstract class ItemsStoreBase
    {
        public abstract void AddItem(Item item);
        public abstract List<Item> GetItems();
    }
}
