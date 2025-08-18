using System.Threading;
using System.Threading.Tasks;
using Janus.ViewModels;

namespace Janus.Services
{
    public interface ITestRunnerService
    {
        Task RunTestAsync(UutViewModel uutViewModel, CancellationToken cancellationToken);
    }
}
