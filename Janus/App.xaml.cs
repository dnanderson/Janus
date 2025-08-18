using Autofac;
using System.Windows;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Janus.Services;
using Janus.ViewModels;

namespace Janus
{
    public partial class App : Application
    {
        public static IContainer? Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ContainerBuilder();

            // Configure NLog
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddNLog();
            });
            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            // Register Services
            builder.RegisterType<Services.MockNi845xService>().As<Services.INi845xService>().InstancePerDependency();
            builder.RegisterType<Services.MockDaqService>().As<Services.IDaqService>().InstancePerDependency();
            builder.RegisterType<Services.MockSmuService>().As<Services.ISmuService>().InstancePerDependency();
            builder.RegisterType<Services.MockUpsService>().As<Services.IUpsService>().SingleInstance(); // UPS can be a singleton
            builder.RegisterType<TestRunnerService>().As<ITestRunnerService>().InstancePerDependency();

            // Register ViewModels
            builder.RegisterType<MainViewModel>().SingleInstance();
            builder.RegisterType<HomeViewModel>().InstancePerDependency();
            builder.RegisterType<UutViewModel>().InstancePerDependency().ExternallyOwned();

            // Register Views
            builder.RegisterType<MainWindow>().SingleInstance();

            Container = builder.Build();

            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}
