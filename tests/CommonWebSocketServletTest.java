import static org.mockito.Mockito.any;
import static org.mockito.Mockito.times;
import org.eclipse.jetty.websocket.api.WebSocketPolicy;
import org.openhab.core.io.rest.auth.AnonymousUserSecurityContext;
import org.openhab.core.io.websocket.event.EventWebSocket;
 * The {@link CommonWebSocketServletTest} contains tests for the {@link EventWebSocket}
public class CommonWebSocketServletTest {
    private final String testAdapterId = "test-adapter-id";
    private @NonNullByDefault({}) CommonWebSocketServlet servlet;
    private @Mock @NonNullByDefault({}) AuthFilter authFilter;
    private @Mock @NonNullByDefault({}) WebSocketServletFactory factory;
    private @Mock @NonNullByDefault({}) WebSocketAdapter testDefaultWsAdapter;
    private @Mock @NonNullByDefault({}) WebSocketAdapter testWsAdapter;
    private @Mock @NonNullByDefault({}) WebSocketPolicy wsPolicy;
    private @Mock @NonNullByDefault({}) ServletUpgradeRequest request;
    private @Mock @NonNullByDefault({}) ServletUpgradeResponse response;
    private @Captor @NonNullByDefault({}) ArgumentCaptor<WebSocketCreator> webSocketCreatorAC;
    public void setup() throws ServletException, NamespaceException, AuthenticationException, IOException {
        servlet = new CommonWebSocketServlet(authFilter);
        when(factory.getPolicy()).thenReturn(wsPolicy);
        servlet.configure(factory);
        verify(factory).setCreator(webSocketCreatorAC.capture());
        when(request.getParameterMap()).thenReturn(Map.of());
        when(authFilter.getSecurityContext(any(), anyBoolean())).thenReturn(new AnonymousUserSecurityContext());
        when(testDefaultWsAdapter.getId()).thenReturn(CommonWebSocketServlet.DEFAULT_ADAPTER_ID);
        when(testWsAdapter.getId()).thenReturn(testAdapterId);
        servlet.addWebSocketAdapter(testDefaultWsAdapter);
        servlet.addWebSocketAdapter(testWsAdapter);
    public void createWebsocketUsingDefaultAdapterPath() throws URISyntaxException {
        when(request.getRequestURI()).thenReturn(new URI("http://127.0.0.1:8080/ws"));
        webSocketCreatorAC.getValue().createWebSocket(request, response);
        verify(testDefaultWsAdapter, times(1)).createWebSocket(request, response);
    public void createWebsocketUsingAdapterPath() throws URISyntaxException {
        when(request.getRequestURI()).thenReturn(new URI("http://127.0.0.1:8080/ws/" + testAdapterId));
        verify(testWsAdapter, times(1)).createWebSocket(request, response);
