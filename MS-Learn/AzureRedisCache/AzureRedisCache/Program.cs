using StackExchange.Redis;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AzureRedisCache;

public static class Program
{
    private static readonly TimeSpan _timeToLive = TimeSpan.FromSeconds(10);    

    private const string RedisConnectionString =
        "mjoy-redis.redis.cache.windows.net:6380,password=[ACCESS-KEY],ssl=True,abortConnect=False";
    private const string RedisKey = "test:key";

    private static async Task Main(string[] args)
    {
        using var cache = ConnectionMultiplexer.Connect(RedisConnectionString);
        var database = cache.GetDatabase();

        var result = await database.ExecuteAsync("ping");
        Console.WriteLine($"PING = {result.Resp3Type} : {result}");
        
        var setValue = await database.StringSetAsync(RedisKey, "100", _timeToLive);
        Console.WriteLine($"SET: {setValue}");

        await GetStringValue(RedisKey);
        
        Thread.Sleep(_timeToLive);
        
        await GetStringValue(RedisKey);

        async Task GetStringValue(string key)
        {
            var getValue = await database.StringGetAsync(key);
            Console.WriteLine($"GET: {getValue}");
        }
    }
}