using System;
using Serilog;
using Serilog.Events;

namespace PokemonCardManager.Services
{
    /// <summary>
    /// Serilog implementation of ILogger interface
    /// </summary>
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;

        public SerilogLogger()
        {
            // Configure Serilog to write to file and debug output
            string logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PokemonCardManager",
                "Logs",
                "log-.txt");

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug()
                .CreateLogger();

            _logger.Information("=== Pokemon Card Manager Started ===");
        }

        public void LogInformation(string message)
        {
            _logger.Information(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                _logger.Error(exception, message);
            }
            else
            {
                _logger.Error(message);
            }
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }
    }
}
