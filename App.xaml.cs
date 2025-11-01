using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
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
            // Registrazione del logger
            services.AddSingleton<Services.ILogger, SerilogLogger>();

            // Configurazione del database
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PokemonCardManager", "pokemoncards.db");

            // Assicurati che la directory esista
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Registrazione Cache e Rate Limiter
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IRateLimiter>(sp => new RateLimiter(maxConcurrentRequests: 5, timeWindow: TimeSpan.FromSeconds(1), logger: sp.GetRequiredService<ILogger>()));

            // Registrazione Theme Service
            services.AddSingleton<IThemeService, ThemeService>();

            // Registrazione Undo/Redo Service (usa factory per ottenere IServiceProvider)
            services.AddSingleton<IUndoRedoService>(sp => new UndoRedoService(sp.GetRequiredService<ILogger>(), sp));

            // Registrazione Bulk Import Service
            services.AddTransient<IBulkImportService, BulkImportService>();

            // Registrazione HttpClient per PokéAPI
            services.AddHttpClient<IPokeApiService, PokeApiService>();

            // Registrazione dei servizi
            services.AddTransient<ICardService, CardService>();
            services.AddTransient<ISaleService, SaleService>();
            services.AddTransient<IDataExportService, DataExportService>();
            services.AddSingleton<PokemonSetService>();

            // Registrazione delle viste e viewmodel
            services.AddSingleton<MainWindow>();
            services.AddTransient<Views.InventoryView>();
            services.AddTransient<Views.SalesView>();
            services.AddTransient<Views.DashboardView>();
            services.AddTransient<Views.SettingsView>();

            // Register IServiceProvider itself for MainWindow
            services.AddSingleton<IServiceProvider>(sp => sp);
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

                // Load theme
                var themeService = ServiceProvider.GetRequiredService<IThemeService>();
                themeService.LoadTheme();

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        }
    }
}