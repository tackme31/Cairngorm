using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Services
{
    public abstract class Recommender
    {
        public abstract List<Item> GetRecommedation(int count = 10);
    }
}
