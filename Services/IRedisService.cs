namespace ECommerceRanking.Services;

public interface IRedisService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<double> IncrementProductScoreAsync(string productId, double score);
    Task<List<RankingItem>> GetRankingAsync(int startRank, int endRank, Order order = Order.Descending);
    Task<long?> GetProductRankAsync(string productId, Order order = Order.Descending);
    Task<double?> GetProductScoreAsync(string productId);
    Task<bool> ClearRankingAsync();
    Task<bool> UploadCSVAsync(IFormFile file);
}

public enum Order
{
    Ascending,
    Descending
}

public class RankingItem
{
    public string ProductId { get; set; } = string.Empty;
    public double Score { get; set; }
    public long Rank { get; set; }
}
