namespace System.Management.Automation;
/// Represents the transfer of bytes from one <see cref="Stream" /> to another
/// asynchronously.
internal sealed class AsyncByteStreamTransfer : IDisposable
    private const int DefaultBufferSize = 1024;
    private readonly BytePipe _bytePipe;
    private readonly BytePipe _destinationPipe;
    private readonly Memory<byte> _buffer;
    private readonly CancellationTokenSource _cts = new();
    private Task? _readToBufferTask;
    public AsyncByteStreamTransfer(
        BytePipe bytePipe,
        BytePipe destinationPipe)
        _bytePipe = bytePipe;
        _destinationPipe = destinationPipe;
        _buffer = new byte[DefaultBufferSize];
    public Task EOF => _readToBufferTask ?? Task.CompletedTask;
    public void BeginReadChunks()
        _readToBufferTask = Task.Run(ReadBufferAsync);
    public void Dispose() => _cts.Cancel();
    private async Task ReadBufferAsync()
        Stream stream;
        Stream? destinationStream = null;
            stream = await _bytePipe.GetStream(_cts.Token);
            destinationStream = await _destinationPipe.GetStream(_cts.Token);
                int bytesRead;
                bytesRead = await stream.ReadAsync(_buffer, _cts.Token);
                if (bytesRead is 0)
                destinationStream.Write(_buffer.Span.Slice(0, bytesRead));
            destinationStream?.Close();
