using System.Threading.Tasks;
using MyAppChecker.Request;

namespace MyAppChecker.Services.Jobs
{
    public interface IRevisionJob
    {
        Task StartRevision(CheckRequest checkRequest);
    }
}