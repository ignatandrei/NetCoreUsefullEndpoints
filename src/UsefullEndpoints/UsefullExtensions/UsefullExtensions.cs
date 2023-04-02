﻿namespace UsefullExtensions;

public static class UsefullExtensions
{
    private static DateTime startDate = DateTime.Now;
        private static DateTime startDateUTC = DateTime.UtcNow;
    public static DateTime? RequestedShutdownAt = null;
    public static CancellationTokenSource cts=new ();
    internal static Dictionary<string, LongRunningTask> lrts = new();
    public static LongRunningTask AddLRTS(string id, string? name = null)
    {
        if(lrts.ContainsKey(id))
        {
            lrts[id].Dispose();
        }
        lrts.Add(id,new LongRunningTask(id, name ?? id));
        return lrts[id];
    }

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
        route.MapUsefullShutdown(cors, authorization);
        route.MapUsefullLRTS(cors, authorization);
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
    public record UserRet (string? name,string? authType, bool isAuthenticated); 
    public static void MapUsefullUser(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/user/authorization", (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            if(user == null)
                return Results.Ok((UserRet?)null);
            if (user.Identity != null)
                return Results.Ok((UserRet?)new UserRet(
                
                    user.Identity.Name,
                    user.Identity.AuthenticationType,
                    user.Identity.IsAuthenticated

                ));

            var auth = user.Identities.FirstOrDefault(it => it.IsAuthenticated);
            if(auth == null)
                return Results.Ok((UserRet?)null);
            return Results.Ok((UserRet?)new UserRet(

                   auth.Name,
                   auth.AuthenticationType,
                   auth.IsAuthenticated

               ));

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
    public static void MapUsefullShutdown(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
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
    public static void MapUsefullLRTS(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/LongRunningTasks/",
            (HttpContext httpContext) =>
            {
                var data=lrts.Select(it=>new { it.Key, it.Value.name }).ToArray();
                return data;
            });

        rh.AddDefault(corsPolicy, authorization);
        var rhCount = route.MapGet("api/usefull/LongRunningTasks/Count",
            (HttpContext httpContext) =>
            {
                return lrts.LongCount();
            });

        rhCount.AddDefault(corsPolicy, authorization);
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

        var rh = route.MapGet("api/usefull/error/WithILogger", (HttpContext httpContext, [FromServices] ILogger<GenericLogging> logger) =>
        {
            try
            {
                //var id = httpContext.Request.RouteValues["id"] as string;
                throw new ArgumentException("this is a fake argument");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "usefull , but fake, error");
                throw;
            }
        });

        rh.AddDefault(corsPolicy, authorization);

        var rhEvent = route.MapGet("api/usefull/error/WithEvtId/{eventId}/{name}", (HttpContext httpContext, [FromServices] ILogger<GenericLogging> logger, [FromQuery] int eventId, [FromQuery] string name ) =>
        {
            try
            {
                //var id = httpContext.Request.RouteValues["id"] as string;
                throw new ArgumentException("this is a fake argument");
            }
            catch (Exception ex)
            {
                
                var evt = new EventId(eventId, name);
                logger?.LogError(evt,ex, "usefull , but fake, error");
                throw;
            }
        });

        rhEvent.AddDefault(corsPolicy, authorization);

        var rhPure = route.MapGet("api/usefull/error/Pure", (HttpContext httpContext) =>
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
        rhPure.AddDefault(corsPolicy, authorization);

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
