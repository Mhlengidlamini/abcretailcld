namespace ABCRetail.AzureBlobService.Interface
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream);
    }
}
