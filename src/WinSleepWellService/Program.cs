using WinSleepWell;
using WinSleepWellService;

if (PrivilegeManager.EnsureAdminPrivileges(true, "WinSleepWell Service") == false)
{
    Environment.Exit(1);
    return;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "WinSleepWell Service";
});

// Configures logging to use Windows Event Log (currently disabled).
// LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

// Registering services for dependency injection
builder.Services.AddSingleton<SleepKeeperService>();  // Adds SleepKeeperService as a singleton, shared across the entire application.
builder.Services.AddHostedService<WindowsBackgroundService>();  // Registers WindowsBackgroundService as a hosted service to run in the background.

// Building the host that manages the application's lifecycle
IHost host = builder.Build();  // Builds the host with the configured services and settings.
host.Run();  // Starts the host, running the application until it is stopped.
