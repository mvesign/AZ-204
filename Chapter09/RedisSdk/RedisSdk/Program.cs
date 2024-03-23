using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisSdk;

public static class Program
{
    private const string EndPoint = "[NAME-OF-RESOURCE].redis.cache.windows.net";
    private const string Password = "[PRIMARY OR SECONDARY KEY]";
    private static IDatabase _cache;

    public static async Task Main(string[] args)
    {
        var config = new ConfigurationOptions
        {
            Password = Password,
            Ssl = true
        };
        config.EndPoints.Add(EndPoint);

        var redisHostConnection = ConnectionMultiplexer.Connect(config);

        _cache = redisHostConnection.GetDatabase();

        await PlayWithStringItem();
        await PlayWithIntItem();
        await PlayWithListItem();
        await PlayWithSetItem();
        await PlayWithHashItem();

        _ = await GeneralCachingApproach(0);
    }

    private static async Task PlayWithStringItem()
    {
        var key = new RedisKey("Msg");
        var value = new RedisValue("Hello");

        await _cache
            .StringSetAsync(key, value)
            .ContinueWith(a => Console.WriteLine($"Create {key} = {value}"));
        await _cache
            .StringAppendAsync(key, ", world!")
            .ContinueWith(a => Console.WriteLine($"append {key}"));
        await _cache
            .StringGetAsync(key)
            .ContinueWith(value => Console.WriteLine($"Now {key} is {value.Result}"));
        _ = await _cache.KeyDeleteAsync(key);

        Console.WriteLine("-------------------------------------------------");
    }

    private static async Task PlayWithIntItem()
    {
        var key = new RedisKey("Counter");
        var value = new RedisValue("1");

        await _cache
            .StringSetAsync(key, value, TimeSpan.FromMinutes(1))
            .ContinueWith(a => Console.WriteLine($"Create {key} = {value}"));
        await _cache
            .StringIncrementAsync(key)
            .ContinueWith(a => Console.WriteLine($"Increment {key}"));
        await _cache
            .StringGetAsync(key)
            .ContinueWith(value => Console.WriteLine($"Now {key} is {value.Result}"));

        Console.WriteLine("-------------------------------------------------");
    }

    private static async Task PlayWithListItem()
    {
        var key = new RedisKey("WeekDays");
        var value1 = new RedisValue("Monday");
        var value2 = new RedisValue("Tuesday");

        await _cache
            .ListLeftPushAsync(key, value1)
            .ContinueWith(a => Console.WriteLine($"Add {value1}"));
        await _cache
            .ListLeftPushAsync(key, value2)
            .ContinueWith(a => Console.WriteLine($"Add {value2}"));
        await _cache
            .ListLeftPushAsync(key, value1)
            .ContinueWith(a => Console.WriteLine($"Add {value1}"));
        await _cache
            .ListLeftPushAsync(key, value2)
            .ContinueWith(a => Console.WriteLine($"Add {value2}"));
        await _cache
            .ListRangeAsync(key)
            .ContinueWith(r =>
            {
                Console.WriteLine($"Now LIST contains:");
                foreach (var value in r.Result)
                    Console.WriteLine($"\t{value}");
            });
        await _cache
            .ListLeftPopAsync(key)
            .ContinueWith(a => Console.WriteLine($"Lpop value: {value2}"));
        _ = await _cache.KeyDeleteAsync(key);

        Console.WriteLine("-------------------------------------------------");
    }

    private static async Task PlayWithSetItem()
    {
        var key = new RedisKey("WeekDays");
        var value1 = new RedisValue("Monday");
        var value2 = new RedisValue("Tuesday");
        var value3 = new RedisValue("Wednesday");

        await _cache
            .SetAddAsync(key, value1)
            .ContinueWith(a => Console.WriteLine($"Add {value1}"));
        await _cache
            .SetAddAsync(key, value2)
            .ContinueWith(a => Console.WriteLine($"Add {value2}"));
        await _cache
            .SetAddAsync(key, value3)
            .ContinueWith(a => Console.WriteLine($"Add {value3}"));
        await _cache
            .SetAddAsync(key, value1)
            .ContinueWith(a => Console.WriteLine($"Add another {value1}")); //add value1 again
        await _cache
            .SetMembersAsync(key)
            .ContinueWith(r =>
            {
                Console.WriteLine($"Now SET contains:");
                foreach (var value in r.Result)
                    Console.WriteLine($"\t{value}");
            });
        await _cache
            .SetRandomMemberAsync(key)
            .ContinueWith(a => Console.WriteLine($"Random member of set: {a.Result}"));
        _ = await _cache.KeyDeleteAsync(key);

        Console.WriteLine("-------------------------------------------------");
    }

    private static async Task PlayWithHashItem()
    {
        var key = new RedisKey("client-hash");
        var values = new HashEntry[] {
            new(new RedisValue("ID"), new RedisValue("777")),
            new(new RedisValue("Name"), new RedisValue("TheCloudShops")),
        };

        await _cache
            .HashSetAsync(key, values)
            .ContinueWith(a => Console.WriteLine($"Set Hash"));
        await _cache
            .HashGetAllAsync(key)
            .ContinueWith(r =>
            {
                Console.WriteLine($"Now HASH contains:");

                foreach (var value in r.Result)
                    Console.WriteLine($"\t{value.Name}-{value.Value}");
            });
        _ = await _cache.KeyDeleteAsync(key);

        Console.WriteLine("-------------------------------------------------");
    }

    private static async Task<Client> GeneralCachingApproach(int ClientId)
    {
        var theKey = new RedisKey($"client:{ClientId}");

        if (await _cache.KeyExistsAsync(theKey))
        {
            var hash = await _cache.HashGetAllAsync(theKey);
            Console.WriteLine("General approach, load from cache");

            return new Client(
                hash.First(k => k.Name == "Name").Value,
                int.Parse(hash.First(k => k.Name == "ID").Value)
            );
        }
        else
        {
            var client = LoadClientFromDB();
            Console.WriteLine("General approach, load DB");

            await _cache.HashSetAsync(
                theKey,
                [
                    new(new RedisValue( "ID" ), new RedisValue( client.ID.ToString() )),
                    new(new RedisValue( "Name" ), new RedisValue( client.Name )),
                ]
             );

            _ = await _cache.KeyExpireAsync(theKey, TimeSpan.FromMinutes(1));

            Console.WriteLine("General approach, put in cache");
            return client;
        }

        static Client LoadClientFromDB()
            => new("TheCloudShops", 0);
    }
}

public record Client(string? Name, int ID)
{
    public override string ToString()
        => $"Client {ID}:{Name}";
}