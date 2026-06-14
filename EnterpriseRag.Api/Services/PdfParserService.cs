using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace EnterpriseRag.Api.Services
{
    public class PdfParserService : IDocumentParserService
    {
        private readonly ILogger<PdfParserService> _logger;

        public PdfParserService(ILogger<PdfParserService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            _logger.LogInformation("Starting PDF text extraction.");

            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Provided file stream is null or empty. Skipping extraction.");
                return string.Empty;
            }

            try
            {
                var textBuilder = new StringBuilder();

                // UglyToad.PdfPig's PdfDocument.Open is synchronous but heavily CPU-bound.
                // We wrap it in Task.Run to keep the async method truly non-blocking.
                return await Task.Run(() => 
                {
                    using (var document = PdfDocument.Open(fileStream))
                    {
                        var pageCount = document.NumberOfPages;
                        _logger.LogInformation("PDF loaded successfully with {PageCount} pages.", pageCount);

                        for (var i = 1; i <= pageCount; i++)
                        {
                            var page = document.GetPage(i);
                            var text = page.Text;

                            if (string.IsNullOrWhiteSpace(text))
                            {
                                _logger.LogDebug("Page {PageNumber} is empty or contains no extractable text.", i);
                                continue;
                            }

                            textBuilder.AppendLine(text);
                        }
                    }

                    var finalString = textBuilder.ToString();
                    _logger.LogInformation("PDF text extraction completed successfully. Extracted {CharacterCount} characters.", finalString.Length);
                    
                    return finalString;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during PDF text extraction.");
                throw;
            }
        }
    }
}
