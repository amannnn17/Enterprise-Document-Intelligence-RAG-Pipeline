using System;
using System.Collections.Generic;

namespace EnterpriseRag.Api.Services
{
    public class TokenSizeChunkingService : IChunkingService
    {
        public IEnumerable<string> GenerateChunks(string text, int chunkSize = 300, int chunkOverlap = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            // Split the text on whitespace characters.
            // This ensures we break on clean boundaries and keep punctuation attached to words.
            char[] delimiters = { ' ', '\r', '\n', '\t' };
            var words = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                yield break;
            }

            // Calculate how far to move forward for each new chunk
            int stepSize = Math.Max(1, chunkSize - chunkOverlap);

            for (int i = 0; i < words.Length; i += stepSize)
            {
                int takeCount = Math.Min(chunkSize, words.Length - i);
                
                // string.Join has an overload specifically for arrays with offset and count,
                // which is extremely efficient and avoids creating any intermediate segments or spans.
                yield return string.Join(" ", words, i, takeCount);

                // If the end of the current chunk reaches or exceeds the total words, stop yielding
                if (i + chunkSize >= words.Length)
                {
                    break;
                }
            }
        }
    }
}
