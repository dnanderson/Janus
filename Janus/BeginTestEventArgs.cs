using System;

namespace Janus
{
    public class BeginTestEventArgs : EventArgs
    {
        public string SerialNumber { get; }
        public string OperatorName { get; }
        public string TestDescription { get; }
        public int Drawer { get; }

        public BeginTestEventArgs(string serialNumber, string operatorName, string testDescription, int drawer)
        {
            SerialNumber = serialNumber;
            OperatorName = operatorName;
            TestDescription = testDescription;
            Drawer = drawer;
        }
    }
}
