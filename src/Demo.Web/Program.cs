using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Demo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();

        var applicationName = builder.Environment.ApplicationName;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        var source = new ActivitySource(applicationName);

        builder.Services.AddSingleton(source);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(applicationName)
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
                .AddSource(applicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithLogging(logging => logging
                .AddOtlpExporter());

        builder.Services.Configure<OpenTelemetryLoggerOptions>(x => x.IncludeScopes = true);

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/", async ([FromServices] HttpClient httpClient, ActivitySource activitySource, ILogger<Program> logger) =>
            {
                var res = await httpClient.GetAsync("https://dummyjson.com/test");

                using (var activity = activitySource.StartActivity("TestActivity"))
                {
                    logger.LogInformation("From TestActivity, structure: {structure}", new { a = "1" });
                    await Task.Delay(500);
                    activity?.AddEvent(new ActivityEvent("Boom!"));
                    await Task.Delay(500);
                }

                return Results.Ok(await res.Content.ReadAsStringAsync());
            })
            .WithName("ExampleApiCall")
            .WithOpenApi();

        await app.RunAsync();
    }
}