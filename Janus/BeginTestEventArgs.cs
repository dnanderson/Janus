using System;

namespace Janus
{
    public class BeginTestEventArgs : EventArgs
    {
        public string SerialNumber { get; }
        public string OperatorName { get; }
        public string TestDescription { get; }
        public string Drawer { get; }

        public BeginTestEventArgs(string serialNumber, string operatorName, string testDescription, string drawer)
        {
            SerialNumber = serialNumber;
            OperatorName = operatorName;
            TestDescription = testDescription;
            Drawer = drawer;
        }
    }
}
