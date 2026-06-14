using EnterpriseRag.Api.Config;
using EnterpriseRag.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace EnterpriseRag.Api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly string _collectionName;

        public MongoDbContext(IMongoClient mongoClient, IOptions<MongoDbConfig> config)
        {
            if (mongoClient == null) throw new ArgumentNullException(nameof(mongoClient));
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            var settings = config.Value;

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
            {
                throw new ArgumentException("MongoDB database name is not configured.", nameof(config));
            }

            _database = mongoClient.GetDatabase(settings.DatabaseName);
            _collectionName = string.IsNullOrWhiteSpace(settings.CollectionName) ? "DocumentChunks" : settings.CollectionName;
        }

        public IMongoCollection<DocumentChunk> DocumentChunks => 
            _database.GetCollection<DocumentChunk>(_collectionName);
    }
}
