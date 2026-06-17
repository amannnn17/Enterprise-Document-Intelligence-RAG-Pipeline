using EnterpriseRag.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public class HuggingFaceEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HuggingFaceEmbeddingService> _logger;
        private const string Model = "sentence-transformers/all-MiniLM-L6-v2";

        public HuggingFaceEmbeddingService(
            HttpClient httpClient,
            IOptions<HuggingFaceConfig> config,
            ILogger<HuggingFaceEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var token = config?.Value?.ApiToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("HuggingFace API Token is not configured. Embeddings will fail.");
            }

            _httpClient.BaseAddress = new Uri($"https://router.huggingface.co/hf-inference/models/{Model}/pipeline/feature-extraction");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string textChunk)
        {
            if (string.IsNullOrWhiteSpace(textChunk))
            {
                return Array.Empty<float>();
            }

            try
            {
                var requestBody = new HfRequest { Inputs = textChunk };
                var response = await _httpClient.PostAsJsonAsync("", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError("HuggingFace API call failed with status: {StatusCode}. Details: {Details}",
                        response.StatusCode, errorDetails);
                    throw new HttpRequestException($"HuggingFace API returned status code {response.StatusCode}. Details: {errorDetails}");
                }

                // HuggingFace feature-extraction returns a float[] directly for single string input
                var embedding = await response.Content.ReadFromJsonAsync<float[]>();

                if (embedding == null || embedding.Length == 0)
                {
                    throw new InvalidOperationException("HuggingFace returned an empty embedding.");
                }

                _logger.LogInformation("Successfully generated embedding with dimension {Dimension}.", embedding.Length);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during HuggingFace embedding generation.");
                throw;
            }
        }

        private class HfRequest
        {
            [JsonPropertyName("inputs")]
            public string Inputs { get; set; } = string.Empty;
        }
    }
}
