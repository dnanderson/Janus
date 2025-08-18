using System;

namespace Janus.Services
{
    public class MockDaqService : IDaqService
    {
        private readonly Random _random = new Random();

        public double ReadVoltage()
        {
            return 12.0 + _random.NextDouble() * 0.1 - 0.05; // 12V +/- 0.05V
        }

        public double ReadTemperature()
        {
            return 25.0 + _random.NextDouble() * 2.0 - 1.0; // 25°C +/- 1°C
        }
    }
}
