 * General purpose audio exception
 * @author Harald Kuhn - Initial contribution
 * @author Kelly Davis - Modified to match discussion in #584
public class AudioException extends Exception {
     * Constructs a new exception with null as its detail message.
    public AudioException() {
     * Constructs a new exception with the specified detail message and cause.
     * @param message Detail message
     * @param cause The cause
    public AudioException(String message, @Nullable Throwable cause) {
     * Constructs a new exception with the specified detail message.
    public AudioException(String message) {
     * Constructs a new exception with the specified cause.
    public AudioException(Throwable cause) {
        super(cause);
