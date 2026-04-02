import javazoom.jl.decoder.JavaLayerException;
import javazoom.jl.player.Player;
import javax.sound.sampled.FloatControl;
import javax.sound.sampled.LineUnavailableException;
import javax.sound.sampled.Port;
import org.openhab.core.audio.AudioSinkAsync;
import org.openhab.core.audio.PipedAudioStream;
import org.openhab.core.audio.UnsupportedAudioFormatException;
import org.openhab.core.audio.UnsupportedAudioStreamException;
import org.openhab.core.common.NamedThreadFactory;
 * This is an audio sink that is registered as a service, which can play wave files to the hosts outputs (e.g. speaker,
 * line-out).
 * @author Miguel Álvarez Díez - Added piped audio stream support
@Component(service = AudioSink.class, immediate = true)
public class JavaSoundAudioSink extends AudioSinkAsync {
    private final Logger logger = LoggerFactory.getLogger(JavaSoundAudioSink.class);
    private boolean isMac = false;
    private @Nullable PercentType macVolumeValue = null;
    private @Nullable static Player streamPlayer = null;
    private NamedThreadFactory threadFactory = new NamedThreadFactory("audio");
    private static final Set<AudioFormat> SUPPORTED_AUDIO_FORMATS = Set.of(AudioFormat.MP3, AudioFormat.WAV,
            AudioFormat.PCM_SIGNED);
    // we accept any stream
    private static final Set<Class<? extends AudioStream>> SUPPORTED_AUDIO_STREAMS = Set.of(AudioStream.class);
    protected void activate(BundleContext context) {
        String os = context.getProperty(Constants.FRAMEWORK_OS_NAME);
        if (os != null && os.toLowerCase().startsWith("macos")) {
            isMac = true;
    public synchronized void processAsynchronously(final @Nullable AudioStream audioStream)
        if (audioStream instanceof PipedAudioStream pipedAudioStream
                && AudioFormat.PCM_SIGNED.isCompatible(pipedAudioStream.getFormat())) {
            pipedAudioStream.onClose(() -> playbackFinished(pipedAudioStream));
            AudioPlayer audioPlayer = new AudioPlayer(pipedAudioStream);
            audioPlayer.start();
                audioPlayer.join();
            } catch (InterruptedException e) {
                logger.debug("Audio stream has been interrupted.");
        } else if (audioStream != null && !AudioFormat.CODEC_MP3.equals(audioStream.getFormat().getCodec())) {
            AudioPlayer audioPlayer = new AudioPlayer(audioStream);
                playbackFinished(audioStream);
                logger.error("Playing audio has been interrupted.");
            if (audioStream == null || audioStream instanceof URLAudioStream) {
                // we are dealing with an infinite stream here
                if (streamPlayer instanceof Player player) {
                    // if we are already playing a stream, stop it first
                    player.close();
                    streamPlayer = null;
                    // the call was only for stopping the currently playing stream
                        // we start a new continuous stream and store its handle
                        playInThread(audioStream, true);
                    } catch (JavaLayerException e) {
                        logger.error("An exception occurred while playing url audio stream : '{}'", e.getMessage());
                // we are playing some normal file (no url stream)
                    playInThread(audioStream, false);
                    logger.error("An exception occurred while playing audio : '{}'", e.getMessage());
    private void playInThread(final AudioStream audioStream, boolean store) throws JavaLayerException {
        // run in new thread
        Player streamPlayerFinal = new Player(audioStream);
        if (store) { // we store its handle in case we want to interrupt it.
            streamPlayer = streamPlayerFinal;
        threadFactory.newThread(() -> {
                streamPlayerFinal.play();
                streamPlayerFinal.close();
        }).start();
            // stop playing streams on shutdown
    public Set<AudioFormat> getSupportedFormats() {
        return SUPPORTED_AUDIO_FORMATS;
    public Set<Class<? extends AudioStream>> getSupportedStreams() {
        return SUPPORTED_AUDIO_STREAMS;
        return "enhancedjavasound";
    public @Nullable String getLabel(@Nullable Locale locale) {
        return "System Speaker";
    public PercentType getVolume() throws IOException {
        if (!isMac) {
            final Float[] volumes = new Float[1];
            runVolumeCommand((FloatControl input) -> {
                FloatControl volumeControl = input;
                volumes[0] = volumeControl.getValue();
            if (volumes[0] != null) {
                return new PercentType(Math.round(volumes[0] * 100f));
                logger.warn("Cannot determine master volume level - assuming 100%");
                return PercentType.HUNDRED;
            // we use a cache of the value as the script execution is pretty slow
            PercentType cachedVolume = macVolumeValue;
            if (cachedVolume == null) {
                Process p = Runtime.getRuntime()
                        .exec(new String[] { "osascript", "-e", "output volume of (get volume settings)" });
                String value;
                try (Scanner scanner = new Scanner(p.getInputStream(), StandardCharsets.UTF_8.name())) {
                    value = scanner.useDelimiter("\\A").next().strip();
                    cachedVolume = new PercentType(value);
                    macVolumeValue = cachedVolume;
                    logger.warn("Cannot determine master volume level, received response '{}' - assuming 100%", value);
            return cachedVolume;
    public void setVolume(final PercentType volume) throws IOException {
        if (volume.intValue() < 0 || volume.intValue() > 100) {
            throw new IllegalArgumentException("Volume value must be in the range [0,100]!");
                input.setValue(volume.floatValue() / 100f);
            Runtime.getRuntime()
                    .exec(new String[] { "osascript", "-e", "set volume output volume " + volume.intValue() });
            macVolumeValue = volume;
    private void runVolumeCommand(Function<FloatControl, Boolean> closure) {
        Mixer.Info[] infos = AudioSystem.getMixerInfo();
        for (Mixer.Info info : infos) {
            Mixer mixer = AudioSystem.getMixer(info);
            if (mixer.isLineSupported(Port.Info.SPEAKER)) {
                Port port;
                    port = (Port) mixer.getLine(Port.Info.SPEAKER);
                    port.open();
                    if (port.isControlSupported(FloatControl.Type.VOLUME)) {
                        FloatControl volume = (FloatControl) port.getControl(FloatControl.Type.VOLUME);
                        closure.apply(volume);
                    port.close();
                } catch (LineUnavailableException e) {
                    logger.error("Cannot access master volume control", e);
