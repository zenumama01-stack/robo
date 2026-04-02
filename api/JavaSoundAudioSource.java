import javax.sound.sampled.TargetDataLine;
 * This is an AudioSource from an input channel of the host.
 * @author Kai Kreuzer - Refactored and stabilized
 * @author Miguel Álvarez - Share microphone line only under Windows OS
 * @author Miguel Álvarez - Share microphone line on all OS using piped audio streams
@Component(service = AudioSource.class, immediate = true)
public class JavaSoundAudioSource implements AudioSource {
    private final Logger logger = LoggerFactory.getLogger(JavaSoundAudioSource.class);
     * Java Sound audio format
    private final javax.sound.sampled.AudioFormat format = new javax.sound.sampled.AudioFormat(16000.0f, 16, 1, true,
     * AudioFormat of the JavaSoundAudioSource
    private final AudioFormat audioFormat = convertAudioFormat(format);
     * TargetDataLine for sharing the mic
    private @Nullable TargetDataLine microphone;
     * Group for microphone sharing
    private final PipedAudioStream.Group streamGroup = PipedAudioStream.newGroup(audioFormat);
     * Task for writing microphone data to each of the open sources
    private @Nullable Future<?> pipeWriteTask;
    private final ScheduledExecutorService executor;
     * Constructs a JavaSoundAudioSource
    public JavaSoundAudioSource() {
        executor = ThreadPoolManager.getScheduledPool("OH-core-javasound-source");
    private TargetDataLine initMicrophone(javax.sound.sampled.AudioFormat format) throws AudioException {
            TargetDataLine microphone = AudioSystem.getTargetDataLine(format);
            microphone.open(format);
            return microphone;
            throw new AudioException("Error creating the audio input stream: " + e.getMessage(), e);
    public AudioStream getInputStream(AudioFormat expectedFormat) throws AudioException {
        if (!expectedFormat.isCompatible(audioFormat)) {
            throw new AudioException("Cannot produce streams in format " + expectedFormat);
        synchronized (streamGroup) {
            TargetDataLine microphone = this.microphone;
            if (microphone == null) {
                microphone = initMicrophone(format);
                this.microphone = microphone;
            PipedAudioStream audioStream;
                audioStream = streamGroup.getAudioStreamInGroup();
                        if (streamGroup.isEmpty()) {
                            Future<?> pipeWriteTask = this.pipeWriteTask;
                            if (pipeWriteTask != null) {
                                pipeWriteTask.cancel(true);
                                this.pipeWriteTask = null;
                            TargetDataLine microphoneDataLine = this.microphone;
                            if (microphoneDataLine != null) {
                                microphoneDataLine.close();
                                this.microphone = null;
            } catch (IOException ie) {
                throw new AudioException("Cannot open stream pipe: " + ie.getMessage());
            var inputStream = new JavaSoundInputStream(audioStream, audioFormat);
            microphone.start();
            startPipeWrite();
            return inputStream;
    private void startPipeWrite() {
        if (this.pipeWriteTask == null) {
            this.pipeWriteTask = executor.submit(() -> {
                int lengthRead;
                byte[] buffer = new byte[1024];
                while (!streamGroup.isEmpty()) {
                    TargetDataLine stream = this.microphone;
                            lengthRead = stream.read(buffer, 0, buffer.length);
                            streamGroup.write(buffer, 0, lengthRead);
                            streamGroup.flush();
                            logger.warn("RuntimeException while reading from JavaSound source: {}", e.getMessage());
                        logger.warn("Unable access to microphone stream");
        return "javasound";
     * Converts a javax.sound.sampled.AudioFormat to an org.openhab.core.audio.AudioFormat
     * @param audioFormat the AudioFormat to convert
     * @return The converted AudioFormat
    private static AudioFormat convertAudioFormat(javax.sound.sampled.AudioFormat audioFormat) {
        String container = AudioFormat.CONTAINER_NONE;
        String codec = audioFormat.getEncoding().toString();
        Boolean bigEndian = Boolean.valueOf(audioFormat.isBigEndian());
        int frameSize = audioFormat.getFrameSize(); // In bytes
        int bitsPerFrame = frameSize * 8;
        Integer bitDepth = ((AudioSystem.NOT_SPECIFIED == frameSize) ? null : Integer.valueOf(bitsPerFrame));
        float frameRate = audioFormat.getFrameRate();
        Integer bitRate = ((AudioSystem.NOT_SPECIFIED == frameRate) ? null
                : Integer.valueOf((int) (frameRate * bitsPerFrame)));
        float sampleRate = audioFormat.getSampleRate();
        Long frequency = ((AudioSystem.NOT_SPECIFIED == sampleRate) ? null : Long.valueOf((long) sampleRate));
        return new AudioFormat(container, codec, bigEndian, bitDepth, bitRate, frequency);
    public String getLabel(@Nullable Locale locale) {
        return "System Microphone";
        return Set.of(audioFormat);
