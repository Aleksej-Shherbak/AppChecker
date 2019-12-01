using System.Collections.Generic;
using System.Threading.Tasks;
using MyAppChecker.Response;

namespace MyAppChecker.Services.CheckApp
{
    public interface ICheckPackagesService
    {
        Task<List<CheckPackageResponse>> CheckAsync(List<string> appIds);
    }
}