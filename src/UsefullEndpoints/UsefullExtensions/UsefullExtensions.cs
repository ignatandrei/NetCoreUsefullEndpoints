using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace UsefullExtensions;

public static class UsefullExtensions
{
    private static DateTime startDate = DateTime.Now;
    private static DateTime startDateUTC = DateTime.UtcNow;
    public static DateTime? RequestedShutdownAt = null;
    public static CancellationTokenSource cts = new();
    private static string MSFTBackGround = "Microsoft.AspNetCore.Hosting.GenericWebHostService";
    internal static Dictionary<string, LongRunningTask> lrts = new();
    public static LongRunningTask AddLRTS(string id, string? name = null)
    {
        if (lrts.ContainsKey(id))
        {
            lrts[id].Dispose();
        }
        lrts.Add(id, new LongRunningTask(id, name ?? id));
        return lrts[id];
    }
    public static void MapHostedServices(this IEndpointRouteBuilder route, IHostedService[] services, string? cors = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rhList = route.MapGet("api/usefull/services/list", Results<Ok<string[]>, NoContent> () =>
        {
            if (services.Length == 0)
                return TypedResults.NoContent();

            var data = services
            .Where(it => it != null)
            .Select(it => it.GetType().FullName ?? "")
            .Where(it => it != MSFTBackGround)
            .ToArray();
            return TypedResults.Ok(data);
        });

        rhList.AddDefault(cors, authorization);

        var rhClose = route.MapPost("api/usefull/services/closeall", async Task<Results<Ok<string>, BadRequest<string[]>>> () =>
        {
            List<string> badServices = new();
            if (services.Length > 0)
                foreach (var service in services)
                {

                    try
                    {
                        if (service == null) continue;
                        if (service.GetType().FullName == MSFTBackGround) continue;
                        await service.StopAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        badServices.Add(service.GetType().Name + " ex: " + ex.Message);
                    }
                }
            if (badServices.Count > 0)
            {
                return TypedResults.BadRequest(badServices.ToArray());
                //return TypedResults.NotFound();
            }

            return TypedResults.Ok("services number:" + services.Length);
        });

        rhClose.AddDefault(cors, authorization);
        var rhStart = route.MapPost("api/usefull/services/startall", async Task<Results<Ok<string>, BadRequest<string[]>>> (HttpContext httpContext) =>
        {
            List<string> badServices = new();
            if (services.Length > 0)
                foreach (var service in services)
                {
                    try
                    {
                        if (service == null) continue;
                        if (service.GetType().FullName == MSFTBackGround) continue;
                        await service.StartAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        badServices.Add(service.GetType().Name + " ex: " + ex.Message);
                    }
                }
            if (badServices.Count > 0)
            {
                return TypedResults.BadRequest<string[]>(badServices.ToArray());
            }

            return TypedResults.Ok("services number:" + services.Length);
        });

        rhStart.AddDefault(cors, authorization);

    }
    public static void MapUsefullAll(this IEndpointRouteBuilder route, string? cors = null, string[]? authorization = null)
    {

        route.MapUsefullStartDate(cors, authorization);
        route.MapUsefullUser(cors, authorization);
        route.MapUsefullEnvironment(cors, authorization);
        route.MapUsefullError(cors, authorization);
        route.MapUsefullDate(cors, authorization);
        route.MapUsefullStartHost(cors, authorization);
        route.MapUsefullEndpoints(cors, authorization);
        route.MapUsefullConfiguration(cors, authorization);
        route.MapUsefullConfigurationKV(cors, authorization);
        route.MapUsefullContext(cors, authorization);
        route.MapUsefullShutdown(cors, authorization);
        route.MapUsefullLRTS(cors, authorization);
        route.MapUsefullProcess(cors, authorization);
        route.MapUsefullRuntimeInformation(cors, authorization);
        route.MapUsefullAdresses(cors, authorization);
    }
    private static void AddDefault(this RouteHandlerBuilder rh, string? corsPolicy = null, string[]? authorization = null)
    {
        rh = rh.WithTags("NetCoreUsefullEndpoints").WithOpenApi();
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
    public static void MapUsefullProcess(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/process/", (HttpContext httpContext) =>
        {
            var p = Process.GetCurrentProcess();
            var data = new
            {
                p.Id,
                p.ProcessName,
                p.StartTime,
                p.TotalProcessorTime,
                ThreadsCount = p.Threads.Count,
                p.WorkingSet64,
                p.PrivateMemorySize64,
                p.PagedMemorySize64,
                p.PagedSystemMemorySize64,
                p.PeakPagedMemorySize64,
                p.PeakVirtualMemorySize64,
                p.PeakWorkingSet64,
                p.VirtualMemorySize64,
                p.BasePriority,
                p.HandleCount,
                p.MachineName,
                PriorityClassName = p.PriorityClass.ToString(),
                p.PriorityClass,
                p.NonpagedSystemMemorySize64,
                p.MainModule?.FileName,
                MinWorkingSet = (long)p.MinWorkingSet,
                MaxWorkingSet = (long)p.MaxWorkingSet,
                TotalProcessorTimeSeconds = p.TotalProcessorTime.TotalSeconds,
                TotalUserProcessorTimeSeconds = p.UserProcessorTime.TotalSeconds,
                TotalPrivilegedProcessorTimeSeconds = p.PrivilegedProcessorTime.TotalSeconds,
                FileVersionInfoShort = new
                {
                    p.MainModule?.FileVersionInfo?.FileVersion,
                    p.MainModule?.FileVersionInfo?.FileName,
                    p.MainModule?.FileVersionInfo?.FileDescription,
                    p.MainModule?.FileVersionInfo?.OriginalFilename,
                    p.MainModule?.FileVersionInfo?.ProductVersion,
                },
                p.MainModule?.FileVersionInfo

            };
            return Results.Ok(data);
        });

        rh.AddDefault(corsPolicy, authorization);


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
    public record UserRet(string? name, string? authType, bool isAuthenticated);
    public static void MapUsefullUser(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/user/authorization", (HttpContext httpContext) =>
        {
            var user = httpContext.User;
            if (user == null)
                return Results.Ok((UserRet?)null);
            if (user.Identity != null)
                return Results.Ok((UserRet?)new UserRet(

                    user.Identity.Name,
                    user.Identity.AuthenticationType,
                    user.Identity.IsAuthenticated

                ));

            var auth = user.Identities.FirstOrDefault(it => it.IsAuthenticated);
            if (auth == null)
                return Results.Ok((UserRet?)null);
            return Results.Ok((UserRet?)new UserRet(

                   auth.Name,
                   auth.AuthenticationType,
                   auth.IsAuthenticated

               ));

        }).WithTags("NetCoreUsefullEndpoints")
        .WithOpenApi();

        rh.AddDefault(corsPolicy, authorization);

        rh = route.MapGet("api/usefull/user/noAuthorization", (HttpContext httpContext) =>
        {
            return Results.Ok(httpContext.User);
        }).AllowAnonymous().WithTags("NetCoreUsefullEndpoints").WithOpenApi();

        if (corsPolicy?.Length > 0)
            rh = rh.RequireCors(corsPolicy);

        rh = route.MapGet("api/usefull/user/isInRole/{roleName}", (HttpContext httpContext, string roleName) =>
        {            
            return httpContext.User?.IsInRole(roleName)??false;

        }).WithTags("NetCoreUsefullEndpoints")
        .WithOpenApi();

        rh.AddDefault(corsPolicy, authorization);

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
    public static void MapUsefullConfigurationKV(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/configurationKV/",
            Results<NoContent, Ok<Dictionary<string, string?>>>
            ([FromServices] IConfiguration c) =>
            {

                if (c != null)
                {
                    var data =
                        c.GetChildren()
                        .Select(it => new { it.Key, it.Value })
                        .ToDictionary(it => it.Key, it => it.Value);
                    ArgumentNullException.ThrowIfNull(data);
                    return TypedResults.Ok(data);
                }
                else
                {
                    return TypedResults.NoContent();
                }
            });
        rh.AddDefault(corsPolicy, authorization);
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
                var h = cts.Token.GetHashCode();
                cts?.Cancel();
                return h;

            });

        rh.AddDefault(corsPolicy, authorization);
        var rhSec = route.MapPost("api/usefull/shutdownAfter/{seconds}",
            (HttpContext httpContext, int seconds) =>
            {
                RequestedShutdownAt = DateTime.UtcNow;
                var h = cts.Token.GetHashCode();
                cts?.CancelAfter(Math.Abs(seconds) * 1000);
                return h;

            });
        rhSec.AddDefault(corsPolicy, authorization);

        var rhForced = route.MapPost("api/usefull/shutdownForced/{id:int}",
            (int id) =>
            {
                Environment.Exit(id);
            });

        rhForced.AddDefault(corsPolicy, authorization);

        var rhGrace = route.MapPost("api/usefull/shutdownGrace",
            (HttpContext httpContext, IHostApplicationLifetime life) =>
            {
                life.StopApplication();
                return;

            });
        rhGrace.AddDefault(corsPolicy, authorization);



    }
    public static void MapUsefullLRTS(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/LongRunningTasks/",
            (HttpContext httpContext) =>
            {
                var data = lrts.Select(it => new { it.Key, it.Value.name }).ToArray();
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
    public static void MapUsefullStartHost(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);
        var rh = route.MapGet("api/usefull/startHost/ticks",
            (HttpContext httpContext) =>
            {
                return TypedResults.Ok(Environment.TickCount64);

            });

        rh.AddDefault(corsPolicy, authorization);

        var rhUTC = route.MapGet("api/usefull/startHost/dateUTC/",
            (HttpContext httpContext) =>
            {
                var dt = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount64);
                return TypedResults.Ok(dt);
                //return Results.Ok(DateTime.Now);
            });

        rhUTC.AddDefault(corsPolicy, authorization);
        var rhLocal = route.MapGet("api/usefull/startHost/dateHost/",
            (HttpContext httpContext) =>
            {
                var dt = DateTime.Now.AddMilliseconds(-Environment.TickCount64);
                return TypedResults.Ok(dt);
                //return Results.Ok(DateTime.Now);
            });

        rhLocal.AddDefault(corsPolicy, authorization);

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
            return TypedResults.Ok(new
            {
                Environment.CommandLine,
                Environment.CurrentDirectory,
                Environment.CpuUsage,
                Environment.CurrentManagedThreadId,
                Environment.ProcessorCount,
                Environment.ProcessId,
                Environment.IsPrivilegedProcess,
                Environment.OSVersion,
                Environment.UserName
                //TODO: add more....
            });
        });
        rh.AddDefault(corsPolicy, authorization);

    }
    public static void MapUsefullRuntimeInformation(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        ArgumentNullException.ThrowIfNull(route);

        var rh = route.MapGet("api/usefull/runtimeinformation/", (HttpContext httpContext) =>
        {
            return TypedResults.Ok(new Helper().FromStaticRuntimeInformation());
        });
        rh.AddDefault(corsPolicy, authorization);
        var rh1 = route.MapGet("api/usefull/runtimeinformationAll/", (HttpContext httpContext) =>
        {
            var info = new Helper().FromStaticRuntimeInformation();
            return TypedResults.Ok(new
            {
                info.FrameworkDescription,
                info.OSDescription,
                ProcessArchitecture = info.ProcessArchitecture.ToString(),
                OSArchitecture = info.OSArchitecture.ToString(),
            });
        });
        rh1.AddDefault(corsPolicy, authorization);

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

        var rhEvent = route.MapGet("api/usefull/error/WithEvtId/{eventId}/{name}", (HttpContext httpContext, [FromServices] ILogger<GenericLogging> logger, [FromQuery] int eventId, [FromQuery] string name) =>
        {
            try
            {
                //var id = httpContext.Request.RouteValues["id"] as string;
                throw new ArgumentException("this is a fake argument");
            }
            catch (Exception ex)
            {

                var evt = new EventId(eventId, name);
                logger?.LogError(evt, ex, "usefull , but fake, error");
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

        var rhText = route.MapGet("api/usefull/endpoints/text", (HttpContext httpContext, [FromServices] IEnumerable<EndpointDataSource> endpointSources) =>
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
    public static void MapUsefullAdresses(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
    {
        var rhText = route.MapGet("api/usefull/adresses", (HttpContext httpContext, [FromServices] IServer server) =>
        {
            var adresses = server.Features.Get<IServerAddressesFeature>();
            var ret= adresses?.Addresses?.ToArray()??[] ;
            return TypedResults.Ok(ret);
        });
        rhText.AddDefault(corsPolicy, authorization);

    }
}