 * A {@link DTEvent} fired when the {@link DTService} encounters an error.
public class DTErrorEvent implements DTEvent {
     * The message describing the error
     * Constructs an instance with the passed {@code message}.
     * @param message The message describing the error
    public DTErrorEvent(String message) {
     * Gets the message describing this error
     * @return The message describing this error
        return this.message;
