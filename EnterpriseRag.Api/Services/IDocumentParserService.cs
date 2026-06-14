using System.IO;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public interface IDocumentParserService
    {
        Task<string> ExtractTextAsync(Stream fileStream);
    }
}
