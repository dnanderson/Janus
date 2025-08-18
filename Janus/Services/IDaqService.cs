namespace Janus.Services
{
    public interface IDaqService
    {
        double ReadVoltage();
        double ReadTemperature();
    }
}
