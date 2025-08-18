using System;
using System.Threading.Tasks;

namespace Janus.Services
{
    public class MockUpsService : IUpsService
    {
        private readonly Random _random = new Random();
        private UpsStatus _status;

        public event EventHandler? SelfTestCompleted;

        public MockUpsService()
        {
            _status = new UpsStatus
            {
                StateOfCharge = 98.5,
                BatteryAgeDays = 365,
                NextReplacementDate = DateTime.Now.AddYears(2),
                InputVoltage = 120.1,
                OutputVoltage = 119.9,
                LastSelfTestResult = "Passed"
            };
        }

        public UpsStatus GetStatus()
        {
            // In a real implementation, this would query the device.
            // Here, we can simulate some fluctuations.
            _status.StateOfCharge = 98.0 + _random.NextDouble();
            _status.InputVoltage = 120.0 + _random.NextDouble() * 0.5 - 0.25;
            _status.OutputVoltage = 120.0 + _random.NextDouble() * 0.2 - 0.1;
            return _status;
        }

        public async void StartSelfTest()
        {
            _status.LastSelfTestResult = "In Progress...";
            SelfTestCompleted?.Invoke(this, EventArgs.Empty);

            await Task.Delay(5000); // Simulate a 5-second self-test

            _status.LastSelfTestResult = _random.Next(10) > 1 ? "Passed" : "Failed";
            SelfTestCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
