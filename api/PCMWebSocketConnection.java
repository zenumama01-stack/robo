import org.eclipse.jetty.websocket.api.RemoteEndpoint;
import org.eclipse.jetty.websocket.api.Session;
import org.eclipse.jetty.websocket.api.WebSocketListener;
import org.eclipse.jetty.websocket.api.annotations.WebSocket;
 * The {@link PCMWebSocketConnection} represents a WebSocket connection used to transmit PCM audio.
 * The websocket uses the text protocol for send commands represented by {@link WebSocketCommand} and the binary
 * protocol to transmit the audio data.
 * The websocket supports only one line for the {@link PCMWebSocketAudioSource} (the audio is shared on the server),
 * the data transmission is instructed by the server (START_LISTENING and STOP_LISTENING commands).
 * The websocket supports multiple lines for the {@link PCMWebSocketAudioSink} (to accomplish that, the outgoing data
 * chucks are prefixed with a 6 byte header to transmit the identity and format specification, check
 * {@link PCMWebSocketAudioSink.PCMWebSocketOutputStream})
@WebSocket
public class PCMWebSocketConnection implements WebSocketListener {
    private final Logger logger = LoggerFactory.getLogger(PCMWebSocketConnection.class);
    protected final Map<String, ServiceRegistration<?>> audioComponentRegistrations = new ConcurrentHashMap<>();
    private volatile @Nullable Session session;
    private @Nullable RemoteEndpoint remote;
    private final PCMWebSocketAdapter wsAdapter;
    private @Nullable ScheduledFuture<?> scheduledDisconnection;
    private boolean initialized = false;
    private @Nullable Runnable dialogTrigger = null;
    private final ObjectMapper jsonMapper = new ObjectMapper();
    private String id = "";
    private @Nullable PCMWebSocketAudioSource audioSource = null;
    public PCMWebSocketConnection(PCMWebSocketAdapter wsAdapter, ScheduledExecutorService executor) {
        this.wsAdapter = wsAdapter;
        this.executor = executor;
    public void sendAudio(byte[] id, byte[] b) {
            var remote = getRemote();
                // concat stream identifier and send
                ByteBuffer buff = ByteBuffer.wrap(new byte[id.length + b.length]);
                buff.put(id);
                buff.put(b);
                remote.sendBytesByFuture(ByteBuffer.wrap(buff.array()));
        } catch (IllegalStateException ignored) {
            logger.warn("Unable to send audio buffer");
    public void setListening(boolean listening) {
        sendClientCommand(new WebSocketCommand(listening ? WebSocketCommand.OutputCommands.START_LISTENING
                : WebSocketCommand.OutputCommands.STOP_LISTENING));
    public void disconnect() {
        var session = getSession();
    public void onWebSocketConnect(@Nullable Session sess) {
        if (sess == null) {
            // never
        this.session = sess;
        this.remote = sess.getRemote();
        logger.debug("New client connected.");
        scheduledDisconnection = executor.schedule(() -> {
                sess.disconnect();
        }, 5, TimeUnit.SECONDS);
    private <T extends WebSocketCommand> void sendClientCommand(T msg) {
                remote.sendStringByFuture(new ObjectMapper().writeValueAsString(msg));
                logger.warn("JsonProcessingException writing JSON message", e);
    public void onWebSocketBinary(byte @Nullable [] payload, int offset, int len) {
        logger.trace("Received binary data of length {}", len);
        PCMWebSocketAudioSource audioSource = this.audioSource;
        if (payload != null && audioSource != null) {
            PCMWebSocketStreamIdUtil.AudioPacketData streamData;
                streamData = PCMWebSocketStreamIdUtil.parseAudioPacket(payload);
                logger.warn("Exception processing binary message: {}", e.getMessage());
            audioSource.writeToStreams(streamData.id(), streamData.sampleRate(), streamData.bitDepth(),
                    streamData.channels(), streamData.audioData());
    public void onWebSocketText(@Nullable String message) {
            JsonNode rootMessageNode = jsonMapper.readTree(message);
            if (rootMessageNode.has("cmd")) {
                String cmd = rootMessageNode.get("cmd").asText().trim().toUpperCase();
                    logger.debug("Handling msg '{}'", cmd);
                    var messageType = WebSocketCommand.InputCommands.valueOf(cmd);
                    switch (messageType) {
                        case INITIALIZE -> {
                            wsAdapter.onSpeakerConnected(this);
                            JsonNode argsNode = rootMessageNode.get("args");
                            var clientOptions = jsonMapper.treeToValue(argsNode, ConnectionOptions.class);
                            var scheduledDisconnection = this.scheduledDisconnection;
                            if (scheduledDisconnection != null) {
                                scheduledDisconnection.cancel(true);
                            // update connection settings
                            id = clientOptions.id;
                            registerSpeakerComponents(id, clientOptions);
                            sendClientCommand(new WebSocketCommand(WebSocketCommand.OutputCommands.INITIALIZED));
                        case ON_SPOT -> onRemoteSpot();
                } catch (IOException | IllegalStateException e) {
                    logger.warn("Error handing command '{}' with message: {}. Disconnecting client", cmd,
                    disconnect();
            logger.warn("Exception parsing JSON message.", e);
            logger.warn("Disconnecting client.");
    public void onWebSocketError(@Nullable Throwable cause) {
        logger.warn("WebSocket Error", cause);
    public void onWebSocketClose(int statusCode, @Nullable String reason) {
        this.session = null;
        this.remote = null;
        logger.debug("Session closed with code {}: {}", statusCode, reason);
        wsAdapter.onClientDisconnected(this);
        unregisterSpeakerComponents(id);
    public void setSinkVolume(int value) {
        if (initialized) {
            sendClientCommand(
                    new WebSocketCommand(WebSocketCommand.OutputCommands.SINK_VOLUME, Map.of("value", value)));
    public void setSourceVolume(int value) {
                    new WebSocketCommand(WebSocketCommand.OutputCommands.SOURCE_VOLUME, Map.of("value", value)));
    public @Nullable RemoteEndpoint getRemote() {
        return this.remote;
    public @Nullable Session getSession() {
        return this.session;
    public boolean isConnected() {
        Session sess = this.session;
        return sess != null && sess.isOpen();
    private synchronized void registerSpeakerComponents(String id, ConnectionOptions clientOptions) throws IOException {
            throw new IOException("Unable to register audio components");
        String label = "PCM Audio WebSocket (" + id + ")";
        logger.debug("Registering dialog components for '{}'", id);
        // register source
        var audioSource = this.audioSource = new PCMWebSocketAudioSource(getSourceId(id), label, this);
        logger.debug("Registering audio source {}", this.audioSource.getId());
        audioComponentRegistrations.put(this.audioSource.getId(), wsAdapter.bundleContext
                .registerService(AudioSource.class.getName(), this.audioSource, new Hashtable<>()));
        // register sink
        var audioSink = new PCMWebSocketAudioSink(getSinkId(id), label, this, clientOptions.forceSampleRate,
                clientOptions.forceBitDepth, clientOptions.forceChannels);
        logger.debug("Registering audio sink {}", audioSink.getId());
        audioComponentRegistrations.put(audioSink.getId(),
                wsAdapter.bundleContext.registerService(AudioSink.class.getName(), audioSink, new Hashtable<>()));
        // init dialog
        if (clientOptions.startDialog) {
            var dialogProvider = this.wsAdapter.audioDialogProvider;
            if (dialogProvider == null) {
                throw new IOException("Voice functionality is not ready");
            dialogTrigger = dialogProvider.startDialog(audioSink, audioSource,
                    !clientOptions.locationItem.isBlank() ? clientOptions.locationItem : null,
                    !clientOptions.listeningItem.isBlank() ? clientOptions.listeningItem : null, () -> disconnect());
            dialogTrigger = null;
    private synchronized void unregisterSpeakerComponents(String id) {
        var source = wsAdapter.audioManager.getSource(getSourceId(id));
        if (source instanceof PCMWebSocketAudioSource hsAudioSource) {
                hsAudioSource.close();
            } catch (Exception ignored) {
            ServiceRegistration<?> sourceReg = audioComponentRegistrations.remove(source.getId());
            if (sourceReg != null) {
                logger.debug("Unregistering audio source {}", source.getId());
                sourceReg.unregister();
        var sink = wsAdapter.audioManager.getSink(getSinkId(id));
            ServiceRegistration<?> sinkReg = audioComponentRegistrations.remove(sink.getId());
            if (sinkReg != null) {
                logger.debug("Unregistering audio sink {}", sink.getId());
                sinkReg.unregister();
    private void onRemoteSpot() {
        var dialogTrigger = this.dialogTrigger;
        if (dialogTrigger != null) {
            dialogTrigger.run();
    private String getSinkId(String id) {
        return "pcm::" + id + "::sink";
    private String getSourceId(String id) {
        return "pcm::" + id + "::source";
    private static class WebSocketCommand {
        public String cmd = "";
        public Map<String, Object> args;
        public WebSocketCommand(OutputCommands cmd) {
            this(cmd, new HashMap<>());
        public WebSocketCommand(OutputCommands cmd, Map<String, Object> args) {
            this.cmd = cmd.name();
            this.args = args;
        public enum OutputCommands {
            INITIALIZED,
            START_LISTENING,
            STOP_LISTENING,
            SINK_VOLUME,
            SOURCE_VOLUME,
        public enum InputCommands {
            INITIALIZE,
            ON_SPOT,
     * The {@link ConnectionOptions} represents the options provided by the ws client.
    public static class ConnectionOptions {
         * Identifier to concatenate to related services (dialog, source and sick)
         * Force sink audio sample rate (resample in server)
        public @Nullable Integer forceSampleRate;
         * Force sink audio bit depth (resample in server)
        public @Nullable Integer forceBitDepth;
         * Force sink audio channels (resample in server)
        public @Nullable Integer forceChannels;
         * Start a dialog processor using the registered audio components
        public boolean startDialog = false;
         * Listening item for the dialog
        public String listeningItem = "";
         * Location item for the dialog
        public String locationItem = "";
        public ConnectionOptions() {
