using EnterpriseRag.Api.Models;
using EnterpriseRag.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger<IngestionController> _logger;

        public IngestionController(
            IDocumentParserService parserService,
            IChunkingService chunkingService,
            ILogger<IngestionController> logger)
        {
            _parserService = parserService;
            _chunkingService = chunkingService;
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
    }
}
