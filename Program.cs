using ProjecFLX.PWMFanService;
using ProjecFLX.PWMFanService.Settings;


IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSystemdConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        ApplicationSettings settings = hostContext.Configuration.GetSection("Application").Get<ApplicationSettings>();

        services.AddSingleton(settings);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
