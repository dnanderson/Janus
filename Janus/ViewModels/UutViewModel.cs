using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Serilog.Events;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Janus.Services;

namespace Janus.ViewModels
{
    public partial class UutViewModel : ObservableObject, IDisposable
    {
        public event EventHandler<UutViewModel>? OnCloseTest;

        private readonly ITestRunnerService _testRunnerService;
        private readonly ILogger<UutViewModel> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Stopwatch _stopwatch = new();
        private readonly Timer _timer;
        private readonly IDisposable _logEventSubscription;
        private Task? _testTask;

        public UutViewModel(ITestRunnerService testRunnerService, ILogger<UutViewModel> logger, SerilogObserverService serilogObserverService)
        {
            _status = "Running";
            SerialLog = new ObservableCollection<string>();
            GeneralLog = new ObservableCollection<string>();
            CorrelationId = Guid.NewGuid();

            _logger = logger;
            _testRunnerService = testRunnerService;

            _logEventSubscription = serilogObserverService.LogEvents
                .Where(logEvent =>
                {
                    if (logEvent.Properties.TryGetValue("CorrelationId", out var correlationIdValue) &&
                        correlationIdValue is ScalarValue scalarValue &&
                        scalarValue.Value is string correlationIdString)
                    {
                        return correlationIdString == CorrelationId.ToString();
                    }
                    return false;
                })
                .Select(logEvent => logEvent.RenderMessage())
                .Subscribe(message =>
                {
                    Application.Current.Dispatcher.Invoke(() => GeneralLog.Add(message));
                });


            _logger.LogInformation("UUT ViewModel created for {SerialNumber}", SerialNumber);
            _testTask = Task.Run(() =>
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = CorrelationId.ToString() }))
                {
                    return _testRunnerService.RunTestAsync(this, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
            _stopwatch.Start();
            _timer = new Timer(_ => OnPropertyChanged(nameof(ElapsedTime)), null, 0, 100);
        }

        public Guid CorrelationId { get; set; }
        public string SerialNumber { get; set; }
        public string OperatorName { get; set; }
        public string TestDescription { get; set; }
        public int Drawer { get; set; }
        public string Header => $"UUT: {SerialNumber}";
        public bool IsClosable => true;
        public TimeSpan ElapsedTime => _stopwatch.Elapsed;
        public ObservableCollection<string> SerialLog { get; }
        public ObservableCollection<string> GeneralLog { get; }

        public void AddSerialLog(string message)
        {
            _logger.LogInformation("SerialLog: {Message}", message);
            Application.Current.Dispatcher.Invoke(() => SerialLog.Add(message));
        }

        public void AddGeneralLog(string message)
        {
            _logger.LogInformation("GeneralLog: {Message}", message);
            Application.Current.Dispatcher.Invoke(() => GeneralLog.Add(message));
        }

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private double _voltage;

        [ObservableProperty]
        private double _temperature;

        [ObservableProperty]
        private double _current;

        [RelayCommand]
        private void StopTest()
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _logger.LogInformation("Stopping test for UUT {SerialNumber}", SerialNumber);
            _cancellationTokenSource.Cancel();
            Status = "Stopped by user.";
            AddGeneralLog("Test stopped by user.");
            _stopwatch.Stop();
        }

        [RelayCommand]
        private void CloseTest()
        {
            _logger.LogInformation("Closing test for UUT {SerialNumber}", SerialNumber);
            StopTest();
            OnCloseTest?.Invoke(this, this);
        }

        public void Dispose()
        {
            _logEventSubscription.Dispose();
            _cancellationTokenSource.Cancel();
            _timer.Dispose();
        }
    }
}
