namespace EnterpriseRag.Api.Models
{
    public class TextChunkDto
    {
        public int SequenceIndex { get; set; }
        public string Text { get; set; } = string.Empty;
        public int TokenCount { get; set; }
    }
}
