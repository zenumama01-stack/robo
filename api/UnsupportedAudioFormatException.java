 * Thrown when a requested format is not supported by an {@link AudioSource}
 * or {@link AudioSink} implementation
public class UnsupportedAudioFormatException extends AudioException {
     * Unsupported {@link AudioFormat}
    private @Nullable AudioFormat unsupportedFormat;
     * Constructs a new exception with the specified detail message, unsupported format, and cause.
     * @param unsupportedFormat Unsupported format
    public UnsupportedAudioFormatException(String message, @Nullable AudioFormat unsupportedFormat,
            @Nullable Throwable cause) {
        this.unsupportedFormat = unsupportedFormat;
     * Constructs a new exception with the specified detail message and unsupported format.
    public UnsupportedAudioFormatException(String message, @Nullable AudioFormat unsupportedFormat) {
        this(message, unsupportedFormat, null);
     * Gets the unsupported format
     * @return The unsupported format
    public @Nullable AudioFormat getUnsupportedFormat() {
        return unsupportedFormat;
