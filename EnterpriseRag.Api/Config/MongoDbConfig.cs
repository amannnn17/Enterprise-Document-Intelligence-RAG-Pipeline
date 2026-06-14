namespace EnterpriseRag.Api.Config
{
    public class MongoDbConfig
    {
        public const string SectionName = "MongoDb";

        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "EnterpriseRagDb";
        public string CollectionName { get; set; } = "DocumentChunks";
    }
}
