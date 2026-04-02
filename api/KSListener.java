 * The listener interface for receiving {@link KSEvent} events.
 * A class interested in processing {@link KSEvent} events implements this interface,
 * and its instances are passed to the {@code KSService}'s {@code spot()} method.
 * Such instances are then targeted for various {@link KSEvent} events corresponding
 * to the keyword spotting process.
public interface KSListener extends DTListener {
     * Invoked when a {@link KSEvent} event occurs during keyword spotting.
     * @param ksEvent The {@link KSEvent} fired by the {@link KSService}
    void ksEventReceived(KSEvent ksEvent);
