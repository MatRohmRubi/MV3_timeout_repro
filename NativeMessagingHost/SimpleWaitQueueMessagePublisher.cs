using System.Text.Json;

namespace NativeMessagingHost
{
  public class SimpleWaitQueueMessagePublisher : IMessagePublisher
  {
    private readonly SimpleWaitQueue<JsonDocument> _queue;

    public SimpleWaitQueueMessagePublisher (SimpleWaitQueue<JsonDocument> queue)
    {
      _queue = queue;
    }

    /// <inheritdoc />
    public void CompleteAdding()
    {
      _queue.CompleteAdding();
    }

    /// <inheritdoc />
    public void PublishMessage (JsonDocument message)
    {
      _queue.Enqueue (message);
    }
  }
}
