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
