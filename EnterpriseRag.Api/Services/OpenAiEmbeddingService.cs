using EnterpriseRag.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public class OpenAiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly EmbeddingConfig _config;
        private readonly ILogger<OpenAiEmbeddingService> _logger;

        public OpenAiEmbeddingService(
            HttpClient httpClient,
            IOptions<EmbeddingConfig> config,
            ILogger<OpenAiEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                _logger.LogWarning("Embedding API Key is not configured. External embedding requests will fail.");
            }

            var baseUrl = _config.BaseUrl;
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string textChunk)
        {
            if (string.IsNullOrWhiteSpace(textChunk))
            {
                _logger.LogWarning("Attempted to generate embedding for empty text chunk.");
                return Array.Empty<float>();
            }

            _logger.LogInformation("Generating embedding for chunk. Length: {Length} characters.", textChunk.Length);

            var requestBody = new OpenAiEmbeddingRequest
            {
                Input = textChunk,
                Model = _config.ModelName
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("embeddings", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI embedding API call failed with status: {StatusCode}. Details: {Details}", 
                        response.StatusCode, errorDetails);
                    throw new HttpRequestException($"OpenAI embedding API returned status code {response.StatusCode}. Details: {errorDetails}");
                }

                var result = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>();
                var embedding = result?.Data?[0]?.Embedding;

                if (embedding == null)
                {
                    _logger.LogError("OpenAI embedding API returned an empty or invalid response structure.");
                    throw new InvalidOperationException("Failed to retrieve embedding array from OpenAI response.");
                }

                _logger.LogInformation("Successfully generated embedding with dimension {Dimension}.", embedding.Length);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during embedding generation.");
                throw;
            }
        }

        private class OpenAiEmbeddingRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;
        }

        private class OpenAiEmbeddingResponse
        {
            [JsonPropertyName("data")]
            public OpenAiEmbeddingData[]? Data { get; set; }
        }

        private class OpenAiEmbeddingData
        {
            [JsonPropertyName("embedding")]
            public float[]? Embedding { get; set; }
        }
    }
}
