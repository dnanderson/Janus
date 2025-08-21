using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Janus.Services;

namespace Janus.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public event EventHandler<BeginTestEventArgs>? OnBeginTest;

        private readonly IUpsService _upsService;
        private readonly ILogger<HomeViewModel> _logger;
        private Timer _timer;

        public HomeViewModel(IUpsService upsService, ILogger<HomeViewModel> logger)
        {
            _upsService = upsService;
            _logger = logger;
            _upsService.SelfTestCompleted += (s, e) => UpdateUpsStatus();

            _logger.LogInformation("HomeViewModel created");
            UpdateUpsStatus();
            _timer = new Timer(_ => UpdateUpsStatus(), null, 0, 5000);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BeginTestCommand))]
        private string? _uutSerialNumber;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BeginTestCommand))]
        private string? _operatorName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BeginTestCommand))]
        private string? _testDescription;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BeginTestCommand))]
        private string? _drawer;


        public string Header => "Home";

        public bool IsClosable => false;

        public System.Collections.ObjectModel.ObservableCollection<UutViewModel> RunningUuts { get; set; }

        [RelayCommand(CanExecute = nameof(CanBeginTest))]
        private void BeginTest()
        {
            _logger.LogInformation("Begin test clicked for UUT {UutSerialNumber}", UutSerialNumber);
            if (int.TryParse(Drawer, out var drawerInt))
            {
                OnBeginTest?.Invoke(this, new BeginTestEventArgs(UutSerialNumber!, OperatorName!, TestDescription!, drawerInt));
            }
        }

        private bool CanBeginTest()
        {
            return !string.IsNullOrWhiteSpace(UutSerialNumber) &&
                   !string.IsNullOrWhiteSpace(OperatorName) &&
                   !string.IsNullOrWhiteSpace(TestDescription) &&
                   !string.IsNullOrWhiteSpace(Drawer);
        }

        [ObservableProperty]
        private UpsStatus? _upsStatus;

        [RelayCommand]
        private void UpdateUpsStatus()
        {
            UpsStatus = _upsService.GetStatus();
        }

        [RelayCommand]
        private void StartUpsSelfTest()
        {
            _logger.LogInformation("UPS self-test started");
            _upsService.StartSelfTest();
        }
    }
}
