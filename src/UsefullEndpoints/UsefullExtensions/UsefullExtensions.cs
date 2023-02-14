using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Text;

namespace UsefullExtensions;

public class MiddlewareShutdown : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (UsefullExtensions.RequestedShutdownAt != null)
        {
            context.Response.StatusCode = 418;
            await context.Response.WriteAsync("Service is stopping at " + UsefullExtensions.RequestedShutdownAt!.Value.ToString("s"));
            return;
        }
        await next(context);
        return;
    }
}


public static class UsefullExtensions
{
    private static DateTime startDate = DateTime.Now;
        private static DateTime startDateUTC = DateTime.UtcNow;
    public static DateTime? RequestedShutdownAt = null;
    public static CancellationTokenSource cts=new ();

        

    public static void MapUsefullAll(this IEndpointRouteBuilder route, string? cors = null, string[]? authorization = null)
    {
       
        route.MapUsefullStartDate(cors, authorization);
        route.MapUsefullUser(cors, authorization);
        route.MapUsefullEnvironment(cors, authorization);
        route.MapUsefullError(cors, authorization);
        route.MapUsefullDate(cors, authorization);
        route.MapUsefullEndpoints(cors, authorization);
        route.MapUsefullConfiguration(cors, authorization);
        route.MapUsefullContext(cors, authorization);
        route.MapShutdown(cors, authorization);
    }
    private static void AddDefault(this RouteHandlerBuilder rh, string? corsPolicy = null, string[]? authorization = null)
    {
        rh=rh.WithTags("NetCoreUsefullEndpoints").WithOpenApi();
        if (authorization?.Length > 0)
        {
            if (authorization.Length == 1 && string.IsNullOrWhiteSpace(authorization[0]))
                rh.RequireAuthorization();

            else
                rh.RequireAuthorization(authorization);
        }
        else
        {
            rh = rh.AllowAnonymous();
        }

        if (!string.IsNullOrWhiteSpace(corsPolicy))
            rh = rh.RequireCors(corsPolicy);
        
    }
    public static void MapUsefullStartDate(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/date/start", (HttpContext httpContext) =>
        {
            return Results.Ok(startDate);
        });

        rh.AddDefault(corsPolicy, authorization);

        var rhUTC = route.MapGet("api/usefull/date/startUTC", (HttpContext httpContext) =>
        {
            return Results.Ok(startDateUTC);
        });

        rhUTC.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullUser(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/user/authorization", (HttpContext httpContext) =>
        {
            return Results.Ok(httpContext.User);
        }).WithTags("NetCoreUsefullEndpoints")
        .WithOpenApi(); 

        if (corsPolicy?.Length > 0)
            rh = rh.RequireCors(corsPolicy);

        if (authorization?.Length > 0 && authorization[0]?.Length > 0)
            rh = rh.RequireAuthorization(authorization);


        rh = route.MapGet("api/usefull/user/noAuthorization", (HttpContext httpContext) =>
        {
            return Results.Ok(httpContext.User);
        }).AllowAnonymous().WithTags("NetCoreUsefullEndpoints").WithOpenApi();

        if (corsPolicy?.Length > 0)
            rh = rh.RequireCors(corsPolicy);

    }
    public static void MapUsefullContext(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        route.MapGet("api/usefull/httpContext/Connection",

            Results<NoContent, Ok<object>>
            (HttpContext httpContext) =>
        {
            var con = httpContext.Connection;
            if (con == null)
            {
                return TypedResults.NoContent();
            }
            var conSerialize = new
            {
                LocalIpAddress = con.LocalIpAddress?.ToString(),
                RemoteIpAddress = con.RemoteIpAddress?.ToString(),
                con.RemotePort,
                con.LocalPort,
                con.ClientCertificate,
                con.Id
            };
            return TypedResults.Ok((object)conSerialize);
        }).AddDefault(corsPolicy, authorization);
    }
    public static void MapUsefullConfiguration(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/configuration/",
            Results<NoContent, ContentHttpResult>
            ([FromServices] IConfiguration config) =>
        {
            var c = config as IConfigurationRoot;
            if (c != null)
            {
                return TypedResults.Content(c.GetDebugView());
            }
            else
            {
                return TypedResults.NoContent();
            }
        });
        rh.AddDefault(corsPolicy, authorization);
    }
    /// <summary>
    /// use with await app.RunAsync(UsefullExtensions.UsefullExtensions.cts.Token);
    /// </summary>
    /// <param name="route"></param>
    /// <param name="corsPolicy"></param>
    /// <param name="authorization"></param>
    public static void MapShutdown(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapPost("api/usefull/shutdown/",
            (HttpContext httpContext) =>
            {
                var h= cts.Token.GetHashCode();
                cts?.Cancel();                    
                return h;
                
            });

        rh.AddDefault(corsPolicy, authorization);
        var rhSec = route.MapPost("api/usefull/shutdownAfter/{seconds}",
            (HttpContext httpContext, int seconds) =>
            {
                RequestedShutdownAt = DateTime.UtcNow;
                var h = cts.Token.GetHashCode();
                cts?.CancelAfter(Math.Abs(seconds)*1000);
                return h;

            });
        rhSec.AddDefault(corsPolicy, authorization);

        var rhForced = route.MapPost("api/usefull/shutdownForced/{id:int}",
            (int id) =>
            {
                Environment.Exit(id);
            });

        rhForced.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullDate(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/date/now",
            (HttpContext httpContext) =>
        {
            return TypedResults.Ok(DateTime.Now);
            //return Results.Ok(DateTime.Now);
        });

        rh.AddDefault(corsPolicy, authorization);

        var rhUTC = route.MapGet("api/usefull/date/nowUTC/",
            (HttpContext httpContext) =>
            {
                return TypedResults.Ok(DateTime.UtcNow);
                //return Results.Ok(DateTime.Now);
            });

        rhUTC.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullEnvironment(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);

        var rh = route.MapGet("api/usefull/environment/", (HttpContext httpContext) =>
        {
            return TypedResults.Ok(new Helper().FromStaticEnvironment());
        });
        rh.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullError(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);

        var rh = route.MapGet("api/usefull/errorWithILogger", (HttpContext httpContext, [FromServices] ILogger logger) =>
        {
            try
            {
                //var id = httpContext.Request.RouteValues["id"] as string;
                throw new ArgumentException("do not support ");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "usefull error");
                throw;
            }
        });
        rh.AddDefault(corsPolicy, authorization);
        rh = route.MapGet("api/usefull/errorPure", (HttpContext httpContext) =>
        {

            try
            {
                var x = 0;
                x++;
                x = (x - 1) / (x - 1);
                return TypedResults.Ok("fake x");
            }
            catch (Exception)
            {
                throw;
            }
        });
        rh.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullEndpoints(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        var rh = route.MapGet("api/usefull/endpoints/graph", (HttpContext httpContext, [FromServices] DfaGraphWriter graphWriter, [FromServices] EndpointDataSource dataSource) =>
        {
            using (var sw = new StringWriter())
            {
                // Write the graph
                graphWriter.Write(dataSource, sw);
                var graph = sw.ToString();

                // Write the graph to the response
                return TypedResults.Content(graph);
            }
        });
        rh.AddDefault(corsPolicy, authorization);

        var rhText=route.MapGet("api/usefull/endpoints/text", (HttpContext httpContext, [FromServices] IEnumerable<EndpointDataSource> endpointSources) =>
        {
            var endpoints = endpointSources.SelectMany(es => es.Endpoints);
            var res = endpoints.Select(endpoint =>
            new
            {
                name = endpoint.DisplayName,
                routeName = endpoint.Metadata.OfType<RouteNameMetadata>().FirstOrDefault(),
                httpMethod = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()

            }).ToArray();
            return TypedResults.Ok(res);
        });
        rhText.AddDefault(corsPolicy, authorization);

    }
}
