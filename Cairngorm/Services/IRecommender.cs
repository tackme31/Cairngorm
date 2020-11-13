using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Services
{
    public interface IRecommender
    {
        List<Item> GetRecommedations(int count);
    }
}
