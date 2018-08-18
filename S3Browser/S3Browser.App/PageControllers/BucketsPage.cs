using Amazon.S3.Model;
using Microsoft.AspNetCore.Blazor.Components;
using S3Browser.App.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S3Browser.App.PageControllers
{
    public class BucketsPage : BlazorComponent
    {
        [Inject]
        protected S3BrowserService S3BrowserService { get; set; }
        public string SearchText { get; set; } = "";
        public bool Loaded { get; set; } = false;
        public IEnumerable<S3Bucket> Buckets { get; set; } = new List<S3Bucket>();

        protected override async Task OnInitAsync()
        {
            await LoadBucketListAsync();
        }

        public async Task LoadBucketListAsync()
        {
            Loaded = false;
            Buckets = await S3BrowserService.GetBucketListAsync(SearchText);
            Loaded = true;
        }
    }
}
