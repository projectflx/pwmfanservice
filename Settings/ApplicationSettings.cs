namespace ProjecFLX.PWMFanService.Settings;
public class ApplicationSettings
{
    public PWMSettings PWM { get; set; } = new PWMSettings();
    public int SleepBetweenLoops { get; set; }
    public int MinTemp { get; set; }
    public int MaxTemp { get; set; }
}
