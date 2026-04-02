 * The listener interface for receiving {@link STTEvent} events.
 * A class interested in processing {@link STTEvent} events implements this interface,
 * and its instances are passed to the {@code STTService}'s {@code recognize()} method.
 * Such instances are then targeted for various {@link STTEvent} events corresponding
 * to the speech recognition process.
public interface STTListener {
     * Invoked when a {@link STTEvent} event occurs during speech recognition.
     * @param sttEvent The {@link STTEvent} fired by the {@link STTService}
    void sttEventReceived(STTEvent sttEvent);
