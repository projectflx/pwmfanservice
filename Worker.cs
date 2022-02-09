using System.Device.Pwm;
using ProjecFLX.PWMFanService.Settings;
using ProjecFLX.PWMFanService.PWM;

namespace ProjecFLX.PWMFanService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ApplicationSettings _settings;

    public Worker(ILogger<Worker> logger, ApplicationSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    private bool validPreconditions(int chip, int channel, int minTemp, int maxTemp, double usedDutyCycle, int frequency, int sleepTimeBetweenLoops)
    {
        bool vdtoverlay = true;
        bool vtemperatureSensor = true;
        bool vchip = false;
        bool vchannel = false;
        bool vminTemp = true;
        bool vmaxTemp = true;
        bool vusedDutyCycle = true;
        bool vfrequency = true;
        bool vsleepTimeBetweenLoops = true;

        // Read /boot/config.txt and identify if the device tree overlay for pwm is active
        if (!isPWMDtOverlayActive())
        {
            _logger.LogError("Device tree overlay for pwm is not present in file /boot/config.txt.");
            vdtoverlay = false;
        }

        // Is /sys/class/thermal/thermal_zone0/temp is present
        if (!isCPUTemperatureSensorAvailable())
        {
            _logger.LogError("CPU-Temperature can't be obtained because \"/sys/class/thermal/thermal_zone0/temp\" is not present.");
            vtemperatureSensor = false;
        }

        List<PWMChip> chips = getAvailablePWMHardware();
        foreach(PWMChip c in chips)
        {
            if(c.Number == chip)
            {
                vchip = true;
                if(channel < c.SupportedChannels)
                {
                    vchannel = true;
                }
            }
        }
        if(!vchip)
        {
            _logger.LogError("PWM chip with the number in \"Chip\" not found.");
        }

        if(!vchannel)
        {
            _logger.LogError("PWM channel with the number in \"Channel\" is not supported for the give \"Chip\".");
        }

        if (minTemp < 0)
        {
            _logger.LogError("Less than zero degree for \"MinTemp\" are not possible.");
            vminTemp = false;
        }

        if (maxTemp <= minTemp)
        {
            _logger.LogError("\"MaxTemp\" can't be less or equal to \"MinTemp\".");
            vmaxTemp = false;
        }

        if (usedDutyCycle < 0.0 || usedDutyCycle > 1.0)
        {
            _logger.LogError("\"UsedDutyCycle\" must be between 0.0 and 1.0.");
            vusedDutyCycle = false;
        }

        if (frequency <= 0)
        {
            _logger.LogError("\"Frequency\" can't be zero.");
            vfrequency = false;
        }

        if (sleepTimeBetweenLoops < 1000)
        {
            _logger.LogError("\"SleepTimeBetweenLoops\" can't be less than one second.");
            vsleepTimeBetweenLoops = false;
        }

        return vdtoverlay && vtemperatureSensor && vchip && vchannel && vminTemp && vmaxTemp && vusedDutyCycle && vfrequency && vsleepTimeBetweenLoops;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get settings from appsettings.json
        int settings_chip = _settings.PWM.Chip;
        int settings_channel = _settings.PWM.Channel;
        int settings_minTemp = _settings.MinTemp;
        int settings_maxTemp = _settings.MaxTemp;
        double settings_usedDutyCycle = _settings.PWM.UsedDutyCycle;
        int settings_frequency = _settings.PWM.Frequency;
        int settings_sleepBetweenLoops = _settings.SleepBetweenLoops;

        // Check preconditions
        if (validPreconditions(settings_chip, settings_channel, settings_minTemp, settings_maxTemp, settings_usedDutyCycle, settings_frequency, settings_sleepBetweenLoops))
        {
            _logger.LogInformation("PWM Info: chip: " + settings_chip + ", channel: " + settings_channel + " frequency: " + settings_frequency + " Hz, sleep between loops: " + settings_sleepBetweenLoops + " ms.");

            using (PwmChannel channel = PwmChannel.Create(settings_chip, settings_channel, settings_frequency))
            {
                channel.Start();

                double currentDutyCycle = -1.0d;
                double dutyCycle = -1.0d;

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Get temperature
                    double temperature = Convert.ToInt32(File.ReadAllText("/sys/class/thermal/thermal_zone0/temp")) / 1000d;

                    // In which temperature-range are we
                    if(temperature < settings_minTemp)
                    {
                        dutyCycle = 0.0d;
                    } else if (temperature > settings_maxTemp)
                    {
                        dutyCycle = 1.0d;
                    } else {
                        dutyCycle = settings_usedDutyCycle;
                    }

                    if(currentDutyCycle != dutyCycle)
                    {
                        channel.DutyCycle = dutyCycle;
                        if(currentDutyCycle == -1.0d)
                        {
                            _logger.LogInformation("Duty cycle changed from intial value to " + (dutyCycle * 100) + "%.");
                        } else {
                            _logger.LogInformation("Duty cycle has changed from " + (currentDutyCycle * 100) + "% to " + (dutyCycle * 100) + "% because of a temperature change.");
                        }
                        currentDutyCycle = dutyCycle;
                    }

                    await Task.Delay(settings_sleepBetweenLoops, stoppingToken);
                }
            }
        }
        else
        {
            _logger.LogError("Preconditions are not met. Please consult syslog for more information on the cause.");
        }
    }

    private bool isCPUTemperatureSensorAvailable()
    {
        FileInfo temperatureFile = new FileInfo("/sys/class/thermal/thermal_zone0/temp");
        if (!temperatureFile.Exists)
        {
            return false;
        }
        return true;
    }

    private bool isPWMDtOverlayActive()
    {
        FileInfo file_bootconfigtxt = new FileInfo("/boot/config.txt");
        StreamReader sr = new StreamReader(file_bootconfigtxt.OpenRead());
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.ToLower().StartsWith("dtoverlay=pwm"))
            {
                return true;
            }
        }
        return false;
    }

    private List<PWMChip> getAvailablePWMHardware()
    {
        string path_pwm = "/sys/class/pwm/";
        string pattern_pwmchip = "pwmchip";

        List<PWMChip> pwm = new List<PWMChip>();

        DirectoryInfo directory_pwm = new DirectoryInfo(path_pwm);
        IEnumerable<DirectoryInfo> directories_pwmchips = directory_pwm.GetDirectories(pattern_pwmchip + "*");
        foreach (DirectoryInfo pwmchip in directories_pwmchips)
        {
            PWMChip chip = new PWMChip(Convert.ToInt32(pwmchip.Name.Remove(0, pattern_pwmchip.Length)), Convert.ToInt32(File.ReadAllText(pwmchip.FullName + Path.DirectorySeparatorChar + "npwm")));
            pwm.Add(chip);
        }
        return pwm;
    }
}
