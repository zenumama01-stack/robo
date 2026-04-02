import org.eclipse.lsp4j.jsonrpc.Launcher;
import org.eclipse.lsp4j.launch.LSPLauncher;
import org.eclipse.lsp4j.services.LanguageClient;
import org.eclipse.xtext.ide.server.LanguageServerImpl;
import org.openhab.core.model.script.ScriptServiceUtil;
import org.openhab.core.model.script.engine.ScriptEngine;
import com.google.inject.Guice;
import com.google.inject.Injector;
 * A service component exposing a Language Server via sockets.
@Component(immediate = true, service = ModelServer.class, configurationPid = ModelServer.CONFIGURATION_PID, //
        property = Constants.SERVICE_PID + "=org.openhab.lsp")
@ConfigurableService(category = "system", label = "Language Server (LSP)", description_uri = ModelServer.CONFIG_URI)
public class ModelServer {
    public static final String CONFIGURATION_PID = "org.openhab.lsp";
    protected static final String CONFIG_URI = "system:lsp";
    private static final String KEY_PORT = "port";
    private static final int DEFAULT_PORT = 5007;
    private final ExecutorService pool = ThreadPoolManager.getPool("lsp");
    private final Logger logger = LoggerFactory.getLogger(ModelServer.class);
    private @Nullable ServerSocket socket;
    private final Injector injector;
    public ModelServer(final @Reference ScriptServiceUtil scriptServiceUtil,
            final @Reference ScriptEngine scriptEngine) {
        this.injector = Guice.createInjector(new RuntimeServerModule(scriptServiceUtil, scriptEngine));
        int port = DEFAULT_PORT;
            port = config.containsKey(KEY_PORT) ? Integer.parseInt(config.get(KEY_PORT).toString()) : DEFAULT_PORT;
            logger.warn("Couldn't parse '{}', using default port '{}' for the Language Server instead",
                    config.get(KEY_PORT), DEFAULT_PORT);
        final int serverPort = port;
        pool.submit(() -> listen(serverPort));
            if (socket != null && !socket.isClosed()) {
            logger.error("Error shutting down the Language Server", e);
    private void listen(int port) {
            socket = new ServerSocket(port);
            logger.info("Started Language Server Protocol (LSP) service on port {}", port);
            while (!socket.isClosed()) {
                logger.debug("Going to wait for a client to connect");
                    Socket client = socket.accept();
                    pool.submit(() -> handleConnection(client));
                    if (!socket.isClosed()) {
                        logger.error("Error accepting client connection: {}", e.getMessage());
            logger.error("Error starting the Language Server", e);
    private void handleConnection(final Socket client) {
        logger.debug("Client {} connected", client.getRemoteSocketAddress());
            LanguageServerImpl languageServer = injector.getInstance(LanguageServerImpl.class);
            Launcher<LanguageClient> launcher = LSPLauncher.createServerLauncher(languageServer,
                    client.getInputStream(), client.getOutputStream());
            languageServer.connect(launcher.getRemoteProxy());
            Future<?> future = launcher.startListening();
            future.get();
            logger.warn("Error communicating with LSP client {}", client.getRemoteSocketAddress());
            // go on, let the thread finish
            logger.error("Error running the Language Server", e);
        logger.debug("Client {} disconnected", client.getRemoteSocketAddress());
