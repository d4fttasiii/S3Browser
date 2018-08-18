using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using S3Browser.App.Models;
using S3Browser.App.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace S3Browser.App.PageControllers
{
    public class FilesPage : BlazorComponent
    {
        [Inject]
        protected Microsoft.AspNetCore.Blazor.Services.IUriHelper UriHelper { get; set; }
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
        public List<S3Element> Elements { get; set; } = new List<S3Element>();
        public List<string> BreadCrumbs { get; set; } = new List<string>();

        protected override async Task OnInitAsync()
        {
            await LoadFilesAsync();
        }

        protected async override Task OnParametersSetAsync()
        {
            await LoadFilesAsync();

            if (!string.IsNullOrWhiteSpace(DecodedPrefix))
            {
                BreadCrumbs = DecodedPrefix.Split('/').ToList();
            }
        }

        protected string EncodedPrefix => System.Web.HttpUtility.UrlEncode(Prefix ?? "");
        protected string DecodedPrefix => System.Web.HttpUtility.UrlDecode(Prefix ?? "");

        protected string ToFolderPath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return $"/buckets/{BucketName}";
            }

            if (string.IsNullOrWhiteSpace(EncodedPrefix))
            {
                return $"/buckets/{BucketName}/{key}";
            }

            return $"/buckets/{BucketName}/{System.Web.HttpUtility.UrlEncode($"{Prefix}/{key}")}";
        } 

        public async Task LoadFilesAsync()
        {
            Loaded = false;
            Elements = (await S3BrowserService.GetFileListAsync(BucketName, DecodedPrefix, SearchText, Size, Page * Size)).ToList();
            Loaded = true;
            AllLoaded = Elements.Count < Size;
        }

        public async Task LoadMoreFilesAsync()
        {
            if (AllLoaded)
            {
                return;
            }
            Page++;
            var moreFiles = await S3BrowserService.GetFileListAsync(BucketName, DecodedPrefix, SearchText, Size, Page * Size);
            if (!moreFiles.Any())
            {
                AllLoaded = true;
                return;
            }

            Elements.AddRange(moreFiles);
        }

        public void OpenFolder(string key)
        {
            UriHelper.NavigateTo(ToFolderPath(key));
        }

        public async Task DownloadFileAsync(string key)
        {
            var base64 = await S3BrowserService.GetContentBase64Async(BucketName, key);
            var fileName = key.Split('/').Last();
            await JSRuntime.Current.InvokeAsync<string>("downloadFile", fileName, base64);
        }

        public async Task DeleteFileAsync(string key)
        {
            var fileToRemove = Elements.First(f => f.Key == key);
            Elements.Remove(fileToRemove);
            await S3BrowserService.DeleteFileAsync(BucketName, key);
        }
    }
}
