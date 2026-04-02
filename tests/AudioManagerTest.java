import static org.mockito.ArgumentMatchers.any;
import java.util.function.BiFunction;
 * @author Henning Treu - Convert to plain java tests
public class AudioManagerTest {
    private @NonNullByDefault({}) AudioSource audioSource;
    public void setup() throws IOException {
        audioSource = mock(AudioSource.class);
        when(audioSource.getId()).thenReturn("audioSourceId");
        when(audioSource.getLabel(any(Locale.class))).thenReturn("audioSourceLabel");
    public void tearDown() throws IOException {
    public void audioManagerPlaysByteArrayAudioStream() throws AudioException {
        ByteArrayAudioStream audioStream = getByteArrayAudioStream(AudioFormat.CONTAINER_WAVE, AudioFormat.CODEC_MP3);
        audioManager.play(audioStream, audioSink.getId());
        AudioFormat expectedAudioFormat = audioStream.getFormat();
        assertThat(audioSink.audioFormat.isCompatible(expectedAudioFormat), is(true));
    public void nullStreamsAreProcessed() {
        audioManager.play(null, audioSink.getId());
        assertThat(audioSink.isStreamProcessed, is(true));
        assertThat(audioSink.isStreamStopped, is(true));
    public void audioManagerPlaysStreamFromWavAudioFiles() throws AudioException {
    public void audioManagerPlaysStreamFromMp3AudioFiles() throws AudioException {
        AudioStream audioStream = new FileAudioStream(new File(fileHandler.mp3FilePath()));
    public void audioManagerPlaysWavAudioFiles() throws AudioException, IOException {
        audioManager.playFile(fileHandler.wavFileName(), audioSink.getId());
    public void audioManagerPlaysMp3AudioFiles() throws AudioException, IOException {
        audioManager.playFile(fileHandler.mp3FileName(), audioSink.getId());
    public void fileIsNotProcessedIfThereIsNoRegisteredSink() throws AudioException {
        File file = new File(fileHandler.mp3FilePath());
        audioManager.playFile(file.getName(), audioSink.getId());
        assertThat(audioSink.isStreamProcessed, is(false));
    public void audioManagerHandlesUnsupportedAudioFormatException() throws AudioException {
        audioSink.isUnsupportedAudioFormatExceptionExpected = true;
        } catch (UnsupportedAudioStreamException e) {
            fail("An exception " + e + " was thrown, while trying to process a stream");
    public void audioManagerHandlesUnsupportedAudioStreamException() throws AudioException {
        audioSink.isUnsupportedAudioStreamExceptionExpected = true;
    public void audioManagerSetsTheVolumeOfASink() throws IOException {
        PercentType initialVolume = new PercentType(67);
        PercentType sinkMockVolume = getSinkMockVolume(initialVolume);
        assertThat(sinkMockVolume, is(initialVolume));
    public void theVolumeOfANullSinkIsZero() throws IOException {
        assertThat(audioManager.getVolume(null), is(PercentType.ZERO));
    public void audioManagerSetsTheVolumeOfNotRegisteredSinkToZero() throws IOException {
        assertThat(sinkMockVolume, is(PercentType.ZERO));
    public void sourceIsRegistered() {
        assertRegisteredSource(false);
    public void defaultSourceIsRegistered() {
        assertRegisteredSource(true);
    public void sinkIsRegistered() {
        assertRegisteredSink(false);
    public void defaultSinkIsRegistered() {
        assertRegisteredSink(true);
    public void sinkIsAddedInParameterOptions() {
        assertAddedParameterOption(AudioManagerImpl.CONFIG_DEFAULT_SINK, Locale.getDefault());
    public void sourceIsAddedInParameterOptions() {
        assertAddedParameterOption(AudioManagerImpl.CONFIG_DEFAULT_SOURCE, Locale.getDefault());
    public void inCaseOfWrongUriNoParameterOptionsAreAdded() {
        Collection<ParameterOption> parameterOptions = audioManager.getParameterOptions(URI.create("wrong.uri"),
                AudioManagerImpl.CONFIG_DEFAULT_SINK, null, Locale.US);
        assertThat("The parameter options were not as expected", parameterOptions, is(nullValue()));
    private void assertRegisteredSource(boolean isSourceDefault) {
        if (isSourceDefault) {
            audioManager.modified(Map.of(AudioManagerImpl.CONFIG_DEFAULT_SOURCE, audioSource.getId()));
            // just to make sure there is no default source
            audioManager.modified(Map.of());
        assertThat(String.format("The source %s was not registered", audioSource.getId()), audioManager.getSource(),
                is(audioSource));
        assertThat(String.format("The source %s was not added to the set of sources", audioSource.getId()),
                audioManager.getAllSources().contains(audioSource), is(true));
                audioManager.getSourceIds(audioSource.getId()).contains(audioSource.getId()), is(true));
    private void assertRegisteredSink(boolean isSinkDefault) {
        if (isSinkDefault) {
            audioManager.modified(Map.of(AudioManagerImpl.CONFIG_DEFAULT_SINK, audioSink.getId()));
            // just to make sure there is no default sink
        assertThat(String.format("The sink %s was not registered", audioSink.getId()), audioManager.getSink(),
                is(audioSink));
        assertThat(String.format("The sink %s was not added to the set of sinks", audioSink.getId()),
                audioManager.getAllSinks().contains(audioSink), is(true));
                audioManager.getSinkIds(audioSink.getId()).contains(audioSink.getId()), is(true));
    private PercentType getSinkMockVolume(PercentType initialVolume) throws IOException {
        audioManager.setVolume(initialVolume, audioSink.getId());
        String sinkMockId = audioSink.getId();
        return audioManager.getVolume(sinkMockId);
     * @param param either default source or default sink
    private void assertAddedParameterOption(String param, Locale locale) {
        String id = "";
        String label = "";
        switch (param) {
            case AudioManagerImpl.CONFIG_DEFAULT_SINK:
                id = audioSink.getId();
                label = Objects.requireNonNull(audioSink.getLabel(locale));
            case AudioManagerImpl.CONFIG_DEFAULT_SOURCE:
                id = audioSource.getId();
                label = Objects.requireNonNull(audioSource.getLabel(locale));
                fail("The parameter must be either default sink or default source");
        Collection<ParameterOption> parameterOptions = audioManager
                .getParameterOptions(URI.create(AudioManagerImpl.CONFIG_URI), param, null, locale);
        BiFunction<String, String, Boolean> isParameterOptionAdded = (v, l) -> parameterOptions.stream()
                .anyMatch(po -> po.getValue().equals(v) && po.getLabel().equals(l));
        assertThat(param + " was not added to the parameter options", isParameterOptionAdded.apply(id, label),
                is(true));
