using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using S3Browser.App.Services;

namespace S3Browser.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Since Blazor is running on the server, we can use an application service
            // to read the forecast data.
            services.AddSingleton(c => new S3BrowserService(new Models.S3Access
            {
                AccessKey = "accessKey1",
                SecretKey = "verySecretKey1",
                ServiceUrl = "http://localhost:8000"
            }));
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
