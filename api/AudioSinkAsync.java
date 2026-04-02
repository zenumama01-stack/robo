import org.openhab.core.common.Disposable;
 * Helper class for asynchronous sink : when the process() method returns, the {@link AudioStream}
 * may or may not be played. It is the responsibility of the implementing AudioSink class to
 * complete the CompletableFuture when playing is done. Any delayed tasks will then be performed, such as volume
 * restoration.
 * @author Gwendal Roulleau - Initial contribution
public abstract class AudioSinkAsync implements AudioSink {
    private final Logger logger = LoggerFactory.getLogger(AudioSinkAsync.class);
    protected final Map<AudioStream, CompletableFuture<@Nullable Void>> runnableByAudioStream = new HashMap<>();
    public CompletableFuture<@Nullable Void> processAndComplete(@Nullable AudioStream audioStream) {
        CompletableFuture<@Nullable Void> completableFuture = new CompletableFuture<@Nullable Void>();
        if (audioStream != null) {
            runnableByAudioStream.put(audioStream, completableFuture);
            processAsynchronously(audioStream);
            completableFuture.completeExceptionally(e);
        if (audioStream == null) {
            // No need to delay the post process task
            completableFuture.complete(null);
        return completableFuture;
    public void process(@Nullable AudioStream audioStream)
            throws UnsupportedAudioFormatException, UnsupportedAudioStreamException {
     * Processes the passed {@link AudioStream} asynchronously. This method is expected to return before the stream is
     * fully played. This is the sink responsibility to call the {@link AudioSinkAsync#playbackFinished(AudioStream)}
     * when it is.
    protected abstract void processAsynchronously(@Nullable AudioStream audioStream)
     * Will complete the future previously returned, allowing the core to run delayed task.
     * @param audioStream The AudioStream is the key to find the delayed CompletableFuture in the storage.
    protected void playbackFinished(AudioStream audioStream) {
        CompletableFuture<@Nullable Void> completableFuture = runnableByAudioStream.remove(audioStream);
        if (completableFuture != null) {
        // if the stream is not needed anymore, then we should call back the AudioStream to let it a chance
        // to auto dispose.
        if (audioStream instanceof Disposable disposableAudioStream) {
                disposableAudioStream.dispose();
                String fileName = audioStream instanceof FileAudioStream file ? file.toString() : "unknown";
                if (logger.isDebugEnabled()) {
                    logger.debug("Cannot dispose of stream {}", fileName, e);
                    logger.warn("Cannot dispose of stream {}, reason {}", fileName, e.getMessage());
