using Janus.Models.Configuration;
using Janus.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Janus.Services
{
    public class TestRunnerService : ITestRunnerService
    {
        private readonly IDaqService _daqService;
        private readonly ISmuService _smuService;
        private readonly ISerialService _serialService;
        private readonly ILogger<TestRunnerService> _logger;
        private readonly HardwareSettings _hardwareSettings;
        private readonly DatabaseSettings _databaseSettings;

        public TestRunnerService(
            IDaqService daqService,
            ISmuService smuService,
            ISerialService serialService,
            IOptions<HardwareSettings> hardwareSettings,
            IOptions<DatabaseSettings> databaseSettings,
            ILogger<TestRunnerService> logger)
        {
            _daqService = daqService;
            _smuService = smuService;
            _serialService = serialService;
            _logger = logger;
            _hardwareSettings = hardwareSettings.Value;
            _databaseSettings = databaseSettings.Value;
        }

        public async Task RunTestAsync(UutViewModel uutViewModel, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting test for UUT {SerialNumber}", uutViewModel.SerialNumber);
                _logger.LogInformation("Database URL: {DatabaseUrl}", _databaseSettings.Url);

                var drawerSettings = _hardwareSettings.Drawers.FirstOrDefault(d => d.DrawerId == uutViewModel.Drawer);
                if (drawerSettings == null)
                {
                    _logger.LogError("No hardware configuration found for drawer {DrawerId}", uutViewModel.Drawer);
                    uutViewModel.Status = "Error: No hardware configuration";
                    return;
                }

                _logger.LogInformation("Using hardware configuration for drawer {DrawerId}: SMU='{SmuResourceName}', DAQ='{DaqResourceName}', Serial='{SerialPort}'",
                    drawerSettings.DrawerId, drawerSettings.SmuResourceName, drawerSettings.DaqResourceName, drawerSettings.SerialDevice.Port);

                await Task.Delay(500, cancellationToken); // Simulate work
                _logger.LogInformation("Hardware configuration complete for UUT {SerialNumber}", uutViewModel.SerialNumber);

                while (!cancellationToken.IsCancellationRequested)
                {
                    uutViewModel.Voltage = _daqService.ReadVoltage();
                    uutViewModel.Temperature = _daqService.ReadTemperature();
                    uutViewModel.Current = _smuService.ReadCurrent();

                    var message = $"READ_STATUS";
                    _logger.LogInformation("SerialLog: > {Message}", message);
                    var response = _serialService.SendReceive(message);
                    _logger.LogInformation("SerialLog: < {Response}", response);

                    _logger.LogInformation("Poll successful for UUT {SerialNumber}. V={Voltage:F2}, T={Temperature:F1}, I={Current:F3}",
                        uutViewModel.SerialNumber, uutViewModel.Voltage, uutViewModel.Temperature, uutViewModel.Current);

                    await Task.Delay(2000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Test for UUT {SerialNumber} was canceled.", uutViewModel.SerialNumber);
                uutViewModel.Status = "Test Canceled";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during test for UUT {SerialNumber}", uutViewModel.SerialNumber);
                uutViewModel.Status = "Error";
            }
        }
    }
}
