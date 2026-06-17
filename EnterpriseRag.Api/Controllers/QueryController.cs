using EnterpriseRag.Api.Data;
using EnterpriseRag.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Controllers
{
    [ApiController]
    [Route("api/v1/query")]
    public class QueryController : ControllerBase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly MongoDbContext _dbContext;
        private readonly ILlmService _llmService;
        private readonly ILogger<QueryController> _logger;

        public QueryController(
            IEmbeddingService embeddingService,
            MongoDbContext dbContext,
            ILlmService llmService,
            ILogger<QueryController> logger)
        {
            _embeddingService = embeddingService;
            _dbContext = dbContext;
            _llmService = llmService;
            _logger = logger;
        }

        public class SearchRequest
        {
            public string UserQuestion { get; set; } = string.Empty;
            public string? DocumentName { get; set; }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserQuestion))
                {
                    return BadRequest(new { error = "UserQuestion cannot be empty." });
                }

                _logger.LogInformation("Generating embedding for user question.");
                
                // (A) Pass the question to IEmbeddingService to get the float array
                var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.UserQuestion);

                if (queryVector == null || queryVector.Length == 0)
                {
                    return StatusCode(500, new { error = "Failed to generate embedding for the question." });
                }

                _logger.LogInformation("Searching MongoDB for similar chunks.");
                
                // (B) Pass that float array to MongoDbContext.SearchSimilarChunksAsync
                var similarChunks = await _dbContext.SearchSimilarChunksAsync(queryVector, limit: 3, documentName: request.DocumentName);

                // (C) Generate answer and return the payload
                var results = similarChunks.Select(chunk => new
                {
                    sourceFile = chunk.SourceFile,
                    sequenceIndex = chunk.SequenceIndex,
                    content = chunk.Content
                }).ToList();

                _logger.LogInformation("Generating answer using Groq LLM.");
                var contextStrings = similarChunks.Select(c => c.Content).ToList();
                var answer = await _llmService.GenerateAnswerAsync(request.UserQuestion, contextStrings);

                return Ok(new
                {
                    question = request.UserQuestion,
                    answer = answer,
                    sources = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during vector search.");
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred during search.",
                    details = ex.Message
                });
            }
        }
    }
}
