using StackExchange.Redis;
using System.ComponentModel;
using System.Diagnostics;

namespace Redis
{
    public class Redis
    {
        private ConnectionMultiplexer redis;
        private IDatabase db;
        private string server;
        private int port;

        public Redis( string server , int port)
        {
            this.server = server;
            this.port = port;

            connectToRedis();
        }

        public void write( string key , string value )
        {
            if (redis == null || db == null)
            {
                connectToRedis();
            }

            if (redis == null || db == null)
            {
                Debug.WriteLine("Failed to connect to Redis server.");
                return;
            }

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                Debug.WriteLine("Key or value is null or empty.");
                return;
            }
            db.StringSet(key, value);
            Debug.WriteLine($"Written to Redis: {key} = {value}");
        }

        #region Private Methods
        private void connectToRedis()
        {
            try
            {
                redis = ConnectionMultiplexer.Connect($"{server}:{port}");
                db = redis.GetDatabase();
                Debug.WriteLine("Connected to Redis server.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting to Redis: {ex.Message}");
            }
        }   

        #endregion

    }
}
