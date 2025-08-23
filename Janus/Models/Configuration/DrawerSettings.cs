namespace Janus.Models.Configuration
{
    public class DrawerSettings
    {
        public int DrawerId { get; set; }
        public string SmuResourceName { get; set; }
        public string DaqResourceName { get; set; }
        public SerialDeviceSettings SerialDevice { get; set; }
    }

    public class SerialDeviceSettings
    {
        public string Port { get; set; }
        public int BaudRate { get; set; }
    }
}
