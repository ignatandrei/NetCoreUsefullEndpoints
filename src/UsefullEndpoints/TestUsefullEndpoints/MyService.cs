namespace TestUsefullEndpoints
{
    public class MyServiceTime : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return ;
                }
                await Task.Delay(10000);
                Console.WriteLine("hosted:" + DateTime.Now.ToString());
            }
            
        }
    }
}
