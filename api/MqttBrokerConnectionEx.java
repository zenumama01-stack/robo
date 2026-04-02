 * We need an extended MqttBrokerConnection to overwrite the protected {@link #connectionCallback} with
 * an instance that takes the mocked version of {@link MqttBrokerConnection} and overwrite the connection state.
 * We also mock the internal Mqtt3AsyncClient that in respect to the success flags
 * immediately succeed or fail with publish, subscribe, unsubscribe, connect, disconnect.
 * @author Jan N. Klug - adjusted to HiveMQ client
public class MqttBrokerConnectionEx extends MqttBrokerConnection {
    public MqttConnectionState connectionStateOverwrite = MqttConnectionState.DISCONNECTED;
    public boolean publishSuccess = true;
    public boolean subscribeSuccess = true;
    public boolean unsubscribeSuccess = true;
    public boolean disconnectSuccess = true;
    public boolean connectSuccess = true;
    public boolean connectTimeout = false;
    public MqttBrokerConnectionEx(String host, @Nullable Integer port, boolean secure, boolean hostnameValidated,
            String clientId) {
        super(host, port, secure, hostnameValidated, clientId);
    public Map<String, Subscription> getSubscribers() {
    void setConnectionCallback(MqttBrokerConnectionEx o) {
        connectionCallback = spy(new ConnectionCallback(o));
        MqttAsyncClientWrapper mockedClient = mock(MqttAsyncClientWrapper.class);
        doAnswer(i -> {
            if (!connectTimeout) {
                connectionCallback.onConnected(null);
                connectionStateOverwrite = MqttConnectionState.CONNECTED;
            return new CompletableFuture<Boolean>();
        }).when(mockedClient).connect(any(), anyInt(), any(), any(), any());
            if (disconnectSuccess) {
                connectionCallback.onDisconnected(new Throwable("disconnect"));
                connectionStateOverwrite = MqttConnectionState.DISCONNECTED;
        }).when(mockedClient).disconnect();
        // subscribe
            if (subscribeSuccess) {
                CompletableFuture<Void> future = new CompletableFuture<>();
                future.completeExceptionally(new Throwable("subscription failed"));
        }).when(mockedClient).subscribe(any(), anyInt(), any());
        // unsubscribe
            if (unsubscribeSuccess) {
                future.completeExceptionally(new Throwable("unsubscription failed"));
        }).when(mockedClient).unsubscribe(any());
        // state
        doAnswer(i -> MqttClientState.CONNECTED).when(mockedClient).getState();
        return mockedClient;
        return connectionStateOverwrite;
