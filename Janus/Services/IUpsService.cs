using System;

namespace Janus.Services
{
    public interface IUpsService
    {
        UpsStatus GetStatus();
        void StartSelfTest();
        event EventHandler SelfTestCompleted;
    }

    public class UpsStatus
    {
        public double StateOfCharge { get; set; }
        public int BatteryAgeDays { get; set; }
        public DateTime NextReplacementDate { get; set; }
        public double InputVoltage { get; set; }
        public double OutputVoltage { get; set; }
        public string? LastSelfTestResult { get; set; }
    }
}
