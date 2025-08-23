namespace Janus.Services
{
    public class Ni845xService : ISerialService
    {
        public string SendReceive(string message)
        {
            // In a real application, this would communicate with the NI-845x device.
            // For now, we'll keep the mock implementation.
            return $"Received: {message}, Responded with: PONG";
        }
    }
}
