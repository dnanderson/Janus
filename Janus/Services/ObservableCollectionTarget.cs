using System;
using System.Collections.ObjectModel;
using NLog;
using NLog.Targets;

namespace Janus.Services
{
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

    [Target("ObservableCollection")]
    public sealed class ObservableCollectionTarget : TargetWithLayout
    {
        public ObservableCollectionTarget()
        {
        }

        public static event EventHandler<LogEventArgs> LogReceived;

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);
            if (logEvent.Properties.TryGetValue("CorrelationId", out var correlationIdObj) && correlationIdObj is Guid correlationId)
            {
                LogReceived?.Invoke(this, new LogEventArgs(correlationId, logMessage));
            }
        }
    }
}
