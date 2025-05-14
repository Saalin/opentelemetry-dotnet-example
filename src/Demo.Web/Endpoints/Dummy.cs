using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Demo.Web.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Demo.Web.Endpoints;

public static class DummyEndpooints
{
    public static void MapDummy(this WebApplication app)
    {
        app.MapGet("/",
                async (HttpClient httpClient, ActivitySource activitySource, ILogger<Program> logger,
                    HttpContext httpContext) =>
                {
                    var res = await httpClient.GetAsync("https://dummyjson.com/test");

                    using (var activity = activitySource.StartActivity(httpContext))
                    {
                        logger.LogInformation("From TestActivity, structure: {@Structure}",
                            new { a = "42", b = new { x = "422" } });
                        await Task.Delay(500);
                        activity?.AddEvent(new ActivityEvent("Boom!"));
                        await Task.Delay(500);
                    }

                    return Results.Ok(await res.Content.ReadAsStringAsync());
                })
            .WithName("ExampleApiCall")
            .WithOpenApi();
    }
}