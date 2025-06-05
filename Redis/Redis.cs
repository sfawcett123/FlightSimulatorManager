using StackExchange.Redis;
using System.ComponentModel;
using System.Diagnostics;

namespace Redis
{
    public class Redis
    {
        ConnectionMultiplexer redis;
        IDatabase db;

        public Redis( string server , int port)
        {
            redis = ConnectionMultiplexer.Connect( server );
            db = redis.GetDatabase();

            db.StringSet("foo", "Test Data");
            Debug.WriteLine(db.StringGet("foo"));
        }

        public void write( string key , string value )
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                Debug.WriteLine("Key or value is null or empty.");
                return;
            }
            db.StringSet(key, value);
            Debug.WriteLine($"Written to Redis: {key} = {value}");
        }

    }
}
