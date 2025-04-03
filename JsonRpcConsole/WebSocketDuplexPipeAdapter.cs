using Fleck;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace JsonRpcConsole;

public class WebSocketDuplexPipeAdapter : IDuplexPipe, IDisposable
{
    private readonly Pipe _inputPipe =
        new(new PipeOptions(useSynchronizationContext: false, readerScheduler: PipeScheduler.ThreadPool));

    private readonly Pipe _outputPipe =
        new(new PipeOptions(useSynchronizationContext: false, readerScheduler: PipeScheduler.ThreadPool));

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    IWebSocketConnection _connection;

    public PipeReader Input
    {
        get => _inputPipe.Reader;
        private set => throw new NotSupportedException();
    }

    public PipeWriter Output
    {
        get => _outputPipe.Writer;
        private set => throw new NotSupportedException();
    }

    public WebSocketDuplexPipeAdapter(IWebSocketConnection socketConnection)
    {
        _connection = socketConnection;
        _connection.OnMessage = OnMessageReceived;
    }

    private void OnMessageReceived(string message)
    {
        if (!_connection.IsAvailable)
        {
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        _outputPipe.Writer.Write(data);
        _outputPipe.Writer.FlushAsync().AsTask().ConfigureAwait(false);
    }

    public async Task StartWriteAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            var result = await _outputPipe.Reader.ReadAsync(token);
            if (result.IsCanceled) break;

            foreach (var segment in result.Buffer)
            {
                await _connection.Send(segment.ToArray());
            }

            _outputPipe.Reader.AdvanceTo(result.Buffer.End);
        }
    }

    public async Task ProcessAsync()
    {
        var inputTask = ProcessInputAsync();
        var outputTask = ProcessOutputAsync();

        // Wait until either task completes (graceful shutdown)
        await Task.WhenAny(inputTask, outputTask);
    }

    private async Task ProcessInputAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            ReadResult readResult = await _inputPipe.Reader.ReadAsync(_cancellationTokenSource.Token);
            if (readResult.IsCanceled) break;
            _inputPipe.Reader.AdvanceTo(readResult.Buffer.End);
        }
    }

    private async Task ProcessOutputAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            ReadResult readResult = await _outputPipe.Reader.ReadAsync(_cancellationTokenSource.Token);
            if (readResult.IsCanceled) break;
            foreach (var segment in readResult.Buffer)
            {
                string message = Encoding.UTF8.GetString(segment.ToArray());
                await _connection.Send(message);
            }

            _outputPipe.Reader.AdvanceTo(readResult.Buffer.End);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _inputPipe.Reader.Complete();
        _outputPipe.Reader.Complete();
    }
}
