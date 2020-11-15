using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Abstractions
{
    public abstract class RecommenderBase
    {
        public abstract List<Item> GetRecommedation(int count = 10);
    }
}
