using System;

namespace PokemonCardManager.Services
{
    /// <summary>
    /// Interface for logging functionality throughout the application
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message
        /// </summary>
        void LogInformation(string message);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message with exception details
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Logs a debug message
        /// </summary>
        void LogDebug(string message);
    }
}
