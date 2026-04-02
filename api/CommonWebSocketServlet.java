package org.openhab.core.io.websocket;
import org.eclipse.jetty.websocket.server.WebSocketServerFactory;
import org.eclipse.jetty.websocket.servlet.WebSocketCreator;
import org.eclipse.jetty.websocket.servlet.WebSocketServlet;
import org.eclipse.jetty.websocket.servlet.WebSocketServletFactory;
import org.openhab.core.io.rest.auth.AuthFilter;
import org.openhab.core.io.websocket.event.EventWebSocketAdapter;
import org.osgi.service.http.NamespaceException;
 * The {@link CommonWebSocketServlet} provides the servlet for WebSocket connections.
 * Clients can authorize in two ways:
 * <li>By setting <code>org.openhab.ws.accessToken.base64.</code> + base64-encoded access token and the
 * {@link CommonWebSocketServlet#WEBSOCKET_PROTOCOL_DEFAULT} in the <code>Sec-WebSocket-Protocol</code> header.</li>
 * <li>By providing the access token as query parameter <code>accessToken</code>.</li>
 * @author Miguel Álvarez Díez - Refactor into a common servlet
 * @author Florian Hotze - Support passing access token through Sec-WebSocket-Protocol header
@HttpWhiteboardServletName(CommonWebSocketServlet.SERVLET_PATH)
@HttpWhiteboardServletPattern(CommonWebSocketServlet.SERVLET_PATH + "/*")
@Component(immediate = true, service = { Servlet.class })
public class CommonWebSocketServlet extends WebSocketServlet {
    public static final String SEC_WEBSOCKET_PROTOCOL_HEADER = "Sec-WebSocket-Protocol";
    public static final String WEBSOCKET_PROTOCOL_DEFAULT = "org.openhab.ws.protocol.default";
    private static final Pattern WEBSOCKET_ACCESS_TOKEN_PATTERN = Pattern
            .compile("org.openhab.ws.accessToken.base64.(?<base64>[A-Za-z0-9+/]*)");
    public static final String SERVLET_PATH = "/ws";
    public static final String DEFAULT_ADAPTER_ID = EventWebSocketAdapter.ADAPTER_ID;
    private final Map<String, WebSocketAdapter> connectionHandlers = new ConcurrentHashMap<>();
    private final AuthFilter authFilter;
    private @Nullable WebSocketServerFactory importNeeded;
    public CommonWebSocketServlet(@Reference AuthFilter authFilter) throws ServletException, NamespaceException {
        this.authFilter = authFilter;
    public void configure(@NonNullByDefault({}) WebSocketServletFactory webSocketServletFactory) {
        webSocketServletFactory.getPolicy().setIdleTimeout(10000);
        webSocketServletFactory.setCreator(new CommonWebSocketCreator());
    protected void addWebSocketAdapter(WebSocketAdapter wsAdapter) {
        this.connectionHandlers.put(wsAdapter.getId(), wsAdapter);
    protected void removeWebSocketAdapter(WebSocketAdapter wsAdapter) {
        this.connectionHandlers.remove(wsAdapter.getId());
    private class CommonWebSocketCreator implements WebSocketCreator {
        private final Logger logger = LoggerFactory.getLogger(CommonWebSocketCreator.class);
        public @Nullable Object createWebSocket(@Nullable ServletUpgradeRequest servletUpgradeRequest,
                @Nullable ServletUpgradeResponse servletUpgradeResponse) {
            if (servletUpgradeRequest == null || servletUpgradeResponse == null) {
            String accessToken = null;
            String secWebSocketProtocolHeader = servletUpgradeRequest.getHeader(SEC_WEBSOCKET_PROTOCOL_HEADER);
            if (secWebSocketProtocolHeader != null) { // if the client sends the Sec-WebSocket-Protocol header
                // respond with the default protocol
                servletUpgradeResponse.setHeader(SEC_WEBSOCKET_PROTOCOL_HEADER, WEBSOCKET_PROTOCOL_DEFAULT);
                // extract the base64 encoded access token from the requested protocols
                Matcher matcher = WEBSOCKET_ACCESS_TOKEN_PATTERN.matcher(secWebSocketProtocolHeader);
                if (matcher.find() && matcher.group("base64") != null) {
                    String base64 = matcher.group("base64");
                        accessToken = new String(Base64.getDecoder().decode(base64));
                        logger.warn("Invalid base64 encoded access token in Sec-WebSocket-Protocol header from {}.",
                                servletUpgradeRequest.getRemoteAddress());
                    logger.warn("Invalid use of Sec-WebSocket-Protocol header from {}.",
            if (accessToken != null ? isAuthorizedRequest(accessToken) : isAuthorizedRequest(servletUpgradeRequest)) {
                String requestPath = servletUpgradeRequest.getRequestURI().getPath();
                String pathPrefix = SERVLET_PATH + "/";
                boolean useDefaultAdapter = requestPath.equals(pathPrefix) || !requestPath.startsWith(pathPrefix);
                WebSocketAdapter wsAdapter;
                if (!useDefaultAdapter) {
                    String adapterId = requestPath.substring(pathPrefix.length());
                    wsAdapter = connectionHandlers.get(adapterId);
                    if (wsAdapter == null) {
                        logger.warn("Missing WebSocket adapter for path {}", adapterId);
                    wsAdapter = connectionHandlers.get(DEFAULT_ADAPTER_ID);
                        logger.warn("Default WebSocket adapter is missing");
                logger.debug("New connection handled by {}", wsAdapter.getId());
                return wsAdapter.createWebSocket(servletUpgradeRequest, servletUpgradeResponse);
                logger.warn("Unauthenticated request to create a websocket from {}.",
        private boolean isAuthorizedRequest(String bearerToken) {
                var securityContext = authFilter.getSecurityContext(bearerToken);
                return securityContext != null
                        && (securityContext.isUserInRole(Role.USER) || securityContext.isUserInRole(Role.ADMIN));
                logger.warn("Error handling WebSocket authorization", e);
        private boolean isAuthorizedRequest(ServletUpgradeRequest servletUpgradeRequest) {
                var securityContext = authFilter.getSecurityContext(servletUpgradeRequest.getHttpServletRequest(),
            } catch (AuthenticationException | IOException e) {
