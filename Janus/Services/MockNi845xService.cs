namespace Janus.Services
{
    public class MockNi845xService : INi845xService
    {
        public string SendReceive(string message)
        {
            return $"Received: {message}, Responded with: PONG";
        }
    }
}
