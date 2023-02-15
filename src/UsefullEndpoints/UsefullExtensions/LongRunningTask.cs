namespace UsefullExtensions;

public record LongRunningTask(string id, string? name = "") : IDisposable
{
    public void Dispose()
    {
        UsefullExtensions.lrts.Remove(id);
        GC.SuppressFinalize(this);
    }

}
