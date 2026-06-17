using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseRag.Api.Services
{
    public interface ILlmService
    {
        Task<string> GenerateAnswerAsync(string userQuestion, List<string> retrievedContext);
    }
}
