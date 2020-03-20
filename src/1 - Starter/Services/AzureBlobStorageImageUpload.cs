using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;


namespace RealEstate.Services
{
	public class AzureBlobStorageImageUpload : IImageUpload
	{
		private string _accountName = "";
		private string _accountKey = "";
		private string _baseUri = "";
		private string _containerName = "";

		public AzureBlobStorageImageUpload(
				string accountName,
				string accountKey,
				string baseUri,
				string containerName)
		{
			_accountName = accountName;
			_accountKey = accountKey;
			_baseUri = baseUri;
			_containerName = containerName;
		}


		public async Task<string> StoreImage(string filename, Stream imageStream)
		{
			// create connection to Azure storage
			var creds = new StorageCredentials(_accountName, _accountKey);
			var storageUri = new StorageUri(new Uri(_baseUri));
			var storageAccount = new CloudStorageAccount(creds, storageUri, null, null, null);

			// create a blob client and get access to the container
			var blobClient = storageAccount.CreateCloudBlobClient();
			var container = blobClient.GetContainerReference(_containerName);
			await container.CreateIfNotExistsAsync();

			// get the blob reference and set permissions
			var blobPermissions = new BlobContainerPermissions() {
				PublicAccess = BlobContainerPublicAccessType.Blob
			};
			await container.SetPermissionsAsync(blobPermissions);
			var blob = container.GetBlockBlobReference(filename);

			// upload the image data to the blob - delete it if it already exists
			await blob.DeleteIfExistsAsync();
			blob.Properties.ContentType = "image/jpeg";
			await blob.UploadFromStreamAsync(imageStream);

			// return a url to the uploaded image
			return blob.Uri.AbsoluteUri;
		}
	}
}
