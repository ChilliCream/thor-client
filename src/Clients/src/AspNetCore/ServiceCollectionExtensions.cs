using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thor.Core;
using Thor.Core.Session;
using Thor.Core.Transmission.BlobStorage;
using Thor.Core.Transmission.EventHub;
using Thor.Extensions.Http;

namespace Thor.Hosting.AspNetCore
{
    /// <summary>
    /// A bunch of convenient extensions methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <c>Thor Tracing</c> services to the service collection with EventHub and BLOB
        /// storage preconfigured for telemetry and attachment transmission.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance.</param>
        /// <returns>The provided <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddTracing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            TracingConfiguration tracingConfiguration = configuration
                .GetSection("Tracing")
                .Get<TracingConfiguration>();

            if (tracingConfiguration.Enabled)
            {
                // TODO: BlogStorage transmission should also not be active out-of-process
                services
                    .AddTracingHttpMessageHandler(configuration)
                    .AddBlobStorageTelemetryAttachmentTransmission(configuration)
                    .AddTracingMinimum(configuration);

                if (tracingConfiguration.InProcess)
                {
                    services
                        .AddEventHubTelemetryEventTransmission(configuration)
                        .AddInProcessTelemetrySession(configuration);
                }
                else
                {
                    services
                        .AddEmptyTelemetrySession(configuration);
                }
            }

            return services;
        }

        /// <summary>
        /// Adds <c>Thor Tracing Core</c> services to the service collection.
        /// Use this method to configure all by your own.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance.</param>
        /// <returns>The provided <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddTracingMinimum(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services
                .AddTracingCore(configuration)
                .AddSingleton<IStartupFilter, TracingStartupFilter>();
        }
    }
}
