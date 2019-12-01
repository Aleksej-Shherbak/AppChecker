using System.Collections.Generic;
using System.Threading.Tasks;
using MyAppChecker.Response;

namespace MyAppChecker.Services.Callback
{
    public interface ISendCallbackService
    {
        Task SendCallbackAsync(List<CheckPackageResponse> checkPackageResponses,
            string callbackUrl);

    }
}