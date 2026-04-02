 * The {@link ScanListener} interface for receiving scan operation events.
 * A class that is interested in errors and termination of an active scan has to implement this interface.
public interface ScanListener {
     * Invoked synchronously when the according scan has finished.
     * This signal is sent latest when the defined timeout for the scan has been reached.
    void onFinished();
     * Invoked synchronously when the according scan has caused an error or has been aborted.
     * @param exception the error which occurred (could be null)
    void onErrorOccurred(@Nullable Exception exception);
