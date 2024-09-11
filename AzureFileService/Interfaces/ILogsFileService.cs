using ABCRetail.Models;

namespace ABCRetail.AzureFileService.Interfaces
{
    public interface ILogsFileService
    {
        Task UploadFileAsync(IFormFile file);
        Task<Stream> DownloadFileAsync(string fileName);
        Task<IEnumerable<LogsFileModel>> ListFilesAsync();
    }
}
