using System.Net;
using System.Xml;

using Azure.Storage.Blobs;

public class AzureBlobXmlResolver : XmlResolver
{
    private readonly string _connectionString;

    public AzureBlobXmlResolver(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override ICredentials Credentials
    {
        set { /* Not used in this resolver */ }
    }

    public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
    {
        // Ensure the scheme is 'azureblob'
        if (absoluteUri.Scheme != "azureblob")
        {
            throw new NotSupportedException("Only 'azureblob' scheme is supported.");
        }

        // Ensure the expected return type is Stream
        if (ofObjectToReturn != null && ofObjectToReturn != typeof(Stream))
        {
            throw new ArgumentException("Expected return type is Stream.", nameof(ofObjectToReturn));
        }

        // Extract container name and blob name from the URI
        string containerName = absoluteUri.Host;
        string blobName = absoluteUri.AbsolutePath.TrimStart('/');

        // Create BlobServiceClient using the provided connection string
        BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

        // Get the container client
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Get the blob client
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        // Download the blob content into a MemoryStream
        MemoryStream memoryStream = new MemoryStream();
        blobClient.DownloadTo(memoryStream);
        memoryStream.Position = 0; // Reset the stream position to the beginning

        return memoryStream;
    }
}
                                    