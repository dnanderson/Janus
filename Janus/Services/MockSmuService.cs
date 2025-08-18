using System;

namespace Janus.Services
{
    public class MockSmuService : ISmuService
    {
        private readonly Random _random = new Random();

        public double ReadCurrent()
        {
            return 1.5 + _random.NextDouble() * 0.01 - 0.005; // 1.5A +/- 0.005A
        }
    }
}
