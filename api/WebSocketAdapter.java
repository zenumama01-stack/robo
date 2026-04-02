 * The {@link WebSocketAdapter} can be implemented to register an adapter for a websocket connection.
 * It will be accessible on the path /ws/ADAPTER_ID of your server.
 * Security is handled by the {@link CommonWebSocketServlet}.
public interface WebSocketAdapter {
     * The adapter id.
     * In combination with the base path {@link CommonWebSocketServlet#SERVLET_PATH} defines the adapter path.
     * @return the adapter id.
     * Creates a websocket instance.
     * It should use the {@code org.eclipse.jetty.websocket.api.annotations} or implement
     * {@link org.eclipse.jetty.websocket.api.WebSocketListener}.
     * @return a websocket instance.
    Object createWebSocket(ServletUpgradeRequest servletUpgradeRequest, ServletUpgradeResponse servletUpgradeResponse);
