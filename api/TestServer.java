import org.eclipse.jetty.server.Server;
import org.eclipse.jetty.server.ServerConnector;
import org.eclipse.jetty.servlet.ServletHandler;
 * Embedded jetty server used in the tests.
 * @author Velin Yordanov - Initial contribution
 * @author Henning Treu - provide in base test bundle
public class TestServer {
    private final Logger logger = LoggerFactory.getLogger(TestServer.class);
    private final Server server = new Server();
    private final String host;
    private final int port;
    private final ServletHolder servletHolder;
     * Creates a new {@link TestServer}. The server is started by {@link #startServer()} and stopped by
     * {@link #stopServer()}, preferably in the tests setup and tearDown methods.
     * @param host the host this server runs on.
     * @param port the port this server runs on. Use {@link TestPortUtil} to find a random free port.
     * @param timeout the idle timeout when receiving new messages on a connection in milliseconds.
     * @param servletHolder a {@link ServletHolder} which holds the {@link Servlet} content will be served from.
    public TestServer(String host, int port, int timeout, ServletHolder servletHolder) {
        this.servletHolder = servletHolder;
     * Starts the server and returns a {@link CompletableFuture}. The {@link CompletableFuture} gets completed as soon
     * as the server is ready to accept connections.
     * @return a {@link CompletableFuture} which completes as soon as the server is ready to accept connections.
    public CompletableFuture<Boolean> startServer() {
        final CompletableFuture<Boolean> serverStarted = new CompletableFuture<>();
        Thread thread = new Thread(new Runnable() {
                ServletHandler handler = new ServletHandler();
                handler.addServletWithMapping(servletHolder, "/*");
                server.setHandler(handler);
                // HTTP connector
                ServerConnector http = new ServerConnector(server);
                http.setHost(host);
                http.setPort(port);
                http.setIdleTimeout(timeout);
                server.addConnector(http);
                    server.start();
                    serverStarted.complete(true);
                    server.join();
                } catch (InterruptedException ex) {
                    logger.error("Server got interrupted", ex);
                    serverStarted.completeExceptionally(ex);
                    logger.error("Error in starting the server", e);
                    serverStarted.completeExceptionally(e);
        return serverStarted;
     * Stops the server.
    public void stopServer() {
            server.stop();
            logger.error("Error in stopping the server", e);
package org.openhab.core.io.net.tests.internal;
import org.eclipse.jetty.http2.server.HTTP2CServerConnectionFactory;
import org.eclipse.jetty.server.HttpConfiguration;
import org.eclipse.jetty.server.HttpConnectionFactory;
 * Based on {@code TestServer} of the FS Internet Radio Binding.
 * @author Wouter Born - Increase test coverage
 * @author Andrew Fiddian-Green - Adapted for org.openhab.core.io.net.tests
    private static final String SERVLET_PATH = "/servlet";
    private static final String WEBSOCKET_PATH = "/ws";
    private final Server server;
    public TestServer(String host, int port) {
        this.server = new Server();
    public URI getHttpUri() {
        return URI.create("http://" + host + ":" + port + SERVLET_PATH);
    public URI getWebSocketUri() {
        return URI.create("ws://" + host + ":" + port + WEBSOCKET_PATH);
    public void startServer() throws Exception {
        handler.addServletWithMapping(new ServletHolder(new TestHttpServlet()), SERVLET_PATH);
        handler.addServletWithMapping(new ServletHolder(new TestWebSocketServlet()), WEBSOCKET_PATH);
        HttpConfiguration httpConfig = new HttpConfiguration();
        HttpConnectionFactory h1 = new HttpConnectionFactory(httpConfig);
        HTTP2CServerConnectionFactory h2c = new HTTP2CServerConnectionFactory(httpConfig);
        ServerConnector connector = new ServerConnector(server, h1, h2c);
        connector.setHost(host);
        connector.setPort(port);
        server.addConnector(connector);
    public void stopServer() throws Exception {
