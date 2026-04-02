 * Wrapper for a source of audio data.
 * In contrast to {@link AudioSource}, this is often a "one time use" instance for passing some audio data,
 * but it is not meant to be registered as a service.
 * The stream needs to be closed by the client that uses it.
 * @author Kai Kreuzer - Refactored to be only a temporary instance for the stream
public abstract class AudioStream extends InputStream {
     * Gets the supported audio format
     * @return The supported audio format
    public abstract AudioFormat getFormat();
     * Usefull for sinks playing the same stream multiple times,
     * to avoid already done computation (like reencoding).
     * @return A string uniquely identifying the stream.
    public @Nullable String getId() {
