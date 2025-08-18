using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Janus.ViewModels;

namespace Janus.Services
{
    public class TestRunnerService : ITestRunnerService
    {
        private readonly IDaqService _daqService;
        private readonly ISmuService _smuService;
        private readonly INi845xService _ni845xService;
        private readonly ILogger<TestRunnerService> _logger;

        public TestRunnerService(IDaqService daqService, ISmuService smuService, INi845xService ni845xService, ILogger<TestRunnerService> logger)
        {
            _daqService = daqService;
            _smuService = smuService;
            _ni845xService = ni845xService;
            _logger = logger;
        }

        public async Task RunTestAsync(UutViewModel uutViewModel, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting test for UUT {SerialNumber}", uutViewModel.SerialNumber);
                await Task.Delay(500, cancellationToken); // Simulate work
                _logger.LogInformation("Hardware configuration complete for UUT {SerialNumber}", uutViewModel.SerialNumber);

                while (!cancellationToken.IsCancellationRequested)
                {
                    uutViewModel.Voltage = _daqService.ReadVoltage();
                    uutViewModel.Temperature = _daqService.ReadTemperature();
                    uutViewModel.Current = _smuService.ReadCurrent();

                    var message = $"READ_STATUS";
                    _logger.LogInformation("SerialLog: > {Message}", message);
                    var response = _ni845xService.SendReceive(message);
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
