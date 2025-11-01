using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface IDataExportService
    {
        Task<bool> ExportCardsToCSV(string filePath);
        Task<bool> ExportSalesToCSV(string filePath);
        Task<bool> BackupDatabase(string filePath);
    }
}