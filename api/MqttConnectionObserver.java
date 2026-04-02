 * Implement this interface to get notified of connection state changes.
 * Register this observer at {@see MqttBrokerConnection}.
 * @author David Graeff - Rewritten
public interface MqttConnectionObserver {
     * Inform the observer if a connection could be established or if a connection
     * is lost. This will be issued in the context of the Mqtt client thread and
     * requires that the control is returned quickly to not stall the Mqtt thread.
     * @param state The new connection state
     * @param error An exception object (might be a MqttException) with the reason why
     *            a connection failed.
    void connectionStateChanged(MqttConnectionState state, @Nullable Throwable error);
