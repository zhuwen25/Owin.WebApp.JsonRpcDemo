using Fleck;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace JsonRpcConsole;

public class WebSocketPipelineAdapter : Stream
{
    private readonly IWebSocketConnection _connection;
    private readonly Pipe _pipe = new();
    public WebSocketPipelineAdapter(IWebSocketConnection connection )
    {
        _connection = connection;
        _connection.OnMessage = OnMessageReceived;
    }
    private void OnMessageReceived(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);


        _pipe.Writer.Write(data);
        _pipe.Writer.FlushAsync();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadResult result = await _pipe.Reader.ReadAsync(cancellationToken);
        long bytesRead = Math.Min(result.Buffer.Length, count);
        result.Buffer.Slice(0, bytesRead).CopyTo(buffer);
        _pipe.Reader.AdvanceTo(result.Buffer.GetPosition(bytesRead));
        return (int) bytesRead;
    }
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        string message = Encoding.UTF8.GetString(buffer, offset, count);
        _connection.Send(message);
        await Task.CompletedTask;
    }

    public override void Flush() => _pipe.Writer.FlushAsync().ConfigureAwait(false).GetAwaiter().GetResult();


    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => throw new NotImplementedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    public override bool CanRead => true;
    public override bool CanSeek  => false;
    public override bool CanWrite  => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
}
