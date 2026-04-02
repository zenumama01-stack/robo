 * OSGi test for {@link AudioManagerImpl}
 * @author Henning Treu - extract servlet tests
public class AudioManagerServletTest extends AbstractAudioServletTest {
    public void setup() {
    public void audioManagerProcessesMultitimeStreams() throws Exception {
        int streamTimeout = 10;
        assertServedStream(streamTimeout);
    public void audioManagerProcessesOneTimeStream() throws Exception {
        assertServedStream(null);
    public void audioManagerDoesNotProcessStreamsIfThereIsNoRegisteredSink() throws Exception {
    private void assertServedStream(@Nullable Integer timeInterval) throws Exception {
        AudioStream audioStream = getByteArrayAudioStream(AudioFormat.CONTAINER_WAVE, AudioFormat.CODEC_PCM_SIGNED);
        String url = serveStream(audioStream, timeInterval);
        audioManager.stream(url, audioSink.getId());
        if (audioManager.getSink() == audioSink) {
            assertThat("The streamed url was not as expected", ((URLAudioStream) audioSink.audioStream).getURL(),
                    is(url));
            assertThat(String.format("The sink %s received an unexpected stream", audioSink.getId()),
                    audioSink.audioStream, is(nullValue()));
    private ByteArrayAudioStream getByteArrayAudioStream(String container, String codec) {
        byte[] testByteArray = new byte[] { 0, 1, 2 };
        return new ByteArrayAudioStream(testByteArray, audioFormat);
