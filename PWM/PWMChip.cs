

namespace ProjecFLX.PWMFanService.PWM;

public class PWMChip
{
    public int Number { get; }

    public int SupportedChannels { get; }


    public PWMChip(int number, int supportedChannels)
    {
        this.Number = number;
        this.SupportedChannels = supportedChannels;
    }

    public override string ToString()
    {
        return "pwmchip" + this.Number + " supports " + this.SupportedChannels + "channel" + ((this.SupportedChannels == 1 ? "." : "s."));
    }

}