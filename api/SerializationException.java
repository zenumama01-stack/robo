 * Signals that a serialization or deserialization exception of some sort has occurred.
public class SerializationException extends IOException {
     * Creates a {@code SerializationException} with the specified detail message.
     * @param message the detail message.
    public SerializationException(@Nullable String message) {
     * Creates a {@code SerializationException} with the specified cause and a detail message of
     * {@code (cause==null ? null : cause.toString())}.
     * @param cause the cause. A {@code null} value is permitted, and indicates that the cause is nonexistent or
     *            unknown.
    public SerializationException(@Nullable Throwable cause) {
     * Creates a {@code SerializationException} with the specified cause and detail message.
    public SerializationException(@Nullable String message, @Nullable Throwable cause) {
