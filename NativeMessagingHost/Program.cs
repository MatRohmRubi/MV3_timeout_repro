using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NativeMessagingHost
{
  public static class Program
  {
    public delegate ValueTask<JsonDocument> DequeueMessageDelegate();

    public static async Task Main (string[] args)
    {
      var input = Console.OpenStandardInput();
      var output = Console.OpenStandardOutput();

      var messageQueue = new SimpleWaitQueue<JsonDocument>();
      var messagePublisher = new SimpleWaitQueueMessagePublisher (messageQueue);
      var cancellationTokenSource = new CancellationTokenSource();

      var pipe = new Pipe();
      var writing = FillIncomingPipeAsync (input, pipe.Writer, cancellationTokenSource.Token);
      var reading = ReadIncomingMessagesFromPipeAsync (pipe.Reader, messagePublisher, cancellationTokenSource.Token);
      var outgoing = WriteOutgoingMessageAsync (output, messageQueue.DequeueAsync, cancellationTokenSource.Token);

      GlobalAction.Start (messagePublisher);

      var server = CreateHostBuilder (args, messagePublisher).Build().RunAsync (cancellationTokenSource.Token);

      var timer = new Timer(_ => GlobalAction.Tick(), null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));

      await Task.WhenAll (writing, reading);

      await timer.DisposeAsync();

      cancellationTokenSource.Cancel();

      await Task.WhenAll (outgoing, server);
    }

    public static IHostBuilder CreateHostBuilder (string[] args, IMessagePublisher messagePublisher)
    {
      return Host.CreateDefaultBuilder (args)
          .ConfigureWebHostDefaults (
              webBuilder =>
              {
                webBuilder.UseStartup<Startup>()
                    .ConfigureServices (e => e.AddSingleton (messagePublisher))
                    .ConfigureLogging (e => e.ClearProviders())
                    .UseKestrel();
              });
    }


    private static async Task FillIncomingPipeAsync (Stream stream, PipeWriter pipeWriter, CancellationToken cancellationToken)
    {
      const int minimumBufferSize = 512;

      try
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          var buffer = pipeWriter.GetMemory (minimumBufferSize);

          var bytesRead = await stream.ReadAsync (buffer, cancellationToken);
          if (bytesRead == 0)
            break;

          pipeWriter.Advance (bytesRead);

          var result = await pipeWriter.FlushAsync (cancellationToken);
          if (result.IsCompleted)
            break;
        }
      }
      catch (OperationCanceledException)
      {
      }

      await pipeWriter.CompleteAsync();
      await stream.DisposeAsync();
    }

    private static async Task ReadIncomingMessagesFromPipeAsync (PipeReader pipeReader, IMessagePublisher messagePublisher, CancellationToken cancellationToken)
    {
      static int GetMessageSize (ReadOnlySequence<byte> sequence)
      {
        Span<byte> header = stackalloc byte[4];
        sequence.CopyTo (header);
        return BinaryPrimitives.ReadInt32LittleEndian (header);
      }

      while (true)
      {
        var readResult = await pipeReader.ReadAsync (cancellationToken);

        var buffer = readResult.Buffer;
        while (buffer.Length >= 4)
        {
          var nextPacketSize = GetMessageSize (buffer.Slice (0, 4));
          if (buffer.Length < nextPacketSize + 4)
            break;

          var messageData = buffer.Slice (4, nextPacketSize);
          var message = JsonDocument.Parse (messageData);
          messagePublisher.PublishMessage (message);

          buffer = buffer.Slice (4 + nextPacketSize);
        }

        pipeReader.AdvanceTo (buffer.Start, buffer.End);

        if (readResult.IsCompleted)
          break;
      }

      messagePublisher.CompleteAdding();
    }

    private static async Task WriteOutgoingMessageAsync (Stream stream, DequeueMessageDelegate dequeueMessage, CancellationToken cancellationToken)
    {
      var headerBuffer = new byte[4];

      var arrayBufferWriter = new ArrayBufferWriter<byte>();
      var utf8JsonWriter = new Utf8JsonWriter (arrayBufferWriter);

      try
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          var message = await dequeueMessage();

          arrayBufferWriter.Clear();
          utf8JsonWriter.Reset();

          message.WriteTo (utf8JsonWriter);
          await utf8JsonWriter.FlushAsync (cancellationToken);

          BinaryPrimitives.WriteInt32LittleEndian (headerBuffer, arrayBufferWriter.WrittenCount);
        }
      }
      catch (OperationCanceledException)
      {
      }
    }
  }
}
