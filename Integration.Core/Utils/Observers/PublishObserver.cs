namespace Integration.Core.Utils.Observers;

public class PublishObserver : IPublishObserver
{
    private readonly ILogger<PublishObserver> _logger;

    public PublishObserver(ILogger<PublishObserver> logger)
    {
        _logger = logger;
    }

    public async Task PostPublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogInformation($"PostPublish(): {context}");
        await Task.CompletedTask;
    }

    public async Task PrePublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogInformation($"PrePublish(): {context}");
        await Task.CompletedTask;
    }

    public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
    {
        _logger.LogInformation($"PublishFault(): {context}");
        await Task.CompletedTask;
    }
}
