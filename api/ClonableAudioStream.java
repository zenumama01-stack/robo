 * This is for an {@link AudioStream}, that can be cloned
 * @author Gwendal Roulleau - Initial contribution, separation from {@link FixedLengthAudioStream}
public interface ClonableAudioStream {
     * Returns a new, fully independent stream instance, which can be read and closed without impacting the original
     * instance.
     * @return a new input stream that can be consumed by the caller
     * @throws AudioException if stream cannot be created
    InputStream getClonedStream() throws AudioException;
