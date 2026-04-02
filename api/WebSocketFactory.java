import org.eclipse.jetty.websocket.client.WebSocketClient;
 * Factory class to create Jetty web socket clients
public interface WebSocketFactory {
     * Creates a new Jetty web socket client.
     * @throws NullPointerException if {@code consumerName} is {@code null}
    WebSocketClient createWebSocketClient(String consumerName);
    WebSocketClient createWebSocketClient(String consumerName, @Nullable SslContextFactory sslContextFactory);
     * Returns a shared Jetty web socket client. You must not call any setter methods or {@code stop()} on it.
     * @return a shared Jetty web socket client
    WebSocketClient getCommonWebSocketClient();
