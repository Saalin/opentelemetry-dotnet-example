using System.Threading.Tasks;
using Demo.Web.Endpoints;
using Demo.Web.Extensions;
using Demo.Web.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo.Web;

public class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();

        var applicationName = builder.Environment.ApplicationName;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddOpenTelemetry(applicationName);

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.ConfigureForwarderHeaders();

        app.UseRouting();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapDummy();
        app.MapHealthChecks("/healthz");

        return app.RunAsync();
    }
}