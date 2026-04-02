import org.eclipse.jetty.client.HttpDestination;
import org.eclipse.jetty.client.HttpExchange;
import org.eclipse.jetty.client.api.Connection;
import org.eclipse.jetty.client.http.HttpChannelOverHTTP;
import org.eclipse.jetty.client.http.HttpClientTransportOverHTTP;
import org.eclipse.jetty.client.http.HttpConnectionOverHTTP;
import org.eclipse.jetty.client.http.HttpReceiverOverHTTP;
import org.eclipse.jetty.http.HttpVersion;
import org.eclipse.jetty.http.PreEncodedHttpField;
import org.eclipse.jetty.io.EndPoint;
import org.eclipse.jetty.util.Promise;
import org.eclipse.jetty.util.thread.QueuedThreadPool;
import org.openhab.core.io.net.http.HttpClientInitializationException;
import org.openhab.core.io.net.http.WebSocketFactory;
 * Factory class to create Jetty web clients
 * @author Kai Kreuzer - added web socket support
 * @author Martin van Wingerden - Add support for ESHTrustManager
@Component(immediate = true, configurationPid = "org.openhab.webclient")
public class WebClientFactoryImpl implements HttpClientFactory, WebSocketFactory {
    private final Logger logger = LoggerFactory.getLogger(WebClientFactoryImpl.class);
    private static final String CONFIG_MIN_THREADS_SHARED = "minThreadsShared";
    private static final String CONFIG_MAX_THREADS_SHARED = "maxThreadsShared";
    private static final String CONFIG_KEEP_ALIVE_SHARED = "keepAliveTimeoutShared";
    private static final String CONFIG_MIN_THREADS_CUSTOM = "minThreadsCustom";
    private static final String CONFIG_MAX_THREADS_CUSTOM = "maxThreadsCustom";
    private static final String CONFIG_KEEP_ALIVE_CUSTOM = "keepAliveTimeoutCustom";
    private static final int MIN_CONSUMER_NAME_LENGTH = 4;
    private static final int MAX_CONSUMER_NAME_LENGTH = 20;
    private static final Pattern CONSUMER_NAME_PATTERN = Pattern.compile("[a-zA-Z0-9_\\-]*");
    private final ExtensibleTrustManager extensibleTrustManager;
    private @NonNullByDefault({}) QueuedThreadPool threadPool;
    private @NonNullByDefault({}) HttpClient commonHttpClient;
    private @NonNullByDefault({}) WebSocketClient commonWebSocketClient;
    private int minThreadsShared;
    private int maxThreadsShared;
    private int keepAliveTimeoutShared; // in s
    private int minThreadsCustom;
    private int maxThreadsCustom;
    private int keepAliveTimeoutCustom; // in s
    private boolean hpackLoadTestDone = false;
    private @Nullable HttpClientInitializationException hpackException = null;
    public WebClientFactoryImpl(final @Reference ExtensibleTrustManager extensibleTrustManager) {
        this.extensibleTrustManager = extensibleTrustManager;
    protected void activate(Map<String, Object> parameters) {
        getConfigParameters(parameters);
    protected void modified(Map<String, Object> parameters) {
        if (threadPool != null) {
            threadPool.setMinThreads(minThreadsShared);
            threadPool.setMaxThreads(maxThreadsShared);
            threadPool.setIdleTimeout(keepAliveTimeoutShared * 1000);
        if (commonHttpClient != null) {
                commonHttpClient.stop();
                logger.error("error while stopping shared Jetty http client", e);
                // nothing else we can do here
            commonHttpClient = null;
            logger.debug("Jetty shared http client stopped");
        if (commonWebSocketClient != null) {
                commonWebSocketClient.stop();
                logger.error("error while stopping shared Jetty web socket client", e);
            commonWebSocketClient = null;
            logger.debug("Jetty shared web socket client stopped");
        threadPool = null;
    public HttpClient createHttpClient(String consumerName) {
        return createHttpClient(consumerName, null);
    public HttpClient createHttpClient(String consumerName, @Nullable SslContextFactory sslContextFactory) {
        logger.debug("http client for consumer {} requested", consumerName);
        checkConsumerName(consumerName);
        return createHttpClientInternal(consumerName, sslContextFactory, false, null);
    public WebSocketClient createWebSocketClient(String consumerName) {
        return createWebSocketClient(consumerName, null);
    public WebSocketClient createWebSocketClient(String consumerName, @Nullable SslContextFactory sslContextFactory) {
        logger.debug("web socket client for consumer {} requested", consumerName);
        return createWebSocketClientInternal(consumerName, sslContextFactory, false, null);
    public HttpClient getCommonHttpClient() {
        logger.debug("shared http client requested");
        return commonHttpClient;
    public WebSocketClient getCommonWebSocketClient() {
        logger.debug("shared web socket client requested");
        return commonWebSocketClient;
    private void getConfigParameters(Map<String, Object> parameters) {
        minThreadsShared = getConfigParameter(parameters, CONFIG_MIN_THREADS_SHARED, 10);
        maxThreadsShared = getConfigParameter(parameters, CONFIG_MAX_THREADS_SHARED, 40);
        keepAliveTimeoutShared = getConfigParameter(parameters, CONFIG_KEEP_ALIVE_SHARED, 300);
        minThreadsCustom = getConfigParameter(parameters, CONFIG_MIN_THREADS_CUSTOM, 5);
        maxThreadsCustom = getConfigParameter(parameters, CONFIG_MAX_THREADS_CUSTOM, 10);
        keepAliveTimeoutCustom = getConfigParameter(parameters, CONFIG_KEEP_ALIVE_CUSTOM, 300);
    @SuppressWarnings({ "null", "unused" })
    private int getConfigParameter(Map<String, Object> parameters, String parameter, int defaultValue) {
        Object value = parameters.get(parameter);
        if (value instanceof Integer integerValue) {
            return integerValue;
                return Integer.parseInt(string);
                logger.warn("ignoring invalid value {} for parameter {}", value, parameter);
            logger.warn("ignoring invalid type {} for parameter {}", value.getClass().getName(), parameter);
    private synchronized void initialize() {
            if (threadPool == null) {
                threadPool = createThreadPool("common", minThreadsShared, maxThreadsShared, keepAliveTimeoutShared);
            if (commonHttpClient == null) {
                commonHttpClient = createHttpClientInternal("common", null, true, threadPool);
                // we need to set the stop timeout AFTER the client has been started, because
                // otherwise the Jetty client sets it back to the default value.
                // We need the stop timeout in order to prevent blocking the deactivation of this
                // component, see https://github.com/eclipse/smarthome/issues/6632
                threadPool.setStopTimeout(0);
                logger.debug("Jetty shared http client created");
            if (commonWebSocketClient == null) {
                commonWebSocketClient = createWebSocketClientInternal("common", null, true, threadPool);
                logger.debug("Jetty shared web socket client created");
            throw new HttpClientInitializationException(
                    "unexpected checked exception during initialization of the jetty client", e);
    private HttpClient createHttpClientInternal(String consumerName, @Nullable SslContextFactory sslContextFactory,
            boolean startClient, @Nullable QueuedThreadPool threadPool) {
            logger.debug("creating http client for consumer {}", consumerName);
            HttpClient httpClient = new HttpClient(new CustomHttpClientTransportOverHTTP(),
                    sslContextFactory != null ? sslContextFactory : createSslContextFactory());
            // If proxy is set as property (standard Java property), provide the proxy information to Jetty HTTP
            // Client
            String httpProxyHost = System.getProperty("http.proxyHost");
            String httpsProxyHost = System.getProperty("https.proxyHost");
            if (httpProxyHost != null) {
                String sProxyPort = System.getProperty("http.proxyPort");
                if (sProxyPort != null) {
                        int port = Integer.parseInt(sProxyPort);
                        httpClient.getProxyConfiguration().getProxies().add(new HttpProxy(httpProxyHost, port));
                    } catch (NumberFormatException ex) {
                        // this was not a correct port. Ignoring.
                        logger.debug("HTTP Proxy detected (http.proxyHost), but invalid proxyport. Ignoring proxy.");
            } else if (httpsProxyHost != null) {
                String sProxyPort = System.getProperty("https.proxyPort");
                        httpClient.getProxyConfiguration().getProxies().add(new HttpProxy(httpsProxyHost, port));
                        logger.debug("HTTP Proxy detected (https.proxyHost), but invalid proxyport. Ignoring proxy.");
            httpClient.setMaxConnectionsPerDestination(2);
                httpClient.setExecutor(threadPool);
                final QueuedThreadPool queuedThreadPool = createThreadPool(consumerName, minThreadsCustom,
                        maxThreadsCustom, keepAliveTimeoutCustom);
                httpClient.setExecutor(queuedThreadPool);
            if (startClient) {
                    logger.error("Could not start Jetty http client", e);
                    throw new HttpClientInitializationException("Could not start Jetty http client", e);
                    "unexpected checked exception during initialization of the Jetty http client", e);
    private WebSocketClient createWebSocketClientInternal(String consumerName,
            @Nullable SslContextFactory sslContextFactory, boolean startClient, @Nullable QueuedThreadPool threadPool) {
            logger.debug("creating web socket client for consumer {}", consumerName);
            HttpClient httpClient = new HttpClient(
            WebSocketClient webSocketClient = new WebSocketClient(httpClient);
                    webSocketClient.start();
                    logger.error("Could not start Jetty web socket client", e);
                    throw new HttpClientInitializationException("Could not start Jetty web socket client", e);
            return webSocketClient;
                    "unexpected checked exception during initialization of the Jetty web socket client", e);
    private QueuedThreadPool createThreadPool(String consumerName, int minThreads, int maxThreads,
            int keepAliveTimeout) {
        QueuedThreadPool queuedThreadPool = new QueuedThreadPool(maxThreads, minThreads, keepAliveTimeout * 1000);
        queuedThreadPool.setName("OH-httpClient-" + consumerName);
        queuedThreadPool.setDaemon(true);
        return queuedThreadPool;
    private void checkConsumerName(String consumerName) {
        if (consumerName.length() < MIN_CONSUMER_NAME_LENGTH) {
                    "consumerName " + consumerName + " too short, minimum " + MIN_CONSUMER_NAME_LENGTH);
        if (consumerName.length() > MAX_CONSUMER_NAME_LENGTH) {
                    "consumerName " + consumerName + " too long, maximum " + MAX_CONSUMER_NAME_LENGTH);
        if (!CONSUMER_NAME_PATTERN.matcher(consumerName).matches()) {
                    "consumerName " + consumerName + " contains illegal character, allowed only [a-zA-Z0-9_-]");
    private SslContextFactory createSslContextFactory() {
        SslContextFactory sslContextFactory = new SslContextFactory.Client();
        sslContextFactory.setEndpointIdentificationAlgorithm("HTTPS");
            logger.debug("Setting up SSLContext for {}", extensibleTrustManager);
            SSLContext sslContext = SSLContext.getInstance("TLS");
            sslContext.init(null, new TrustManager[] { extensibleTrustManager }, null);
            sslContextFactory.setSslContext(sslContext);
        } catch (NoSuchAlgorithmException | KeyManagementException ex) {
            throw new HttpClientInitializationException("Cannot create an TLS context!", ex);
        return sslContextFactory;
    public HTTP2Client createHttp2Client(String consumerName) {
        return createHttp2Client(consumerName, null);
    public HTTP2Client createHttp2Client(String consumerName, @Nullable SslContextFactory sslContextFactory) {
        return createHttp2ClientInternal(consumerName, sslContextFactory);
    private HTTP2Client createHttp2ClientInternal(String consumerName, @Nullable SslContextFactory sslContextFactory) {
            logger.debug("creating HTTP/2 client for consumer {}", consumerName);
            if (!hpackLoadTestDone) {
                    PreEncodedHttpField field = new PreEncodedHttpField(HttpHeader.C_METHOD, "PUT");
                    ByteBuffer bytes = ByteBuffer.allocate(32);
                    field.putTo(bytes, HttpVersion.HTTP_2);
                    hpackException = null;
                    hpackException = new HttpClientInitializationException("Jetty HTTP/2 hpack module not loaded", e);
                hpackLoadTestDone = true;
            if (hpackException != null) {
                throw hpackException;
            HTTP2Client http2Client = new HTTP2Client();
            http2Client.addBean(sslContextFactory != null ? sslContextFactory : createSslContextFactory());
            http2Client.setExecutor(
                    createThreadPool(consumerName, minThreadsCustom, maxThreadsCustom, keepAliveTimeoutCustom));
            return http2Client;
                    "unexpected checked exception during initialization of the Jetty HTTP/2 client", e);
     * Extends the default {@link HttpClientTransportOverHTTP) but exposes the underling {@link EndPoint} of each
     * request/response.
     * It mimics the way it's done in higher Jetty Http client versions.
    private static class CustomHttpClientTransportOverHTTP extends HttpClientTransportOverHTTP {
        protected HttpConnectionOverHTTP newHttpConnection(EndPoint endPoint, HttpDestination destination,
                Promise<Connection> promise) {
            return new HttpConnectionOverHTTP(endPoint, destination, promise) {
                protected HttpChannelOverHTTP newHttpChannel() {
                    return new HttpChannelOverHTTP(this) {
                        protected HttpReceiverOverHTTP newHttpReceiver() {
                            return new CustomHttpReceiverOverHTTP(this);
        private static class CustomHttpReceiverOverHTTP extends HttpReceiverOverHTTP {
            private final HttpChannelOverHTTP channel;
            public CustomHttpReceiverOverHTTP(HttpChannelOverHTTP channel) {
                super(channel);
                this.channel = channel;
            public boolean headerComplete() {
                HttpExchange exchange = getHttpExchange();
                if (exchange != null) {
                    // Store the EndPoint is case of upgrades
                    exchange.getRequest().getConversation().setAttribute(EndPoint.class.getName(),
                            channel.getHttpConnection().getEndPoint());
                return super.headerComplete();
