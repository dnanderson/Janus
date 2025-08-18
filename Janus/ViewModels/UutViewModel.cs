using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Janus.Services;

namespace Janus.ViewModels
{
    public class UutViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<UutViewModel>? OnCloseTest;

        private readonly ITestRunnerService _testRunnerService;
        private readonly ILogger<UutViewModel> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Stopwatch _stopwatch = new();
        private readonly Timer _timer;
        private Task? _testTask;

        public UutViewModel(ITestRunnerService testRunnerService, ILogger<UutViewModel> logger)
        {
            Status = "Running";
            SerialLog = new ObservableCollection<string>();
            GeneralLog = new ObservableCollection<string>();
            CorrelationId = Guid.NewGuid();

            _logger = logger;
            _testRunnerService = testRunnerService;

            StopTestCommand = new RelayCommand(StopTest);
            CloseCommand = new RelayCommand(CloseTest);

            ObservableLogger.LogReceived += OnLogReceived;

            _logger.LogInformation("UUT ViewModel created for {SerialNumber}", SerialNumber);
            _testTask = Task.Run(() =>
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = CorrelationId }))
                {
                    return _testRunnerService.RunTestAsync(this, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
            _stopwatch.Start();
            _timer = new Timer(_ => OnPropertyChanged(nameof(ElapsedTime)), null, 0, 100);
        }

        private void OnLogReceived(object? sender, LogEventArgs e)
        {
            if (e.CorrelationId != CorrelationId) return;

            Application.Current.Dispatcher.Invoke(() => GeneralLog.Add(e.Message));
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

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        private double _voltage;
        public double Voltage
        {
            get => _voltage;
            set
            {
                _voltage = value;
                OnPropertyChanged();
            }
        }

        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
            }
        }

        private double _current;
        public double Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        public ICommand StopTestCommand { get; }
        public ICommand CloseCommand { get; }

        private void StopTest()
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _logger.LogInformation("Stopping test for UUT {SerialNumber}", SerialNumber);
            _cancellationTokenSource.Cancel();
            Status = "Stopped by user.";
            AddGeneralLog("Test stopped by user.");
            _stopwatch.Stop();
        }

        private void CloseTest()
        {
            _logger.LogInformation("Closing test for UUT {SerialNumber}", SerialNumber);
            StopTest();
            OnCloseTest?.Invoke(this, this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        public void Dispose()
        {
            ObservableLogger.LogReceived -= OnLogReceived;
            _cancellationTokenSource.Cancel();
            _timer.Dispose();
        }
    }
}
