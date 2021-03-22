namespace Cairngorm.Services
{
    public interface IRecommenderFactory
    {
        IRecommender GetRecommender(string name);
    }
}
