using System;
using System.Text;
using System.Text.Json;

namespace NativeMessagingHost
{
  public class GlobalAction
  {
    private static IMessagePublisher s_messagePublisher;
    private static readonly StringBuilder s_log = new();
    public static DateTime Started { get; private set; }

    public static void Start (IMessagePublisher messagePublisher)
    {
      s_messagePublisher = messagePublisher;

      Started = DateTime.Now;
      s_log.AppendLine ($"Started @{Started.ToShortTimeString()}");
    }

    public static void Tick ()
    {
      s_log.AppendLine ($"Tick @{DateTime.Now.ToShortTimeString()}; alive: {DateTime.Now - Started:g}");
      s_messagePublisher.PublishMessage(JsonDocument.Parse("34"));
    }

    public static string GetLog ()
    {
      return s_log.ToString();
    }
  }
}
