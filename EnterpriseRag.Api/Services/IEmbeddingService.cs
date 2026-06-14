using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string textChunk);
    }
}
