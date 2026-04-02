 * Thrown if an error occurs communicating with the server. The exception contains a reason code. The semantic of the
 * reason code depends on the underlying implementation.
public class MqttException extends Exception {
    private static final long serialVersionUID = 301L;
    private Throwable cause;
     * Constructs a new <code>MqttException</code> with the specified reason
     * @param reason the reason for the exception.
    public MqttException(String reason) {
        this.cause = new Exception("reason");
     * Constructs a new <code>MqttException</code> with the specified
     * <code>Throwable</code> as the underlying reason.
     * @param cause the underlying cause of the exception.
    public MqttException(Throwable cause) {
     * Returns the underlying cause of this exception, if available.
     * @return the Throwable that was the root cause of this exception,
     *         which may be <code>null</code>.
    public @Nullable Throwable getCause() {
     * Returns the detail message for this exception. May be null.
        return cause.getMessage();
     * Returns a <code>String</code> representation of this exception.
        return cause.toString();
