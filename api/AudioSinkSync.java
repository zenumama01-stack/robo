 * Helper class for synchronous sink : when the process() method returns,
 * the source is considered played, and could be disposed.
 * Any delayed tasks can then be performed, such as volume restoration.
public abstract class AudioSinkSync implements AudioSink {
    private final Logger logger = LoggerFactory.getLogger(AudioSinkSync.class);
            processSynchronously(audioStream);
            // as the stream is not needed anymore, we should dispose of it
     * Processes the passed {@link AudioStream} and returns only when the playback is ended.
    protected abstract void processSynchronously(@Nullable AudioStream audioStream)
