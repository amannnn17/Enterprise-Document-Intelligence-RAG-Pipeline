using EnterpriseRag.Api.Data;
using EnterpriseRag.Api.Models;
using EnterpriseRag.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Controllers
{
    [ApiController]
    [Route("api/v1/ingestion")]
    public class IngestionController : ControllerBase
    {
        private readonly IDocumentParserService _parserService;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<IngestionController> _logger;

        public IngestionController(
            IDocumentParserService parserService,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            MongoDbContext dbContext,
            ILogger<IngestionController> logger)
        {
            _parserService = parserService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("debug-chunk")]
        public async Task<IActionResult> DebugChunk(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file was uploaded or the file is empty." });
                }

                if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { error = "Only PDF files are supported." });
                }

                _logger.LogInformation("Processing file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

                // Open the stream directly from the IFormFile
                using var stream = file.OpenReadStream();
                
                // Phase 1: Extract Text
                var rawText = await _parserService.ExtractTextAsync(stream);

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    _logger.LogWarning("No extractable text was found in the provided PDF.");
                    return Ok(new { message = "No extractable text found in the PDF.", chunks = Array.Empty<TextChunkDto>() });
                }

                // Phase 2: Chunk the Text
                var stringChunks = _chunkingService.GenerateChunks(rawText).ToList();

                // Map the output to structured DTOs
                var structuredChunks = stringChunks.Select((text, index) => new TextChunkDto
                {
                    SequenceIndex = index,
                    Text = text,
                    // Calculate token count based on the same delimiter logic
                    TokenCount = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length
                }).ToList();

                _logger.LogInformation("Successfully processed PDF into {ChunkCount} chunks.", structuredChunks.Count);

                return Ok(new
                {
                    totalChunks = structuredChunks.Count,
                    chunks = structuredChunks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process PDF ingestion for chunking.");
                
                // Return a structured 500 error response
                return StatusCode(500, new 
                { 
                    error = "An unexpected error occurred while processing the file.", 
                    details = ex.Message 
                });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file was uploaded or the file is empty." });
                }

                if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { error = "Only PDF files are supported." });
                }

                _logger.LogInformation("Starting pipeline upload for file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

                // 1. Extract Text
                using var stream = file.OpenReadStream();
                var rawText = await _parserService.ExtractTextAsync(stream);

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    _logger.LogWarning("No extractable text was found in the provided PDF: {FileName}", file.FileName);
                    return BadRequest(new { error = "No extractable text found in the PDF." });
                }

                // 2. Chunk the Text
                var stringChunks = _chunkingService.GenerateChunks(rawText).ToList();
                if (stringChunks.Count == 0)
                {
                    _logger.LogWarning("Chunking service generated 0 chunks for: {FileName}", file.FileName);
                    return BadRequest(new { error = "Failed to split document text into chunks." });
                }

                // 3. Generate Embeddings for each chunk asynchronously
                var documentChunks = new List<DocumentChunk>();
                for (int i = 0; i < stringChunks.Count; i++)
                {
                    var textChunk = stringChunks[i];
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(textChunk);

                    var docChunk = new DocumentChunk
                    {
                        Id = MongoDB.Bson.ObjectId.GenerateNewId(),
                        SourceFile = file.FileName,
                        Content = textChunk,
                        SequenceIndex = i,
                        Embedding = embedding,
                        CreatedAt = DateTime.UtcNow
                    };
                    documentChunks.Add(docChunk);
                }

                // 4. Perform a bulk write operation (InsertManyAsync) to save all records to MongoDB
                _logger.LogInformation("Bulk inserting {Count} chunks into MongoDB for file {FileName}", documentChunks.Count, file.FileName);
                await _dbContext.DocumentChunks.InsertManyAsync(documentChunks);

                _logger.LogInformation("Successfully processed and persisted {FileName}", file.FileName);

                return Ok(new
                {
                    fileName = file.FileName,
                    totalChunksProcessed = documentChunks.Count,
                    status = "Successfully Persisted"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process PDF ingestion and storage pipeline.");
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred during pipeline processing.",
                    details = ex.Message
                });
            }
        }
    }
}
