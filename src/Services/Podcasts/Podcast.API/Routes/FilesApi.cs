using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Podcast.API.Routes;

public static class FilesApi
{
    public static RouteGroupBuilder MapFilesApi(this RouteGroupBuilder group)
    {
        group.MapPost("/uploadImage", UploadFileImage).WithName("UploadFileImage");
        group.MapPost("/uploadEpisode", UploadFileEpisode).WithName("UploadFileEpisode");
        return group;
    }

    public static async ValueTask<Results<NotFound, BadRequest<string>, Ok<string>>> UploadFileImage(
        PodcastDbContext podcastDbContext,
        IFormFile file,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.NotFound();
        }

        var directory = "image";
        
        var fileName = await SaveFileToStorage(file, directory, configuration, cancellationToken);
                
        return TypedResults.Ok($"{fileName}");
    }

    public static async ValueTask<Results<NotFound, BadRequest<string>, Ok<string>>> UploadFileEpisode(
        PodcastDbContext podcastDbContext,
        IFormFile file,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return TypedResults.NotFound();
        }

        var directory = "episode";
          
        var fileName = await SaveFileToStorage(file, "episode", configuration, cancellationToken);
        
        return TypedResults.Ok($"{fileName}");
    }

    private static async Task<string> SaveFileToStorage(
        IFormFile file,
        string directory,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetValue<string>("AzureStorage:ConnectionString") ??
                               throw new NullReferenceException("'AzureStorage:ConnectionString' not found.");
        var containerName = configuration.GetValue<string>("AzureStorage:ContainerName") ??
                            throw new NullReferenceException("'AzureStorage:ContainerName' not found.");

        var storageName = configuration.GetValue<string>("AzureStorage:StorageName") ??
                            throw new NullReferenceException("'AzureStorage:StorageName' not found.");
        
        var blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        
        // Ensure the container exists
        await containerClient.CreateIfNotExistsAsync();

        var newFileName = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(file.FileName);
        
        var blobFileName = $"{directory}/{newFileName}{fileExtension}";

        var blobClient = containerClient.GetBlobClient(blobFileName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true, cancellationToken);
        }

        return $"https://{storageName}.blob.core.windows.net/{containerName}/{blobFileName}";
    }
}