using Microsoft.Extensions.Hosting;
using System.ServiceProcess;
using WinSleepWell;
using WinSleepWellLib;
using WinSleepWellService;


if (PrivilegeManager.EnsureAdminPrivileges(true, "Service") == false)
{
    Environment.Exit(1);
    return;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = Identifiers.ServiceName;
});

// Configures logging to use Windows Event Log (currently disabled).
// LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

// Registering services for dependency injection
builder.Services.AddHostedService<WinBackgroundService>();  // Registers WinBackgroundService as a hosted service to run in the background.

// Building the host that manages the application's lifecycle
IHost host = builder.Build();  // Builds the host with the configured services and settings.
host.Run();  // Starts the host, running the application until it is stopped.


/*
ServiceBase[] ServicesToRun = new ServiceBase[]
{
    new WinServiceManager(serviceName: Identifiers.ServiceName)
};
ServiceBase.Run(ServicesToRun);  // Starts the host, running the application until it is stopped.
*/

/*
ServiceBase[] ServicesToRun;
ServicesToRun = new ServiceBase[]
{
                new LidStatusService.LidStatusService()
};
ServiceBase.Run(ServicesToRun);
*/