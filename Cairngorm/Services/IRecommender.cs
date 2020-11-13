using Sitecore.Data.Items;
using System.Collections.Generic;

namespace Cairngorm.Services
{
    public interface IRecommender
    {
        List<Item> GetRecommedations(string recommenderName, int count);
    }
}
