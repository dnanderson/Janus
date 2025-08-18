using Microsoft.Extensions.Logging;

namespace Janus.Services
{
    [ProviderAlias("Observable")]
    public class ObservableLoggerProvider : ILoggerProvider
    {
        public IExternalScopeProvider ScopeProvider { get; } = new LoggerExternalScopeProvider();

        public ILogger CreateLogger(string categoryName)
        {
            return new ObservableLogger(categoryName, this);
        }

        public void Dispose()
        {
        }
    }
}
