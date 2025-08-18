using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Janus.Services
{
    public class ObservableLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ObservableLoggerProvider _provider;

        public static event EventHandler<LogEventArgs>? LogReceived;

        public ObservableLogger(string categoryName, ObservableLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return _provider.ScopeProvider.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var correlationId = GetCorrelationIdFromScope();
            LogReceived?.Invoke(this, new LogEventArgs(correlationId, message));
        }

        private Guid GetCorrelationIdFromScope()
        {
            var scopeProvider = _provider.ScopeProvider;
            var correlationId = Guid.Empty;

            scopeProvider.ForEachScope((scope, state) =>
            {
                if (scope is IReadOnlyList<KeyValuePair<string, object>> scopeValues)
                {
                    foreach (var kvp in scopeValues)
                    {
                        if (kvp.Key.Equals("CorrelationId", StringComparison.OrdinalIgnoreCase) && kvp.Value is Guid id)
                        {
                            correlationId = id;
                            return; // Found it, no need to continue
                        }
                    }
                }
            }, (object)null!);

            return correlationId;
        }
    }

    public class LogEventArgs : EventArgs
    {
        public Guid CorrelationId { get; }
        public string Message { get; }

        public LogEventArgs(Guid correlationId, string message)
        {
            CorrelationId = correlationId;
            Message = message;
        }
    }
}
