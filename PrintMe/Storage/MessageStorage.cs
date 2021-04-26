using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintMe.Storage
{
    public class MessageStorage
    {
        private readonly RedisCacheClient _client;
        private readonly string _collName;

        public MessageStorage(IConfiguration configuration)
        {
            string instance = configuration.GetValue<string>("Instance");
            string connectionString = configuration.GetConnectionString("Redis");

            _collName = $"Messages_{instance}";

            _client = new RedisCacheClient(new SinglePool(connectionString),
                new NewtonsoftSerializer(new JsonSerializerSettings()),
                new RedisConfiguration() { ConnectionString = connectionString });
        }

        public Task AddMessage(string key, Message value)
        {
            return _client.Db0.HashSetAsync(_collName, key, value);
        }

        public Task RemoveMessage(string key)
        {
            return _client.Db0.HashDeleteAsync(_collName, key);
        }

        public Task<bool> HasMessage(string key)
        {
            return _client.Db0.HashExistsAsync(_collName, key);
        }

        public Task<Message> GetMessage(string key)
        {
            return _client.Db0.HashGetAsync<Message>(_collName, key);
        }

        public Task<IEnumerable<Message>> GetAllMessages()
        {
            return _client.Db0.HashValuesAsync<Message>(_collName);
        }

    }

    public class SinglePool : IRedisCacheConnectionPoolManager
    {
        private readonly IConnectionMultiplexer connection;

        public SinglePool(string connectionString)
        {
            var options = ConfigurationOptions.Parse(connectionString);

            connection = ConnectionMultiplexer.Connect(options);
        }

        public void Dispose()
        {
        }

        public IConnectionMultiplexer GetConnection()
        {
            return connection;
        }

        public ConnectionPoolInformation GetConnectionInformations()
        {
            return new ConnectionPoolInformation();
        }
    }
}
