using EnterpriseRag.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.ClientModel;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public class GroqEmbeddingService : IEmbeddingService
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<GroqEmbeddingService> _logger;
        private const string GroqEmbeddingModel = "nomic-embed-text-v1_5";

        public GroqEmbeddingService(
            IOptions<GroqConfig> config,
            ILogger<GroqEmbeddingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var apiKey = config?.Value?.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Groq API Key is not configured. Embeddings will fail.");
            }

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.groq.com/openai/v1")
            };

            _client = new OpenAIClient(new ApiKeyCredential(apiKey ?? string.Empty), options);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string textChunk)
        {
            if (string.IsNullOrWhiteSpace(textChunk))
            {
                return Array.Empty<float>();
            }

            try
            {
                var embeddingClient = _client.GetEmbeddingClient(GroqEmbeddingModel);
                ClientResult<OpenAIEmbedding> result = await embeddingClient.GenerateEmbeddingAsync(textChunk);
                
                var embedding = result?.Value?.ToFloats().ToArray();
                if (embedding == null || embedding.Length == 0)
                {
                    throw new InvalidOperationException("Failed to generate embedding from Groq.");
                }

                _logger.LogInformation("Successfully generated embedding with dimension {Dimension}.", embedding.Length);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during Groq embedding generation.");
                throw;
            }
        }
    }
}
