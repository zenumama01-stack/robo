 * This is an {@link AudioSource} implementation connected to a {@link PCMWebSocketConnection} supporting
 * a single PCM audio line through a WebSocket connection and shared across all active {@link AudioStream} instances.
public class PCMWebSocketAudioSource implements AudioSource {
    private static final int SUPPORTED_BIT_DEPTH = 16;
    private static final int SUPPORTED_SAMPLE_RATE = 16000;
    private static final int SUPPORTED_CHANNELS = 1;
    public static AudioFormat supportedFormat = new AudioFormat(AudioFormat.CONTAINER_WAVE,
            AudioFormat.CODEC_PCM_SIGNED, false, SUPPORTED_BIT_DEPTH, null, (long) SUPPORTED_SAMPLE_RATE,
            SUPPORTED_CHANNELS);
    private final Logger logger = LoggerFactory.getLogger(PCMWebSocketAudioSource.class);
    private final String sourceId;
    private final String sourceLabel;
    private @Nullable PipedOutputStream sourceAudioPipedOutput;
    private @Nullable PipedInputStream sourceAudioPipedInput;
    private @Nullable InputStream sourceAudioStream;
    private final PipedAudioStream.Group streamGroup = PipedAudioStream.newGroup(supportedFormat);
    private @Nullable Future<?> sourceWriteTask;
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool("pcm-audio-source");
    private byte @Nullable [] streamId;
    public PCMWebSocketAudioSource(String id, String label, PCMWebSocketConnection websocket) {
        this.sourceId = id;
        this.sourceLabel = label;
        return this.sourceId;
        return this.sourceLabel;
        return Set.of(supportedFormat);
    public AudioStream getInputStream(AudioFormat audioFormat) throws AudioException {
            final PipedAudioStream stream = streamGroup.getAudioStreamInGroup();
                if (this.streamGroup.size() == 1) {
                    logger.debug("Send start listening {}", getId());
                    this.streamId = null;
                    websocket.setListening(true);
            stream.onClose(this::onStreamClose);
            throw new AudioException(e);
        streamGroup.close();
    public void writeToStreams(byte[] id, int sampleRate, int bitDepth, int channels, byte[] payload) {
            logger.debug("Source already disposed, ignoring data");
        if (this.streamId == null) {
            this.streamId = id;
        } else if (!Arrays.equals(this.streamId, id)) {
            logger.warn("Only one concurrent data line is supported, ignoring data from source stream {}", id);
        boolean needsConvert = sampleRate != SUPPORTED_SAMPLE_RATE || bitDepth != SUPPORTED_BIT_DEPTH
                || channels != SUPPORTED_CHANNELS;
        if (!needsConvert) {
            streamGroup.write(payload);
        if (this.sourceAudioPipedOutput == null || this.sourceAudioStream == null) {
                this.sourceAudioPipedOutput = new PipedOutputStream();
                var sourceAudioPipedInput = this.sourceAudioPipedInput = new PipedInputStream(
                        this.sourceAudioPipedOutput, (sampleRate * (bitDepth / 8) * channels) * 2);
                        "Enabling converting pcm audio for the audio source stream: sample rate {}, bit depth {}, channels {} => sample rate {}, bit depth {}, channels {}",
                        sampleRate, bitDepth, channels, SUPPORTED_SAMPLE_RATE, SUPPORTED_BIT_DEPTH, SUPPORTED_CHANNELS);
                this.sourceAudioStream = PCMWebSocketAudioUtil.getPCMStreamNormalized(sourceAudioPipedInput, sampleRate,
                        bitDepth, channels, SUPPORTED_SAMPLE_RATE, SUPPORTED_BIT_DEPTH, SUPPORTED_CHANNELS);
                sourceWriteTask = scheduler.submit(() -> {
                    int bytesPer250ms = (SUPPORTED_SAMPLE_RATE * (SUPPORTED_BIT_DEPTH / 8) * SUPPORTED_CHANNELS) / 4;
                        byte[] convertedPayload;
                            convertedPayload = this.sourceAudioStream.readNBytes(bytesPer250ms);
                            Thread.sleep(0);
                        } catch (InterruptedIOException | InterruptedException e) {
                            if (e.getMessage().contains("Pipe closed")) {
                            logger.error("Error reading converted audio data", e);
                        streamGroup.write(convertedPayload);
                logger.error("Unable to setup audio source stream", e);
            this.sourceAudioPipedOutput.write(payload);
            logger.error("Error converting source audio format", e);
    private void onStreamClose() {
        logger.debug("Unregister source audio stream for '{}'", getId());
                logger.debug("Send stop listening {}", getId());
                websocket.setListening(false);
                if (this.sourceWriteTask != null) {
                    this.sourceWriteTask.cancel(true);
                    this.sourceWriteTask = null;
                if (this.sourceAudioStream != null) {
                        this.sourceAudioStream.close();
                    this.sourceAudioStream = null;
                if (this.sourceAudioPipedOutput != null) {
                        this.sourceAudioPipedOutput.close();
                    this.sourceAudioPipedOutput = null;
                if (this.sourceAudioPipedInput != null) {
                        this.sourceAudioPipedInput.close();
                    this.sourceAudioPipedInput = null;
