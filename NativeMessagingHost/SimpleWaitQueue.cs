using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NativeMessagingHost
{
  public class SimpleWaitQueue<T>
  {
    private readonly object _lock = new();
    private readonly Queue<T> _queue = new();
    private readonly AsyncAutoResetEvent _hasNewElementEvent = new (false);


    private bool _completed = false;

    public void CompleteAdding()
    {
      lock (_lock)
      {
        _completed = true;
        _hasNewElementEvent.Set();
      }
    }

    public void Enqueue (T element)
    {
      lock (_lock)
      {
        if (_completed)
          return;

        _queue.Enqueue (element);
        _hasNewElementEvent.Set();
      }
    }

    public async ValueTask<T> DequeueAsync()
    {
      while (true)
      {
        lock (_lock)
        {
          if (_queue.TryDequeue (out var result))
            return result;
          if (_completed)
            throw new OperationCanceledException();
        }

        await _hasNewElementEvent.WaitAsync();
      }
    }
  }
}
