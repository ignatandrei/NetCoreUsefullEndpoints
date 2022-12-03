using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Text;

namespace UsefullExtensions
{
    public static class UsefullExtensions
    {
        public static void MapUsefullAll(this IEndpointRouteBuilder route,string? cors= null, string[]? authorization=null)
        {
            route.MapUsefullUser(cors, authorization);
            route.MapUsefullEnvironment(cors, authorization);
            route.MapUsefullError(cors, authorization);
            route.MapUsefullDate(cors, authorization);
            route.MapUsefullEndpoints(cors, authorization);
            route.MapUsefullConfiguration(cors, authorization);
            route.MapUsefullContext(cors, authorization);
        }
        private static void AddDefault(this RouteHandlerBuilder rh, string? corsPolicy = null, string[]? authorization = null)
        {
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
        public static void MapUsefullUser(this IEndpointRouteBuilder route, string? corsPolicy = null,string[]? authorization =null)
        {
            ArgumentNullException.ThrowIfNull(route);
            var rh = route.MapGet("api/usefull/user/authorization", (HttpContext httpContext) =>
            {
                return Results.Ok(httpContext.User);
            });
            
            if (corsPolicy?.Length > 0)
                rh=rh.RequireCors(corsPolicy);
            
            if (authorization?.Length > 0 && authorization[0]?.Length>0)
                rh = rh.RequireAuthorization(authorization);

            
            rh = route.MapGet("api/usefull/user/noAuthorization", (HttpContext httpContext) =>
            {
                return Results.Ok(httpContext.User);
            }).AllowAnonymous();

            if (corsPolicy?.Length > 0)
                rh = rh.RequireCors(corsPolicy);

        }
        public static void MapUsefullContext(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
        {
            ArgumentNullException.ThrowIfNull(route);
            route.MapGet("api/usefull/httpContext/Connection", (HttpContext httpContext) =>
            {
                var con = httpContext.Connection;
                if(con == null)
                {
                    return Results.NoContent();
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
                return Results.Ok(conSerialize);
            }).AddDefault(corsPolicy, authorization);
        }
        public static void MapUsefullConfiguration(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
        {
            ArgumentNullException.ThrowIfNull(route);
            var rh=route.MapGet("api/usefull/configuration/", ([FromServices]IConfiguration config) =>
            {
                var c = config as IConfigurationRoot;
                if (c != null)
                {
                    return Results.Content(c.GetDebugView());
                }
                else
                {                    
                    return Results.NoContent();
                }
            });
            rh.AddDefault(corsPolicy, authorization);
        }
        public static void MapUsefullDate(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
        {
            ArgumentNullException.ThrowIfNull(route);
            var rh = route.MapGet("api/usefull/date/", (HttpContext httpContext) =>
            {
                return Results.Ok(DateTime.Now);
            });
            rh.AddDefault(corsPolicy, authorization);
        }
        public static void MapUsefullEnvironment(this IEndpointRouteBuilder route, string? corsPolicy = null, string[]? authorization = null)
        {
            ArgumentNullException.ThrowIfNull(route);

            var rh=route.MapGet("api/usefull/environment/", (HttpContext httpContext) =>
            {
                return Results.Ok(new Helper().FromStaticEnvironment());
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
            rh=route.MapGet("api/usefull/errorPure", (HttpContext httpContext) =>
            {
                //return Results.Ok("tesr");
                try
                {
                    var x = 0;
                    x++;
                    x = (x - 1) / (x - 1);
                    return Results.Ok("fake x");
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
            var rh=route.MapGet("api/usefull/endpoints/graph", (HttpContext httpContext, [FromServices] DfaGraphWriter graphWriter, [FromServices] EndpointDataSource dataSource) =>
            {
                using (var sw = new StringWriter())
                {
                    // Write the graph
                    graphWriter.Write(dataSource, sw);
                    var graph = sw.ToString();

                    // Write the graph to the response
                    return Results.Content(graph);
                }
            });
            rh.AddDefault(corsPolicy, authorization);

            route.MapGet("api/usefull/endpoints/text", (HttpContext httpContext, [FromServices] IEnumerable<EndpointDataSource> endpointSources) =>
            {

            
            var endpoints = endpointSources.SelectMany(es => es.Endpoints);
            var res = endpoints.Select(endpoint =>
            new {
                name = endpoint.DisplayName,
                routeName = endpoint.Metadata.OfType<RouteNameMetadata>().FirstOrDefault(),
                httpMethod= endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()

            }).ToArray();
                return Results.Ok(res);
            });
            rh.AddDefault(corsPolicy, authorization);

        }
    }
}
