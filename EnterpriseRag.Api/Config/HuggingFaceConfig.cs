namespace EnterpriseRag.Api.Config
{
    public class HuggingFaceConfig
    {
        public const string SectionName = "HuggingFace";
        public string ApiToken { get; set; } = string.Empty;
    }
}
