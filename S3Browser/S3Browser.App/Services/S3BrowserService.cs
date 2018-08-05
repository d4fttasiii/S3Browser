using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using S3Browser.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace S3Browser.App.Services
{
    public class S3BrowserService
    {
        private readonly IAmazonS3 _client;

        public S3BrowserService(S3Access s3Access)
        {
            _client = new AmazonS3Client(
                new BasicAWSCredentials(s3Access.AccessKey, s3Access.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = s3Access.ServiceUrl,
                    UseHttp = true,
                    ForcePathStyle = true
                }
            );
        }

        public async Task<IEnumerable<S3Bucket>> GetBucketListAsync(string searchText = "")
        {
            var response = await _client.ListBucketsAsync();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return response.Buckets;
            }

            return response.Buckets.Where(b => b.BucketName.ToLower().Contains(searchText.ToLower()));
        }

        public async Task<IEnumerable<S3Object>> GetFileListAsync(string bucketName, string prefix = "", int take = 50, int skip = 0)
        {
            var request = new ListObjectsRequest
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var response = await _client.ListObjectsAsync(request);

            return response.S3Objects
                .Where(s3o => s3o.Size > 0)
                .Skip(0)
                .Take(50);
        }

        public async Task<byte[]> GetContentBytesAsync(string bucketName, string path)
        {
            byte[] buffer = new byte[16 * 1024];

            using (var stream = await GetStreamAsync(bucketName, path))
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task DeleteFileAsync(string bucketName, string path)
        {
            await _client.DeleteObjectAsync(bucketName, path);
        }

        private async Task<Stream> GetStreamAsync(string bucketName, string path)
        {
            var response = await _client.GetObjectAsync(bucketName, path);

            return response.ResponseStream;
        }
    }
}
