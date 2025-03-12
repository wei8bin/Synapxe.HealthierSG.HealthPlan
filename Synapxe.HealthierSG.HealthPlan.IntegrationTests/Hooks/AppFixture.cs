using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Synapxe.HealthierSG.HealthPlan.IntegrationTests
{
    internal static class AppFixture
    {
        private const string EnvironmentName = "Integration";

        internal static async Task<DistributedApplication> CreateAppAsync<TStartup>()
            where TStartup : class
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<TStartup>([], (doi, opts) => { doi.DisableDashboard = true; });
            var resources = appHost.Resources.OfType<ProjectResource>().Select(appHost.CreateResourceBuilder);
            foreach (var resource in resources)
            {
                resource.WithEnvironment("ASPNETCORE_ENVIRONMENT", EnvironmentName);
                resource.WithEnvironment("ASPNETCORE_CONTENTROOT", Directory.GetCurrentDirectory());
            }

            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                // we skip handler validation for self-signed certificates
                clientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
                clientBuilder.AddStandardResilienceHandler(options =>
                {
                    options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(1);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(2); // needs to be at least double the AttemptTimeout to pass options validation
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(4);
                });
            });

            var app = await appHost.BuildAsync();
            var resourceNotificationSvc = app.Services.GetRequiredService<ResourceNotificationService>();
            await app.StartAsync();

            await resourceNotificationSvc.WaitForResourceAsync("server", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

            return app;
        }
    }
}
