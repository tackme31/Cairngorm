using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Services
{
    public interface IRecommendationService
    {
        List<Item> GetRecommedations(int count);
    }
}
