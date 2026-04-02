package org.openhab.core.io.websocket.audio.internal;
import static java.nio.ByteBuffer.wrap;
import org.eclipse.jetty.websocket.servlet.ServletUpgradeRequest;
import org.eclipse.jetty.websocket.servlet.ServletUpgradeResponse;
import org.openhab.core.audio.AudioDialogProvider;
import org.openhab.core.io.websocket.WebSocketAdapter;
 * The {@link PCMWebSocketAdapter} creates instances of {@link PCMWebSocketConnection} to handle PCM audio streams.
@Component(immediate = true, service = { PCMWebSocketAdapter.class, WebSocketAdapter.class })
public class PCMWebSocketAdapter implements WebSocketAdapter {
    public static final String ADAPTER_ID = "audio-pcm";
    private final Logger logger = LoggerFactory.getLogger(PCMWebSocketAdapter.class);
    private final ScheduledExecutorService executor = ThreadPoolManager.getScheduledPool("audio-pcm-websocket");
    protected final BundleContext bundleContext;
    protected final AudioManager audioManager;
    protected final AudioDialogProvider audioDialogProvider;
    private final ScheduledFuture<?> pingTask;
    private final Set<PCMWebSocketConnection> webSocketConnections = Collections.synchronizedSet(new HashSet<>());
    public PCMWebSocketAdapter(BundleContext bundleContext, final @Reference AudioManager audioManager,
            final @Reference AudioDialogProvider audioDialogProvider) {
        this.audioDialogProvider = audioDialogProvider;
        this.pingTask = executor.scheduleWithFixedDelay(this::pingHandlers, 10, 5, TimeUnit.SECONDS);
    protected void onSpeakerConnected(PCMWebSocketConnection speaker) throws IllegalStateException {
        synchronized (webSocketConnections) {
            if (getSpeakerConnection(speaker.getId()) != null) {
                throw new IllegalStateException("Another speaker with the same id is already connected");
            webSocketConnections.add(speaker);
            logger.debug("connected speakers {}", webSocketConnections.size());
    protected void onClientDisconnected(PCMWebSocketConnection connection) {
        logger.debug("speaker disconnected '{}'", connection.getId());
            webSocketConnections.remove(connection);
    public @Nullable PCMWebSocketConnection getSpeakerConnection(String id) {
            return webSocketConnections.stream()
                    .filter(speakerConnection -> speakerConnection.getId().equalsIgnoreCase(id)).findAny().orElse(null);
        return ADAPTER_ID;
    public Object createWebSocket(ServletUpgradeRequest servletUpgradeRequest,
            ServletUpgradeResponse servletUpgradeResponse) {
        logger.debug("creating connection");
        return new PCMWebSocketConnection(this, executor);
    public synchronized void deactivate() {
        logger.debug("stopping connection check");
        pingTask.cancel(true);
        disconnectAll();
    private void pingHandlers() {
        ArrayList<PCMWebSocketConnection> handlers = new ArrayList<>(webSocketConnections);
        for (var handler : handlers) {
                boolean pinged = false;
                var remote = handler.getRemote();
                if (remote != null) {
                        remote.sendPing(wrap("oh".getBytes(StandardCharsets.UTF_8)));
                        pinged = true;
                if (!pinged) {
                    logger.debug("Ping failed, disconnecting speaker '{}'", handler.getId());
                    var session = handler.getSession();
                    if (session != null) {
                        session.close();
    private void disconnectAll() {
        logger.debug("Disconnecting {} clients...", webSocketConnections.size());
        ArrayList<PCMWebSocketConnection> connections = new ArrayList<>(webSocketConnections);
        for (var connection : connections) {
            onClientDisconnected(connection);
