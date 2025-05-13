using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demo.Web.Observability;

public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        var source = new ActivitySource(serviceName);

        services.AddSingleton(source);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes([
                    new KeyValuePair<string, object>("test", "val")
                ]))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("System.Net.Http")
                .AddMeter("System.Net.NameResolution")
                .AddOtlpExporter())
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithLogging(logging => logging
                .AddProcessor<FlatteningLogProcessor>()
                .AddOtlpExporter());

        services.Configure<OpenTelemetryLoggerOptions>(x => x.IncludeScopes = true);
    }
}