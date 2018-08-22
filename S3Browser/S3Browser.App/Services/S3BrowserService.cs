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
        private IAmazonS3 _client;

        public S3Access S3Access { get; private set; }

        public S3BrowserService(S3Access s3Access)
        {
            Connect(s3Access);
        }

        public void Connect(S3Access s3Access)
        {
            _client = new AmazonS3Client(
                new BasicAWSCredentials(s3Access.AccessKey, s3Access.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = s3Access.ServiceUrl,
                    UseHttp = s3Access.UseHttps,
                    ForcePathStyle = true
                }
            );
            S3Access = s3Access;
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

        public async Task<IEnumerable<S3Element>> GetFileListAsync(string bucketName, string prefix = "", string filter = "", int take = 50, int skip = 0)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                prefix = prefix.Last() != '/' ? $"{prefix}/" : prefix;
            }

            var request = new ListObjectsRequest
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var response = await _client.ListObjectsAsync(request);

            var folders = response.S3Objects
                .Where(o => !string.IsNullOrWhiteSpace(filter) ? o.Key.Contains(filter) : true)
                .Where(o =>
                {
                    var path = string.IsNullOrEmpty(prefix) ? o.Key : o.Key.Replace(prefix, "");

                    return path.Split('/').Count() >= 2;
                })
                .GroupBy(o =>
                {
                    var path = string.IsNullOrEmpty(prefix) ? o.Key : o.Key.Replace(prefix, "");
                    return path.Split('/').First();
                })
                .Select(g =>
                {
                    return new S3Element
                    {
                        Key = g.Key,
                        Name = g.Key,
                        // Path = $"{bucketName}/{prefix}/{g.Key}",
                        IsFolder = true
                    };
                })
                .ToList()
                .Skip(skip)
                .Take(take)
                ;

            var files = response.S3Objects
                .Where(s3o => !string.IsNullOrWhiteSpace(filter) ? s3o.Key.Contains(filter) : true)
                .Where(o =>
                {
                    var path = string.IsNullOrEmpty(prefix) ? o.Key : o.Key.Replace(prefix, "");

                    return path.Split('/').Count() == 1 && !string.IsNullOrWhiteSpace(path);
                })
                .Skip(skip)
                .Take(take - folders.Count())
                .Select(o => new S3Element
                {
                    Key = o.Key,
                    Name = o.Key.Split('/').Last(),
                    LastModified = o.LastModified,
                    IsFolder = false
                });

            return folders.Union(files);
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

        public async Task<string> GetContentBase64Async(string bucketName, string path)
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
                var byteArray = ms.ToArray();

                return Convert.ToBase64String(byteArray);
            }
        }

        public async Task UploadBytesAsync(string bucketName, string key, byte[] content)
        {
            using (Stream stream = new MemoryStream(content))
            {
                await _client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream
                });
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
