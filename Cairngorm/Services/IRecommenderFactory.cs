using Cairngorm.Abstractions;

namespace Cairngorm.Services
{
    public interface IRecommenderFactory
    {
        RecommenderBase GetRecommender(string recommenderName);
    }
}
