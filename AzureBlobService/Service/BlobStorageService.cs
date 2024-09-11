using ABCRetail.AzureBlobService.Interface;
using Azure.Storage.Blobs;

namespace ABCRetail.AzureBlobService.Service;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStorageService(string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        //create a blob storage if it doesn't exist
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("productimages");
        _blobContainerClient.CreateIfNotExists();
    }

    //upload file to blob storage container
    public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
    {
        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, true);
            return blobClient.Uri.ToString(); //return URL of uploaded file
        }
        catch (Exception ex)
        {
            // Log the exception or handle it accordingly
            Console.WriteLine($"Error uploading file to blob storage: {ex.Message}");
            throw; // Optionally rethrow the exception if you want it to propagate
        }

    }
}