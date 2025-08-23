namespace Janus.Services
{
    public class FtdiService : ISerialService
    {
        public string SendReceive(string message)
        {
            // In a real application, this would communicate with the FTDI device.
            // For now, we'll use a mock implementation.
            return $"FTDI Received: {message}, Responded with: PONG";
        }
    }
}
