import org.openhab.core.audio.FixedLengthAudioStream;
 * This is an {@link AudioSink} implementation connected to the {@link PCMWebSocketConnection} that allows to
 * transmit concurrent PCM audio lines through WebSocket.
 * To identify the different audio lines the data chucks are prefixed by a header added by the
 * {@link PCMWebSocketOutputStream} class.
public class PCMWebSocketAudioSink implements AudioSink {
     * Byte sent after the last chunk for a stream to indicate the stream has ended, so the client can dispose resources
     * associated with the stream.
     * It's sent on a finally clause that covers the audio transmission execution so it gets sent even if some
     * exception interrupts the audio transmission.
    private static final byte STREAM_TERMINATION_BYTE = (byte) 254;
    private static final Set<AudioFormat> SUPPORTED_FORMATS = Set.of(AudioFormat.WAV, AudioFormat.PCM_SIGNED);
    private static final Set<Class<? extends AudioStream>> SUPPORTED_STREAMS = Set.of(FixedLengthAudioStream.class,
            PipedAudioStream.class);
    private final Logger logger = LoggerFactory.getLogger(PCMWebSocketAudioSink.class);
    private final String sinkId;
    private final String sinkLabel;
    private final PCMWebSocketConnection websocket;
    private PercentType sinkVolume = new PercentType(100);
    private Integer forceSampleRate;
    private Integer forceBitDepth;
    private Integer forceChannels;
    public PCMWebSocketAudioSink(String id, String label, PCMWebSocketConnection websocket,
            @Nullable Integer forceSampleRate, @Nullable Integer forceBitDepth, @Nullable Integer forceChannels) {
        this.sinkId = id;
        this.sinkLabel = label;
        this.websocket = websocket;
        this.forceSampleRate = forceSampleRate;
        this.forceBitDepth = forceBitDepth;
        this.forceChannels = forceChannels;
        return this.sinkId;
        return this.sinkLabel;
        OutputStream outputStream = null;
            long duration = -1;
            if (AudioFormat.CONTAINER_WAVE.equals(audioStream.getFormat().getContainer())) {
                logger.debug("Removing wav container from data");
                    logger.warn("IOException trying to remove wav header: {}", e.getMessage());
            var audioFormat = audioStream.getFormat();
            if (audioStream instanceof SizeableAudioStream sizeableAudioStream) {
                long byteLength = sizeableAudioStream.length();
                long bytesPerSecond = (Objects.requireNonNull(audioFormat.getBitDepth()) / 8)
                        * Objects.requireNonNull(audioFormat.getFrequency())
                        * Objects.requireNonNull(audioFormat.getChannels());
                float durationInSeconds = (float) byteLength / bytesPerSecond;
                duration = Math.round(durationInSeconds * 1000);
                logger.debug("Duration of input stream : {}", duration);
            AtomicBoolean transferenceAborted = new AtomicBoolean(false);
            if (audioStream instanceof PipedAudioStream pipedAudioStream) {
                pipedAudioStream.onClose(() -> transferenceAborted.set(true));
            int sampleRate = Objects.requireNonNull(audioFormat.getFrequency()).intValue();
            int bitDepth = Objects.requireNonNull(audioFormat.getBitDepth());
            int channels = Objects.requireNonNull(audioFormat.getChannels());
            int targetSampleRate = Objects.requireNonNullElse(forceSampleRate, sampleRate);
            Integer targetBitDepth = Objects.requireNonNullElse(forceBitDepth, bitDepth);
            Integer targetChannels = Objects.requireNonNullElse(forceChannels, channels);
            outputStream = new PCMWebSocketOutputStream(websocket, targetSampleRate, targetBitDepth.byteValue(),
                    targetChannels.byteValue());
            InputStream finalAudioStream;
            if ( //
            (forceSampleRate != null && !forceSampleRate.equals(sampleRate)) || //
                    (forceBitDepth != null && !forceBitDepth.equals(bitDepth)) || //
                    (forceChannels != null && !forceChannels.equals(channels)) //
                logger.debug("Sound is not in the target format. Trying to re-encode it");
                finalAudioStream = PCMWebSocketAudioUtil.getPCMStreamNormalized(audioStream, sampleRate, bitDepth,
                        channels, targetSampleRate, targetBitDepth, targetChannels);
                finalAudioStream = audioStream;
            int bytesPer500ms = (targetSampleRate * (targetBitDepth / 8) * targetChannels) / 2;
            transferAudio(finalAudioStream, outputStream, bytesPer500ms, duration, transferenceAborted);
        } catch (InterruptedIOException ignored) {
            logger.warn("IOException: {}", e.getMessage());
            logger.warn("InterruptedException: {}", e.getMessage());
                logger.warn("IOException: {}", e.getMessage(), e);
                if (outputStream != null) {
                    outputStream.close();
    private void transferAudio(InputStream inputStream, OutputStream outputStream, int chunkSize, long duration,
            AtomicBoolean aborted) throws IOException, InterruptedException {
        Instant start = Instant.now();
        long transferred = 0;
            byte[] buffer = new byte[chunkSize];
            while (!aborted.get() && (read = inputStream.read(buffer, 0, chunkSize)) >= 0) {
                outputStream.write(buffer, 0, read);
                transferred += read;
                // send a byte indicating this stream has ended, so it can be tear down on the client
                outputStream.write(new byte[] { STREAM_TERMINATION_BYTE }, 0, 1);
                logger.warn("Unable to send termination byte to sink {}", sinkId);
        logger.debug("Sent {} bytes of audio", transferred);
        if (duration != -1) {
            Instant end = Instant.now();
            long millisSecondTimedToSendAudioData = Duration.between(start, end).toMillis();
            if (millisSecondTimedToSendAudioData < duration) {
                long timeToSleep = duration - millisSecondTimedToSendAudioData;
                logger.debug("Sleep time to let the system play sound : {}ms", timeToSleep);
                Thread.sleep(timeToSleep);
        return SUPPORTED_FORMATS;
        return SUPPORTED_STREAMS;
        return this.sinkVolume;
    public void setVolume(PercentType percentType) throws IOException {
        this.sinkVolume = percentType;
        websocket.setSinkVolume(percentType.intValue());
     * This is an {@link OutputStream} implementation for writing binary data to the websocket that
     * will prefix each chunk with a header composed of 8 bytes.
     * Header: 2 bytes (stream id) + 4 byte (stream sample rate) + 1 byte (stream bit depth) + 1 byte (channels).
    protected static class PCMWebSocketOutputStream extends OutputStream {
        private final byte[] header;
        public PCMWebSocketOutputStream(PCMWebSocketConnection websocket, int sampleRate, byte bitDepth,
                byte channels) {
            this.header = PCMWebSocketStreamIdUtil.generateAudioPacketHeader(sampleRate, bitDepth, channels).array();
            write(ByteBuffer.allocate(4).putInt(b).array());
        public void write(byte @Nullable [] b) throws IOException {
                throw new IOException("Stream closed");
            if (b != null) {
                websocket.sendAudio(header, b);
        public void write(byte @Nullable [] b, int off, int len) throws IOException {
                write(Arrays.copyOfRange(b, off, off + len));
