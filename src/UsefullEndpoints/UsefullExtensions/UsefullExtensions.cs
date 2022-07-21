using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.Logging;

namespace UsefullExtensions
{
    public static class UsefullExtensions
    {
        public static void MapAllUsefull(this IEndpointRouteBuilder route)
        {

            route.MapUser();
            route.MapEnvironment();
            route.MapError();
            route.MapGraph();
        }

        public static void MapUser(this IEndpointRouteBuilder route)
        {
            ArgumentNullException.ThrowIfNull(route);
            route.MapGet("api/usefull/user/", (HttpContext httpContext) =>
            {
                return Results.Ok(httpContext.User);
            });
        }
        public static void MapEnvironment(this IEndpointRouteBuilder route)
        {
            ArgumentNullException.ThrowIfNull(route);
            //route.MapGet("api/usefull/environment/",  (HttpContext httpContext) =>
            //{
            //    return Results.Ok(Environment.UserDomainName);
            //});

            route.MapGet("api/usefull/environment/", (HttpContext httpContext) =>
            {
                return Results.Ok(new Helper().FromStaticEnvironment());
            });


        }
        public static void MapError(this IEndpointRouteBuilder route)
        {
            ArgumentNullException.ThrowIfNull(route);

            route.MapGet("api/usefull/errorWithILogger", (HttpContext httpContext, [FromServices] ILogger logger) =>
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

            route.MapGet("api/usefull/errorPure", (HttpContext httpContext) =>
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

        }
        public static void MapGraph(this IEndpointRouteBuilder route)
        {
            route.MapGet("api/usefull/graph/text", (HttpContext httpContext, [FromServices] DfaGraphWriter graphWriter, [FromServices] EndpointDataSource dataSource) =>
            {
                using (var sw = new StringWriter())
                {
                    // Write the graph
                    graphWriter.Write(dataSource, sw);
                    var graph = sw.ToString();

                    // Write the graph to the response
                    return Results.Ok(graph);
                }
            });
        }
    }
}
