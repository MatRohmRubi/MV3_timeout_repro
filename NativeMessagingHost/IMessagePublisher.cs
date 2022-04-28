using System.Text.Json;

namespace NativeMessagingHost
{
  public interface IMessagePublisher
  {
    void CompleteAdding();

    void PublishMessage (JsonDocument message);
  }
}
