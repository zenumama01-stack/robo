 * The listener interface for receiving {@link DTEvent} events.
 * A class interested in processing {@link DTEvent} events implements this interface.
public interface DTListener {
     * Invoked when a {@link DTEvent} event occurs.
     * @param dtEvent The {@link DTEvent} fired by the {@link DTService}
    void dtEventReceived(DTEvent dtEvent);
