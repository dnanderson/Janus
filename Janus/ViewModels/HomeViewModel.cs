using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Janus.Services;

namespace Janus.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<BeginTestEventArgs>? OnBeginTest;

        private readonly IUpsService _upsService;
        private readonly ILogger<HomeViewModel> _logger;
        private Timer _timer;

        public HomeViewModel(IUpsService upsService, ILogger<HomeViewModel> logger)
        {
            _upsService = upsService;
            _logger = logger;
            _upsService.SelfTestCompleted += (s, e) => UpdateUpsStatus();

            BeginTestCommand = new RelayCommand(BeginTest, CanBeginTest);
            StartUpsSelfTestCommand = new RelayCommand(StartUpsSelfTest);
            CloseCommand = new RelayCommand(() => {}, () => false); // Disabled command

            _logger.LogInformation("HomeViewModel created");
            UpdateUpsStatus(); // Initial status
            _timer = new Timer(_ => UpdateUpsStatus(), null, 0, 5000);
        }

        private string? _uutSerialNumber;
        public string? UutSerialNumber
        {
            get => _uutSerialNumber;
            set
            {
                _uutSerialNumber = value;
                OnPropertyChanged();
            }
        }

        private string? _operatorName;
        public string? OperatorName
        {
            get => _operatorName;
            set
            {
                _operatorName = value;
                OnPropertyChanged();
            }
        }

        private string? _testDescription;
        public string? TestDescription
        {
            get => _testDescription;
            set
            {
                _testDescription = value;
                OnPropertyChanged();
            }
        }

        private string? _drawer;
        public string? Drawer
        {
            get => _drawer;
            set
            {
                _drawer = value;
                OnPropertyChanged();
            }
        }


        public string Header => "Home";

        public bool IsClosable => false;

        public System.Collections.ObjectModel.ObservableCollection<UutViewModel> RunningUuts { get; set; }

        public ICommand BeginTestCommand { get; }

        private void BeginTest()
        {
            _logger.LogInformation("Begin test clicked for UUT {UutSerialNumber}", UutSerialNumber);
            OnBeginTest?.Invoke(this, new BeginTestEventArgs(UutSerialNumber!, OperatorName!, TestDescription!, Drawer!));
        }

        private bool CanBeginTest()
        {
            return !string.IsNullOrWhiteSpace(UutSerialNumber) &&
                   !string.IsNullOrWhiteSpace(OperatorName) &&
                   !string.IsNullOrWhiteSpace(TestDescription) &&
                   !string.IsNullOrWhiteSpace(Drawer);
        }

        private UpsStatus? _upsStatus;
        public UpsStatus? UpsStatus
        {
            get => _upsStatus;
            private set
            {
                _upsStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartUpsSelfTestCommand { get; }
        public ICommand CloseCommand { get; }

        private void UpdateUpsStatus()
        {
            UpsStatus = _upsService.GetStatus();
        }

        private void StartUpsSelfTest()
        {
            _logger.LogInformation("UPS self-test started");
            _upsService.StartSelfTest();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
