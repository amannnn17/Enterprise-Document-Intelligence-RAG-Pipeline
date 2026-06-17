using EnterpriseRag.Api.Config;
using EnterpriseRag.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<List<DocumentChunk>> SearchSimilarChunksAsync(float[] queryVector, int limit = 3, string? documentName = null)
        {
            var vectorSearchArgs = new BsonDocument
            {
                { "index", "vector_index" },
                { "path", "Embedding" },
                { "queryVector", new BsonArray(queryVector) },
                { "numCandidates", 100 },
                { "limit", limit }
            };

            if (!string.IsNullOrWhiteSpace(documentName))
            {
                vectorSearchArgs.Add("filter", new BsonDocument("SourceFile", documentName));
            }

            var vectorSearchStage = new BsonDocument("$vectorSearch", vectorSearchArgs);

            var pipeline = new EmptyPipelineDefinition<DocumentChunk>()
                .AppendStage<DocumentChunk, DocumentChunk, DocumentChunk>(vectorSearchStage);

            return await DocumentChunks.Aggregate(pipeline).ToListAsync();
        }
    }
}
