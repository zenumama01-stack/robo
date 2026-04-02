 * This is the interface for a dialog trigger service that only allows to register a {@link DTEvent} listener.
public interface BasicDTService extends DTService {
     * Used to register the dialog trigger events listener
     * @param dtListener Non-null {@link DTListener} that {@link DTEvent} events target
     * @throws DTException if the listener cannot be registered due to an internal error or invalid state
    DTServiceHandle registerListener(DTListener dtListener) throws DTException;
