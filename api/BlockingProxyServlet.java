import org.eclipse.jetty.client.api.Response;
import org.eclipse.jetty.client.util.InputStreamResponseListener;
import org.eclipse.jetty.http.HttpField;
 * A blocking version of the proxy servlet that complies with Servlet API 2.4.
 * @author Svilen Valkanov - Replaced Apache HttpClient with Jetty
 * @author John Cocula - refactored to support alternate implementation
public class BlockingProxyServlet extends HttpServlet {
    private final Logger logger = LoggerFactory.getLogger(BlockingProxyServlet.class);
    private static final long serialVersionUID = -4716754591953017794L;
    private static HttpClient httpClient = new HttpClient(new SslContextFactory.Client());
    /** Timeout for HTTP requests in ms */
    private static final int TIMEOUT = 15000;
    BlockingProxyServlet(ProxyServletService service) {
                logger.warn("Cannot start HttpClient!", e);
        return "Proxy (blocking)";
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        URI uri = service.uriFromRequest(request);
            Request httpRequest = httpClient.newRequest(uri);
            service.maybeAppendAuthHeader(uri, httpRequest);
            InputStreamResponseListener listener = new InputStreamResponseListener();
            // do the client request
                httpRequest.send(listener);
                // wait for the response headers to arrive or the timeout to expire
                Response httpResponse = listener.get(TIMEOUT, TimeUnit.MILLISECONDS);
                // copy all response headers
                for (HttpField header : httpResponse.getHeaders()) {
                    response.setHeader(header.getName(), header.getValue());
            } catch (TimeoutException e) {
                logger.warn("Proxy servlet failed to stream content due to a timeout");
                response.sendError(HttpServletResponse.SC_GATEWAY_TIMEOUT);
                logger.warn("Proxy servlet failed to stream content: {}", e.getMessage());
                response.sendError(HttpServletResponse.SC_BAD_REQUEST, e.getMessage());
            // now copy/stream the body content
            try (InputStream responseContent = listener.getInputStream()) {
                responseContent.transferTo(response.getOutputStream());
