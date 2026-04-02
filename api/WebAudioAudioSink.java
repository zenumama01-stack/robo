 * This is an audio sink that publishes an event through SSE and temporarily serves the stream via HTTP for web players
 * to pick it up.
public class WebAudioAudioSink extends AudioSinkAsync {
    private final Logger logger = LoggerFactory.getLogger(WebAudioAudioSink.class);
    private static final Set<AudioFormat> SUPPORTED_AUDIO_FORMATS = Set.of(AudioFormat.MP3, AudioFormat.WAV);
    private AudioHTTPServer audioHTTPServer;
    private EventPublisher eventPublisher;
    public WebAudioAudioSink(@Reference AudioHTTPServer audioHTTPServer, @Reference EventPublisher eventPublisher) {
        this.audioHTTPServer = audioHTTPServer;
    public void processAsynchronously(@Nullable AudioStream audioStream)
            // in case the audioStream is null, this should be interpreted as a request to end any currently playing
            // stream.
            sendEvent("");
        logger.debug("Received audio stream of format {}", audioStream.getFormat());
        // we need to serve it for a while and make it available to multiple clients
            StreamServed servedStream = audioHTTPServer.serve(audioStream, 10, true);
            // we will let the HTTP servlet run the delayed task when finished with the stream
            servedStream.playEnd().thenRun(() -> this.playbackFinished(audioStream));
            sendEvent(servedStream.url());
    private void sendEvent(String url) {
        PlayURLEvent event = WebAudioEventFactory.createPlayURLEvent(url);
        return "webaudio";
        return "Web Audio";
        throw new IOException("Web Audio sink does not support volume level changes.");
