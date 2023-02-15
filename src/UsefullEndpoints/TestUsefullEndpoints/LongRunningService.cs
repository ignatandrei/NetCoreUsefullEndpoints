
internal class LongRunningService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(true)
        {
            using var lrts = UsefullExtensions.UsefullExtensions.AddLRTS("Ticks" + DateTime.UtcNow.Ticks);
            await Task.Delay(5000);
            Console.WriteLine("Long Running Service=>call longrunning task and/or shutdownafter 10 to see");
            if (stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("in long running cancel requested");
                //comment the following line to see stopped forcefully
                return;
            }
        }
    }
}