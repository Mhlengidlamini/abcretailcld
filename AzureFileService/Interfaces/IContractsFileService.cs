using ABCRetail.Models;

namespace ABCRetail.AzureFileService.Interfaces
{
    public interface IContractsFileService
    {
        Task UploadFileAsync(IFormFile file);
        Task<Stream> DownloadFileAsync(string fileName);
        Task<IEnumerable<ContractsFileModel>> ListFilesAsync();
    }
}
