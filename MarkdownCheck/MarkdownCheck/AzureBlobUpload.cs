using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MarkdownCheck
{
  class AzureBlobUpload
  {
    // Upload html files
    // Upload image files
    // Upload main.json, series.json, rss.xml
    CloudBlobClient BlobClient;

    string HtmlDir;
    string RssPath;
    string MainJson;
    string PhotosDir;
    string SeriesJson;
    
    public AzureBlobUpload(string htmlDir, string mainJson, string rssPath, string photosDir, string seriesJson)
    {
      string ConnectionString = Environment.GetEnvironmentVariable("zeerorgprocessedblog_sas");
      CloudStorageAccount storageAccount;
      if (!CloudStorageAccount.TryParse(ConnectionString, out storageAccount))
      {
        throw new Exception("Connection string not set " + ConnectionString);
      }

      this.BlobClient = storageAccount.CreateCloudBlobClient();
      this.RssPath = rssPath;
      this.PhotosDir = photosDir;
      this.MainJson = mainJson;
      this.HtmlDir = htmlDir;
      this.SeriesJson = seriesJson;
    }

    public void Upload()
    {
      UploadHtml();
      UploadMetadata();
      UploadPhoto();
    }

    void UploadHtml()
    {
      CloudBlobContainer blobContainer = BlobClient.GetContainerReference("compiled");
      Parallel.ForEach(new DirectoryInfo(HtmlDir).GetFiles("*.html"), async (file) => {
        await AzureBlobUpload.UploadBlob(blobContainer.GetBlockBlobReference(file.Name), file.FullName);
      });
    }

    void UploadPhoto()
    {
      if (String.IsNullOrEmpty(PhotosDir))
      {
        return;
      }
      CloudBlobContainer blobContainer = BlobClient.GetContainerReference("photos");
      Parallel.ForEach(new DirectoryInfo(PhotosDir).GetFiles(), async (file) => {
        await AzureBlobUpload.UploadBlob(blobContainer.GetBlockBlobReference(file.Name), file.FullName);
      });
    }

    void UploadMetadata()
    {
      if (String.IsNullOrEmpty(SeriesJson) || String.IsNullOrEmpty(MainJson))
      {
        return;
      }
      CloudBlobContainer blobContainer = BlobClient.GetContainerReference("metadata");
      Task.WaitAll(
        AzureBlobUpload.UploadBlob(blobContainer.GetBlockBlobReference("main.json"), MainJson),
        AzureBlobUpload.UploadBlob(blobContainer.GetBlockBlobReference("series.json"), SeriesJson),
        AzureBlobUpload.UploadBlob(blobContainer.GetBlockBlobReference("rss.xml"), RssPath)
      );
    }

    static async Task UploadBlob(CloudBlockBlob blockBlob, string filePath)
    {
      await blockBlob.UploadFromFileAsync(filePath);
      blockBlob.Properties.CacheControl = "max-age=0, must-revalidate, public";
      await blockBlob.SetPropertiesAsync();
    }
  }
}