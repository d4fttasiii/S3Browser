using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using S3Browser.App.Models;
using S3Browser.App.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S3Browser.App.PageControllers
{
    public class FilesPage : BlazorComponent
    {
        [Inject]
        protected S3BrowserService S3BrowserService { get; set; }
        [Parameter]
        protected string BucketName { get; set; }
        [Parameter]
        protected string Prefix { get; set; }

        public string SearchText { get; set; } = "";
        public bool Loaded { get; set; } = false;
        public bool AllLoaded { get; set; } = false;
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 5;
        public List<S3Element> Files { get; set; } = new List<S3Element>();

        protected override async Task OnInitAsync()
        {
            await LoadFilesAsync();
        }

        public async Task LoadFilesAsync()
        {
            Loaded = false;
            Files = (await S3BrowserService.GetFileListAsync(BucketName, System.Web.HttpUtility.UrlDecode(Prefix ?? ""), SearchText, Size, Page * Size)).ToList();
            Loaded = true;
            AllLoaded = Files.Count < Size;
        }

        public async Task LoadMoreFilesAsync()
        {
            if (AllLoaded)
            {
                return;
            }
            Page++;
            var moreFiles = await S3BrowserService.GetFileListAsync(BucketName, System.Web.HttpUtility.UrlDecode(Prefix ?? ""), SearchText, Size, Page * Size);
            if (!moreFiles.Any())
            {
                AllLoaded = true;
                return;
            }

            Files.AddRange(moreFiles);
        }

        public async Task DownloadFileAsync(string key)
        {
            var base64 = await S3BrowserService.GetContentBase64Async(BucketName, key);
            var fileName = key.Split('/').Last();
            await JSRuntime.Current.InvokeAsync<string>("downloadFile", fileName, base64);
        }

        public async Task DeleteFileAsync(string key)
        {
            var fileToRemove = Files.First(f => f.Key == key);
            Files.Remove(fileToRemove);
            await S3BrowserService.DeleteFileAsync(BucketName, key);
        }
    }
}
