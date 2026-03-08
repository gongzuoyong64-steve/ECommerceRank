using System.Formats.Asn1;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using StackExchange.Redis;

namespace ECommerceRanking.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private const string RankingKey = "product:ranking";

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _database.StringGetAsync(key);
        if (json.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(json!);
    }

    public async Task<double> IncrementProductScoreAsync(string productId, double score)
    {
        var tran = _database.CreateTransaction();

        var incrementTask = tran.SortedSetIncrementAsync(RankingKey, productId, score);
        _ = tran.KeyExpireAsync(RankingKey, TimeSpan.FromMinutes(15));

        await tran.ExecuteAsync();

        return await incrementTask;
    }

    public async Task<List<RankingItem>> GetRankingAsync(int startRank, int endRank, Order order = Order.Descending)
    {
        var redisOrder = order == Order.Descending ? StackExchange.Redis.Order.Descending : StackExchange.Redis.Order.Ascending;
        var entries = await _database.SortedSetRangeByRankWithScoresAsync(RankingKey, startRank, endRank, redisOrder);
        var result = new List<RankingItem>();
        foreach (var entry in entries)
        {
            result.Add(new RankingItem
            {
                ProductId = entry.Element.ToString(),
                Score = entry.Score,
                Rank = startRank + Array.IndexOf(entries, entry) + 1
            });
        }
        return result;
    }

    public async Task<long?> GetProductRankAsync(string productId, Order order = Order.Descending)
    {
        return await _database.SortedSetRankAsync(RankingKey, productId, (StackExchange.Redis.Order)(order == Order.Descending ? Order.Descending : Order.Ascending));
    }

    public async Task<double?> GetProductScoreAsync(string productId)
    {
        return await _database.SortedSetScoreAsync(RankingKey, productId);
    }

    public async Task<bool> ClearRankingAsync()
    {
        return await _database.KeyDeleteAsync(RankingKey);
    }

    public async Task<bool> UploadCSVAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        });

        var records = csv.GetRecords<dynamic>();
        var entries = new List<SortedSetEntry>();

        foreach (var record in records)
        {
            // 取欄位值
            var dict = record as IDictionary<string, object>;
            if (dict == null) continue;

            var key = dict.ElementAt(0).Value?.ToString();   // 第一欄作為 key
            var value = dict.ElementAt(1).Value?.ToString(); // 第二欄作為 value
            if (!string.IsNullOrEmpty(key) && double.TryParse(value, out double score))
            {
                entries.Add(new SortedSetEntry(key, score));
            }
            
        }

        if (entries.Count > 0)
        {
            var tran = _database.CreateTransaction();

            var addTask = tran.SortedSetAddAsync(RankingKey, entries.ToArray());
            int baseMinutes = 15;
            int jitter = TimeHelper.GenerateJitter();
            _ = tran.KeyExpireAsync(RankingKey, TimeSpan.FromMinutes(baseMinutes + jitter));
            await tran.ExecuteAsync();
            await addTask;
        }
        return true;
    }

}
