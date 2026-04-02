import java.io.FileOutputStream;
import java.nio.ByteBuffer;
import java.text.ParseException;
import javax.sound.sampled.AudioFileFormat;
import javax.sound.sampled.AudioInputStream;
import javax.sound.sampled.AudioSystem;
import org.openhab.core.audio.AudioFormat;
import org.openhab.core.audio.AudioStream;
import org.openhab.core.audio.FileAudioStream;
import org.openhab.core.audio.URLAudioStream;
import org.openhab.core.audio.utils.ToneSynthesizer;
import org.openhab.core.config.core.ConfigOptionProvider;
import org.openhab.core.config.core.ParameterOption;
 * @author Miguel Álvarez - Add record from source
@Component(immediate = true, configurationPid = "org.openhab.audio", //
        property = Constants.SERVICE_PID + "=org.openhab.audio")
@ConfigurableService(category = "system", label = "Audio", description_uri = AudioManagerImpl.CONFIG_URI)
public class AudioManagerImpl implements AudioManager, ConfigOptionProvider {
    static final String CONFIG_URI = "system:audio";
    static final String CONFIG_DEFAULT_SINK = "defaultSink";
    static final String CONFIG_DEFAULT_SOURCE = "defaultSource";
    private final Logger logger = LoggerFactory.getLogger(AudioManagerImpl.class);
    // service maps
    private final Map<String, AudioSource> audioSources = new ConcurrentHashMap<>();
    private final Map<String, AudioSink> audioSinks = new ConcurrentHashMap<>();
     * default settings filled through the service configuration
    private @Nullable String defaultSource;
    private @Nullable String defaultSink;
    protected void activate(Map<String, Object> config) {
    void modified(@Nullable Map<String, Object> config) {
            this.defaultSource = config.get(CONFIG_DEFAULT_SOURCE) instanceof Object source ? source.toString() : null;
            this.defaultSink = config.get(CONFIG_DEFAULT_SINK) instanceof Object sink ? sink.toString() : null;
    public void play(@Nullable AudioStream audioStream) {
        play(audioStream, null);
    public void play(@Nullable AudioStream audioStream, @Nullable String sinkId) {
        play(audioStream, sinkId, null);
    public void play(@Nullable AudioStream audioStream, @Nullable String sinkId, @Nullable PercentType volume) {
        AudioSink sink = getSink(sinkId);
        if (sink != null) {
            Runnable restoreVolume = handleVolumeCommand(volume, sink);
            sink.processAndComplete(audioStream).exceptionally(exception -> {
                logger.warn("Error playing '{}': {}", audioStream, exception.getMessage(), exception);
            }).thenRun(restoreVolume);
            logger.warn("Failed playing audio stream '{}' as no audio sink was found.", audioStream);
    public void playFile(String fileName) throws AudioException {
        playFile(fileName, null, null);
    public void playFile(String fileName, @Nullable PercentType volume) throws AudioException {
        playFile(fileName, null, volume);
    public void playFile(String fileName, @Nullable String sinkId) throws AudioException {
        playFile(fileName, sinkId, null);
    public void playFile(String fileName, @Nullable String sinkId, @Nullable PercentType volume) throws AudioException {
        Objects.requireNonNull(fileName, "File cannot be played as fileName is null.");
        File file = Path.of(OpenHAB.getConfigFolder(), SOUND_DIR, fileName).toFile();
        FileAudioStream is = new FileAudioStream(file);
        play(is, sinkId, volume);
    public void stream(@Nullable String url) throws AudioException {
        stream(url, null);
    public void stream(@Nullable String url, @Nullable String sinkId) throws AudioException {
        AudioStream audioStream = url != null ? new URLAudioStream(url) : null;
    public void playMelody(String melody) {
        playMelody(melody, null);
    public void playMelody(String melody, @Nullable String sinkId) {
        playMelody(melody, sinkId, null);
    public void playMelody(String melody, @Nullable String sinkId, @Nullable PercentType volume) {
        if (sink == null) {
            logger.warn("Failed playing melody as no audio sink {} was found.", sinkId);
        var synthesizerFormat = AudioFormat.getBestMatch(ToneSynthesizer.getSupportedFormats(),
                sink.getSupportedFormats());
        if (synthesizerFormat == null) {
            logger.warn("Failed playing melody as sink {} does not support wav.", sinkId);
            var audioStream = new ToneSynthesizer(synthesizerFormat).getStream(ToneSynthesizer.parseMelody(melody));
            play(audioStream, sinkId, volume);
        } catch (IOException | ParseException e) {
            logger.warn("Failed playing melody: {}", e.getMessage());
    public void record(int seconds, String filename, @Nullable String sourceId) throws AudioException {
        var audioSource = sourceId != null ? getSource(sourceId) : getSource();
        if (audioSource == null) {
            throw new AudioException("Audio source '" + (sourceId != null ? sourceId : "default") + "' not available");
        var audioFormat = AudioFormat.getBestMatch(audioSource.getSupportedFormats(),
                Set.of(AudioFormat.PCM_SIGNED, AudioFormat.WAV));
            throw new AudioException("Unable to find valid audio format");
        javax.sound.sampled.AudioFormat jAudioFormat = new javax.sound.sampled.AudioFormat(
                Objects.requireNonNull(audioFormat.getFrequency()), Objects.requireNonNull(audioFormat.getBitDepth()),
                Objects.requireNonNull(audioFormat.getChannels()), true, false);
        int secondByteLength = ((int) jAudioFormat.getSampleRate() * jAudioFormat.getFrameSize());
        int targetByteLength = secondByteLength * seconds;
        ByteBuffer recordBuffer = ByteBuffer.allocate(targetByteLength);
        try (var audioStream = audioSource.getInputStream(audioFormat)) {
            if (audioFormat.isCompatible(AudioFormat.WAV)) {
                AudioWaveUtils.removeFMT(audioStream);
                    var bytes = audioStream.readNBytes(secondByteLength);
                    if (bytes.length == 0) {
                        logger.debug("End of input audio stream reached");
                    if (recordBuffer.position() + bytes.length > recordBuffer.limit()) {
                        logger.debug("Recording limit reached");
                    recordBuffer.put(bytes);
                    logger.warn("Reading audio data failed");
            logger.warn("IOException while reading audioStream: {}", e.getMessage());
        String recordFilename = filename.endsWith(".wav") ? filename : filename + ".wav";
        logger.info("Saving record file: {}", recordFilename);
        byte[] audioBytes = new byte[recordBuffer.position()];
        logger.info("Saving bytes: {}", audioBytes.length);
        recordBuffer.rewind();
        recordBuffer.get(audioBytes);
        File recordFile = new File(
                OpenHAB.getConfigFolder() + File.separator + SOUND_DIR + File.separator + recordFilename);
        try (FileOutputStream fileOutputStream = new FileOutputStream(recordFile)) {
            AudioSystem.write(
                    new AudioInputStream(new ByteArrayInputStream(audioBytes), jAudioFormat,
                            (long) Math.ceil(((double) audioBytes.length) / jAudioFormat.getFrameSize())), //
                    AudioFileFormat.Type.WAVE, //
                    fileOutputStream //
            fileOutputStream.flush();
            logger.warn("IOException while saving record file: {}", e.getMessage());
    public PercentType getVolume(@Nullable String sinkId) throws IOException {
            return sink.getVolume();
        return PercentType.ZERO;
    public void setVolume(PercentType volume, @Nullable String sinkId) throws IOException {
            sink.setVolume(volume);
    public @Nullable String getSourceId() {
        return defaultSource;
    public @Nullable AudioSource getSource() {
        AudioSource source = null;
        if (defaultSource != null) {
            source = audioSources.get(defaultSource);
            if (source == null) {
                logger.warn("Default AudioSource service '{}' not available!", defaultSource);
        } else if (!audioSources.isEmpty()) {
            source = audioSources.values().iterator().next();
            logger.debug("No AudioSource service available!");
    public Set<AudioSource> getAllSources() {
        return new HashSet<>(audioSources.values());
    public @Nullable AudioSource getSource(@Nullable String sourceId) {
        return (sourceId == null) ? getSource() : audioSources.get(sourceId);
    public Set<String> getSourceIds(String pattern) {
        String regex = pattern.replace("?", ".?").replace("*", ".*?");
        Set<String> matchedSources = new HashSet<>();
        for (String aSource : audioSources.keySet()) {
            if (aSource.matches(regex)) {
                matchedSources.add(aSource);
        return matchedSources;
    public @Nullable String getSinkId() {
        return defaultSink;
    public @Nullable AudioSink getSink() {
        AudioSink sink = null;
        if (defaultSink != null) {
            sink = audioSinks.get(defaultSink);
                logger.warn("Default AudioSink service '{}' not available!", defaultSink);
        } else if (!audioSinks.isEmpty()) {
            sink = audioSinks.values().iterator().next();
            logger.debug("No AudioSink service available!");
    public Set<AudioSink> getAllSinks() {
        return new HashSet<>(audioSinks.values());
    public @Nullable AudioSink getSink(@Nullable String sinkId) {
        return (sinkId == null) ? getSink() : audioSinks.get(sinkId);
    public Set<String> getSinkIds(String pattern) {
        Set<String> matchedSinkIds = new HashSet<>();
        for (String sinkId : audioSinks.keySet()) {
            if (sinkId.matches(regex)) {
                matchedSinkIds.add(sinkId);
        return matchedSinkIds;
    public @Nullable Collection<ParameterOption> getParameterOptions(URI uri, String param, @Nullable String context,
            @Nullable Locale locale) {
        if (CONFIG_URI.equals(uri.toString())) {
            final Locale safeLocale = locale != null ? locale : Locale.getDefault();
            if (CONFIG_DEFAULT_SOURCE.equals(param)) {
                return audioSources.values().stream().sorted(comparing(s -> s.getLabel(safeLocale)))
                        .map(s -> new ParameterOption(s.getId(), s.getLabel(safeLocale))).toList();
            } else if (CONFIG_DEFAULT_SINK.equals(param)) {
                return audioSinks.values().stream().sorted(comparing(s -> s.getLabel(safeLocale)))
    public Runnable handleVolumeCommand(@Nullable PercentType volume, AudioSink sink) {
        boolean volumeChanged = false;
        PercentType oldVolume = null;
        Runnable toRunWhenProcessFinished = () -> {
        if (volume == null) {
            return toRunWhenProcessFinished;
        // set notification sound volume
            // get current volume
            oldVolume = sink.getVolume();
        } catch (IOException | UnsupportedOperationException e) {
            logger.debug("An exception occurred while getting the volume of sink '{}' : {}", sink.getId(),
                    e.getMessage(), e);
        if (!volume.equals(oldVolume) || oldVolume == null) {
                volumeChanged = true;
                logger.debug("An exception occurred while setting the volume of sink '{}' : {}", sink.getId(),
        final PercentType oldVolumeFinal = oldVolume;
        // restore volume only if it was set before
        if (volumeChanged && oldVolumeFinal != null) {
            toRunWhenProcessFinished = () -> {
                    sink.setVolume(oldVolumeFinal);
    protected void addAudioSource(AudioSource audioSource) {
        this.audioSources.put(audioSource.getId(), audioSource);
    protected void removeAudioSource(AudioSource audioSource) {
        this.audioSources.remove(audioSource.getId());
    protected void addAudioSink(AudioSink audioSink) {
        this.audioSinks.put(audioSink.getId(), audioSink);
    protected void removeAudioSink(AudioSink audioSink) {
        this.audioSinks.remove(audioSink.getId());
