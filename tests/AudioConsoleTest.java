import static org.hamcrest.CoreMatchers.*;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import org.openhab.core.audio.internal.fake.AudioSinkFake;
import org.openhab.core.audio.internal.utils.BundledSoundFileHandler;
 * OSGi test for {@link AudioConsoleCommandExtension}
 * @author Petar Valchev - Initial contribution
 * @author Wouter Born - Migrate tests from Groovy to Java
public class AudioConsoleTest extends AbstractAudioServletTest {
    private @NonNullByDefault({}) AudioConsoleCommandExtension audioConsoleCommandExtension;
    private @NonNullByDefault({}) AudioManagerImpl audioManager;
    private @NonNullByDefault({}) AudioSinkFake audioSink;
    private @NonNullByDefault({}) String consoleOutput;
    private @NonNullByDefault({}) BundledSoundFileHandler fileHandler;
    private final byte[] testByteArray = new byte[] { 0, 1, 2 };
    private final Console consoleMock = new Console() {
        public void println(String s) {
            consoleOutput = s;
        public void printUsage(String s) {
        public void print(String s) {
    private final int testTimeout = 5;
    public void setUp() throws IOException {
        fileHandler = new BundledSoundFileHandler();
        audioSink = new AudioSinkFake();
        audioManager = new AudioManagerImpl();
        audioManager.addAudioSink(audioSink);
        LocaleProvider localeProvider = mock(LocaleProvider.class);
        when(localeProvider.getLocale()).thenReturn(Locale.getDefault());
        audioConsoleCommandExtension = new AudioConsoleCommandExtension(audioManager, localeProvider);
    public void tearDown() {
        fileHandler.close();
    public void testUsages() {
        assertThat("Could not get AudioConsoleCommandExtension's usages", audioConsoleCommandExtension.getUsages(),
                is(notNullValue()));
    public void audioConsolePlaysFile() throws AudioException, IOException {
        AudioStream audioStream = new FileAudioStream(new File(fileHandler.wavFilePath()));
        String[] args = { AudioConsoleCommandExtension.SUBCMD_PLAY, fileHandler.wavFileName() };
        audioConsoleCommandExtension.execute(args, consoleMock);
        assertThat(audioSink.audioFormat.isCompatible(audioStream.getFormat()), is(true));
    public void audioConsolePlaysFileForASpecifiedSink() throws AudioException, IOException {
        String[] args = { AudioConsoleCommandExtension.SUBCMD_PLAY, audioSink.getId(), fileHandler.wavFileName() };
    public void audioConsolePlaysFileForASpecifiedSinkWithASpecifiedVolume() throws AudioException, IOException {
        String[] args = { AudioConsoleCommandExtension.SUBCMD_PLAY, audioSink.getId(), fileHandler.wavFileName(),
                "25" };
    public void audioConsolePlaysFileForASpecifiedSinkWithAnInvalidVolume() {
                "invalid" };
        waitForAssert(() -> assertThat("The given volume was invalid", consoleOutput,
                is("Specify volume as percentage between 0 and 100")));
    public void audioConsolePlaysStream() throws Exception {
        AudioStream audioStream = getByteArrayAudioStream(testByteArray, AudioFormat.CONTAINER_WAVE,
                AudioFormat.CODEC_PCM_SIGNED);
        String url = serveStream(audioStream, testTimeout);
        String[] args = { AudioConsoleCommandExtension.SUBCMD_STREAM, url };
        assertThat("The streamed URL was not as expected", ((URLAudioStream) audioSink.audioStream).getURL(), is(url));
    public void audioConsolePlaysStreamForASpecifiedSink() throws Exception {
        String[] args = { AudioConsoleCommandExtension.SUBCMD_STREAM, audioSink.getId(), url };
    public void audioConsoleListsSinks() {
        String[] args = { AudioConsoleCommandExtension.SUBCMD_SINKS };
        waitForAssert(() -> assertThat("The listed sink was not as expected", consoleOutput,
                is(String.format("* %s (%s)", audioSink.getLabel(Locale.getDefault()), audioSink.getId()))));
    public void audioConsoleListsSources() {
        AudioSource audioSource = mock(AudioSource.class);
        when(audioSource.getId()).thenReturn("sourceId");
        audioManager.addAudioSource(audioSource);
        String[] args = { AudioConsoleCommandExtension.SUBCMD_SOURCES };
        waitForAssert(() -> assertThat("The listed source was not as expected", consoleOutput,
                is(String.format("* %s (%s)", audioSource.getLabel(Locale.getDefault()), audioSource.getId()))));
