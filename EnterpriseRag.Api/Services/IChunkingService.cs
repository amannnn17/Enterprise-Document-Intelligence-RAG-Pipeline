using System.Collections.Generic;

namespace EnterpriseRag.Api.Services
{
    public interface IChunkingService
    {
        IEnumerable<string> GenerateChunks(string text, int chunkSize = 300, int chunkOverlap = 50);
    }
}
