/// Represents a lazily retrieved <see cref="Stream" /> for transfering bytes
/// to or from.
internal abstract class BytePipe
    public abstract Task<Stream> GetStream(CancellationToken cancellationToken);
    internal AsyncByteStreamTransfer Bind(BytePipe bytePipe)
        Debug.Assert(bytePipe is not null);
        return new AsyncByteStreamTransfer(bytePipe, destinationPipe: this);
/// Represents a <see cref="Stream" /> lazily retrieved from the underlying
/// <see cref="NativeCommandProcessor" />.
internal sealed class NativeCommandProcessorBytePipe : BytePipe
    private readonly NativeCommandProcessor _nativeCommand;
    private readonly bool _stdout;
    internal NativeCommandProcessorBytePipe(
        NativeCommandProcessor nativeCommand,
        bool stdout)
        Debug.Assert(nativeCommand is not null);
        _nativeCommand = nativeCommand;
        _stdout = stdout;
    public override async Task<Stream> GetStream(CancellationToken cancellationToken)
        // If the native command we're wrapping is the upstream command then
        // NativeCommandProcessor.Prepare will have already been called before
        // the creation of this BytePipe.
        if (_stdout)
            return _nativeCommand.GetStream(stdout: true);
        await _nativeCommand.WaitForProcessInitializationAsync(cancellationToken);
        return _nativeCommand.GetStream(stdout: false);
/// Provides an byte pipe implementation representing a <see cref="FileStream" />.
internal sealed class FileBytePipe : BytePipe
    private readonly Stream _stream;
    private FileBytePipe(Stream stream)
        Debug.Assert(stream is not null);
        _stream = stream;
    internal static FileBytePipe Create(string fileName, bool append)
                fileName,
                resolvedEncoding: null,
                append,
                Force: true,
                NoClobber: false,
                out fileStream,
                streamWriter: out _,
                readOnlyFileInfo: out _,
                isLiteralPath: true);
        catch (Exception e) when (e.Data.Contains(typeof(ErrorRecord)))
            // The error record is attached to the exception when thrown to preserve
            // the call stack.
            ErrorRecord? errorRecord = e.Data[typeof(ErrorRecord)] as ErrorRecord;
            if (errorRecord is null)
            e.Data.Remove(typeof(ErrorRecord));
            throw new RuntimeException(null, e, errorRecord);
        ApplicationInsightsTelemetry.SendExperimentalUseData("PSNativeCommandPreserveBytePipe", "f");
        return new FileBytePipe(fileStream);
    public override Task<Stream> GetStream(CancellationToken cancellationToken) => Task.FromResult(_stream);
