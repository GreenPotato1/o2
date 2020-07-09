using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using CloudBlobClient = Microsoft.Azure.Storage.Blob.CloudBlobClient;
using CloudBlobContainer = Microsoft.Azure.Storage.Blob.CloudBlobContainer;
using CloudBlockBlob = Microsoft.Azure.Storage.Blob.CloudBlockBlob;

namespace O2.Black.Toolkit.Core
{
    //Todo: added unitTests and IntegrationTests
   public static class AzureBlobHelper
    {
        public static void ClearStorage(TypeTable typeTable)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(accountName: CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountName, 
                CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            
            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(CloudStorage.Instance.GetAccountCloudStorage(typeTable).Container);
            
            if(container.Exists())
                container.Delete();
        }
        
        public static async Task<string> UploadFileToStorage(Stream fileStream, string fileName,TypeTable typeTable)
        {
            try
            {
                // Create storagecredentials object by reading the values from the configuration (appsettings.json)
                StorageCredentials storageCredentials = new StorageCredentials(accountName: CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountName, 
                    CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountKey);

                // Create cloudstorage account by passing the storagecredentials
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
                CloudBlobContainer container = blobClient.GetContainerReference(CloudStorage.Instance.GetAccountCloudStorage(typeTable).Container);
                container.CreateIfNotExists();
            
                // Get the reference to the block blob from the container
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                string ext = System.IO.Path.GetExtension(fileName).ToLower();
                blockBlob.Properties.ContentType = MimeTypes.GetMimeType(ext);
                // Upload the file
                await blockBlob.UploadFromStreamAsync(fileStream);
                string sas = blockBlob.GetSharedAccessSignature(
                    new SharedAccessBlobPolicy() { 
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessStartTime = DateTime.Now.AddMinutes(-5),
                        SharedAccessExpiryTime = DateTime.Now.AddYears(999) 
                    });

                var url = blockBlob.Uri + sas;
            
                return url;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }

        public static void DeletePhoto(string filename, TypeTable typeTable)
        {
            try
            {
                // Create storagecredentials object by reading the values from the configuration (appsettings.json)
                StorageCredentials storageCredentials = new StorageCredentials(accountName: CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountName, 
                    CloudStorage.Instance.GetAccountCloudStorage(typeTable).AccountKey);

                // Create cloudstorage account by passing the storagecredentials
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
                CloudBlobContainer container = blobClient.GetContainerReference(CloudStorage.Instance.GetAccountCloudStorage(typeTable).Container);
                
            
                // // Get the reference to the block blob from the container
                 var blockBlob = container.GetBlockBlobReference(filename);
                 blockBlob.DeleteIfExistsAsync();
                // string ext = System.IO.Path.GetExtension(fileName).ToLower();
                // blockBlob.Properties.ContentType = MimeTypes.GetMimeType(ext);
                // // Upload the file
                // await blockBlob.UploadFromStreamAsync(fileStream);
                // string sas = blockBlob.GetSharedAccessSignature(
                //     new SharedAccessBlobPolicy() { 
                //         Permissions = SharedAccessBlobPermissions.Read,
                //         SharedAccessStartTime = DateTime.Now.AddMinutes(-5),
                //         SharedAccessExpiryTime = DateTime.Now.AddYears(999) 
                //     });
                //
                // var url = blockBlob.Uri + sas;
                //
                // return url;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}