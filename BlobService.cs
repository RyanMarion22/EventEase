using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;


public class BlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(IConfiguration configuration)
    {
        _connectionString = configuration["DefaultEndpointsProtocol=https;AccountName=ryanst10440289;AccountKey=7kkKWH7JqQa3IRMntO7VX+SgWtQ+K3A2psZsBXWiU+i2TYPPijFu0DxZng2egyWihjE0jnSwMCQu+AStCxALSw==;EndpointSuffix=core.windows.net"];
        _containerName = configuration["images"];
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var blobClient = new BlobContainerClient(_connectionString, _containerName);
        await blobClient.CreateIfNotExistsAsync();

        var blob = blobClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream);
        }

        return blob.Uri.ToString();
    }
}
