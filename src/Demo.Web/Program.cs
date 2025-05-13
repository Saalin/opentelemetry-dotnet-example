using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Demo.Web.Observability;

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

        builder.Services.AddOpenTelemetry(serviceName: applicationName);
        
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapHealthChecks("/healthz");
        
        app.MapGet("/", async ([FromServices] HttpClient httpClient, ActivitySource activitySource, ILogger<Program> logger) =>
            {
                var res = await httpClient.GetAsync("https://dummyjson.com/test");

                using (var activity = activitySource.StartActivity())
                {
                    logger.LogInformation("From TestActivity, structure: {@Structure}", new { a = "42", b = new { x = "422"} });
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