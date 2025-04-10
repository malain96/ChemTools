using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

namespace ChemTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IServiceProvider ServiceProvider { get; set; }

        private IConfiguration Configuration { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            ServiceCollection serviceCollection = new();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            AppSettings appSettings = new();
            Configuration.GetSection(nameof(AppSettings)).Bind(appSettings);
            services.AddSingleton(appSettings);

            NucleosideSettings nucleosideSettings = new();
            Configuration.GetSection(nameof(NucleosideSettings)).Bind(nucleosideSettings);
            services.AddSingleton(nucleosideSettings);

            services.AddScoped(typeof(MainWindow));
        }
    }
}