using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3Browser.App.Models;
using S3Browser.App.Services;
using System;

namespace S3Browser.App
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true);

            Configuration = builder.Build();

            // Since Blazor is running on the server, we can use an application service
            // to read the forecast data.
            services.AddSingleton(c => new S3BrowserService(new S3Access
            {
                AccessKey = Configuration["s3Access:accessKey"],
                SecretKey = Configuration["s3Access:secretKey"],
                ServiceUrl = Configuration["s3Access:serviceUrl"],
                UseHttps = Convert.ToBoolean(Configuration["s3Access:useHttps"])
            }));
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
