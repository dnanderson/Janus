using Janus.Models.Configuration;
using Janus.Services;
using Janus.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;

namespace Janus
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog((hostContext, services, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .WriteTo.Observable(services.GetRequiredService<SerilogObserverService>());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureServices(services, hostContext.Configuration);
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
            services.Configure<SerialServiceSettings>(configuration.GetSection("SerialService"));
            services.Configure<HardwareSettings>(configuration);

            services.AddSingleton<SerilogObserverService>();

            // Register Services
            var serialServiceSettings = configuration.GetSection("SerialService").Get<SerialServiceSettings>();
            switch (serialServiceSettings?.Type)
            {
                case "Ni845x":
                    services.AddTransient<ISerialService, Ni845xService>();
                    break;
                case "FTDI":
                    services.AddTransient<ISerialService, FtdiService>();
                    break;
                default:
                    // Default to Ni845x or throw an exception
                    services.AddTransient<ISerialService, Ni845xService>();
                    break;
            }
            services.AddTransient<IDaqService, MockDaqService>();
            services.AddTransient<ISmuService, MockSmuService>();
            services.AddSingleton<IUpsService, MockUpsService>();
            services.AddTransient<ITestRunnerService, TestRunnerService>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<UutViewModel>();

            services.AddTransient<UutViewModelFactory>(provider =>
                (serialNumber, operatorName, testDescription, drawer) =>
                {
                    var uutViewModel = provider.GetRequiredService<UutViewModel>();
                    uutViewModel.SerialNumber = serialNumber;
                    uutViewModel.OperatorName = operatorName;
                    uutViewModel.TestDescription = testDescription;
                    uutViewModel.Drawer = drawer;
                    return uutViewModel;
                });

            // Register Views
            services.AddSingleton<MainWindow>();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
