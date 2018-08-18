using System;
namespace S3Browser.App.Models
{
    public class S3Element
    {
        public string Key { get; set; }

        public string Path { get; set; }

        public bool IsFolder { get; set; }

        public DateTime LastModified { get; set; }
    }
}
