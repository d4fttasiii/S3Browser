using Microsoft.AspNetCore.Blazor.Components;
using S3Browser.App.Models;
using S3Browser.App.Services;

namespace S3Browser.App.PageControllers
{
    public class SettingsPage : BlazorComponent
    {
        [Inject]
        protected S3BrowserService S3BrowserService { get; set; }

        public S3Access S3Access { get; set; }

        protected override void OnInit()
        {
            S3Access = S3BrowserService.S3Access;
        }

        public void ToggleUseHttps()
        {
            S3Access.UseHttps = !S3Access.UseHttps;
        }

        public void Connect()
        {
            S3BrowserService.Connect(S3Access);
        }
    }
}
