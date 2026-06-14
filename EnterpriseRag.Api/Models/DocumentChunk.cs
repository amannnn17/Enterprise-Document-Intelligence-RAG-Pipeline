using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EnterpriseRag.Api.Models
{
    public class DocumentChunk
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string SourceFile { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int SequenceIndex { get; set; }

        public float[] Embedding { get; set; } = Array.Empty<float>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
