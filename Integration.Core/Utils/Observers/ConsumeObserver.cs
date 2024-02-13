namespace Integration.Core.Utils.Observers;

public class ConsumeObserver : IConsumeObserver
{
    private readonly ILogger<ConsumeObserver> _logger;

    public ConsumeObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogInformation($"ConsumeFault(): {context}");
        await Task.CompletedTask;
    }

    public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogInformation($"PostConsume(): {context}");
        await Task.CompletedTask;
    }

    public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogInformation($"PreConsume(): {context}");
        await Task.CompletedTask;
    }
}
