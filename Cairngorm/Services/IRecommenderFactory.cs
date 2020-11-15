namespace Cairngorm.Services
{
    public interface IRecommenderFactory
    {
        Recommender GetRecommender(string recommenderName);
    }
}
