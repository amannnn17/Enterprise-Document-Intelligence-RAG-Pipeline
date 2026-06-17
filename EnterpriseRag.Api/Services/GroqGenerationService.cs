using EnterpriseRag.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ClientModel;

namespace EnterpriseRag.Api.Services
{
    public class GroqGenerationService : ILlmService
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<GroqGenerationService> _logger;
        private const string GroqModel = "llama-3.1-8b-instant";

        public GroqGenerationService(
            IOptions<GroqConfig> config,
            ILogger<GroqGenerationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            var apiKey = config?.Value?.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Groq API Key is not configured. LLM generation will fail.");
            }

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.groq.com/openai/v1")
            };

            // If apiKey is null or empty, OpenAIClient may throw depending on version, 
            // but we provide a fallback empty string to avoid immediate crash at startup.
            _client = new OpenAIClient(new ApiKeyCredential(apiKey ?? string.Empty), options);
        }

        public async Task<string> GenerateAnswerAsync(string userQuestion, List<string> retrievedContext)
        {
            if (string.IsNullOrWhiteSpace(userQuestion))
            {
                return "Please provide a question.";
            }

            string joinedContext = retrievedContext != null && retrievedContext.Count > 0
                ? string.Join("\n\n", retrievedContext)
                : "No context available.";

            string systemPrompt = $"You are an enterprise AI assistant. Answer the user's question using strictly the provided context. If the answer is not in the context, say 'I do not know.' Context: {joinedContext}";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userQuestion)
            };

            try
            {
                var chatClient = _client.GetChatClient(GroqModel);
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages);

                if (completion?.Content != null && completion.Content.Count > 0)
                {
                    return completion.Content[0].Text;
                }

                return "No response generated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating answer with Groq.");
                throw;
            }
        }
    }
}
