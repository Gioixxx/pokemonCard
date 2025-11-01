using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokemonCardManager.Data;
using PokemonCardManager.Services;
using System;
using System.IO;
using System.Windows;

namespace PokemonCardManager
{
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Configurazione del database
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PokemonCardManager", "pokemoncards.db");
            
            // Assicurati che la directory esista
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Registrazione dei servizi
            services.AddTransient<ICardService, CardService>();
            services.AddTransient<ISaleService, SaleService>();
            services.AddTransient<IDataExportService, DataExportService>();

            // Registrazione delle viste e viewmodel
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inizializzazione del database
            if (ServiceProvider != null)
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                }

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        }
    }
}