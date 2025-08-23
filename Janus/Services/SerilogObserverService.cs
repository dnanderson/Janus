using Serilog.Events;
using System;
using System.Reactive.Subjects;

namespace Janus.Services
{
    public class SerilogObserverService : IDisposable
    {
        private readonly ReplaySubject<LogEvent> _logEventSubject = new();

        public IObservable<LogEvent> LogEvents => _logEventSubject;

        public void OnNext(LogEvent logEvent)
        {
            _logEventSubject.OnNext(logEvent);
        }

        public void Dispose()
        {
            _logEventSubject.Dispose();
        }
    }
}
