namespace EnterpriseRag.Api.Config
{
    public class EmbeddingConfig
    {
        public const string SectionName = "Embedding";

        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "text-embedding-3-small";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    }
}
