using Janus.Services;
using Janus.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Janus
{
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddProvider(new ObservableLoggerProvider());
            });

            // Register Services
            services.AddTransient<INi845xService, MockNi845xService>();
            services.AddTransient<IDaqService, MockDaqService>();
            services.AddTransient<ISmuService, MockSmuService>();
            services.AddSingleton<IUpsService, MockUpsService>();
            services.AddTransient<ITestRunnerService, TestRunnerService>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<UutViewModel>();

            services.AddSingleton<UutViewModelFactory>(provider =>
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
    }
}
