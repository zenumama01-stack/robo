 * Thrown when a requested {@link AudioStream} is not supported by an {@link AudioSource} or {@link AudioSink}
 * implementation
public class UnsupportedAudioStreamException extends AudioException {
     * Unsupported {@link AudioStream}
    private @Nullable Class<? extends @Nullable AudioStream> unsupportedAudioStreamClass;
     * @param message The message
     * @param unsupportedAudioStreamClass The unsupported audio stream class
    public UnsupportedAudioStreamException(String message,
            @Nullable Class<? extends @Nullable AudioStream> unsupportedAudioStreamClass, @Nullable Throwable cause) {
        this.unsupportedAudioStreamClass = unsupportedAudioStreamClass;
            @Nullable Class<? extends @Nullable AudioStream> unsupportedAudioStreamClass) {
        this(message, unsupportedAudioStreamClass, null);
     * Gets the unsupported audio stream class.
     * @return The unsupported audio stream class
    public @Nullable Class<? extends @Nullable AudioStream> getUnsupportedAudioStreamClass() {
        return unsupportedAudioStreamClass;
