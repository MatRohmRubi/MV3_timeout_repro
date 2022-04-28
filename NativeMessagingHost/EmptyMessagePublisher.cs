using System.Text.Json;

namespace NativeMessagingHost
{
  public class EmptyMessagePublisher : IMessagePublisher
  {
    private readonly SimpleWaitQueue<JsonDocument> _queue;

    public EmptyMessagePublisher (SimpleWaitQueue<JsonDocument> queue)
    {
      _queue = queue;
    }

    public void CompleteAdding()
    {
      _queue.CompleteAdding();
    }

    public void PublishMessage (JsonDocument message)
    {
    }
  }
}
