import static org.mockito.ArgumentMatchers.anyInt;
import org.eclipse.jetty.http.HttpStatus;
 * Test cases for {@link AudioServlet}
public class AudioServletTest extends AbstractAudioServletTest {
    private static final String MEDIA_TYPE_AUDIO_WAV = "audio/wav";
    private static final String MEDIA_TYPE_AUDIO_OGG = "audio/ogg";
    private static final String MEDIA_TYPE_AUDIO_MPEG = "audio/mpeg";
    public void audioServletProcessesByteArrayStream() throws Exception {
        AudioStream audioStream = getByteArrayAudioStream(testByteArray, AudioFormat.CONTAINER_NONE,
                AudioFormat.CODEC_MP3);
        ContentResponse response = getHttpResponse(audioStream);
        assertThat("The response status was not as expected", response.getStatus(), is(HttpStatus.OK_200));
        assertThat("The response content was not as expected", response.getContent(), is(testByteArray));
        assertThat("The response media type was not as expected", response.getMediaType(),
                is(equalTo(MEDIA_TYPE_AUDIO_MPEG)));
    public void audioServletProcessesStreamFromWavFileWithoutAcceptHeader() throws Exception {
        try (BundledSoundFileHandler fileHandler = new BundledSoundFileHandler()) {
                    is(MEDIA_TYPE_AUDIO_WAV));
    public void audioServletProcessesStreamFromWavFileWithAcceptHeader() throws Exception {
            ContentResponse response = getHttpResponseWithAccept(audioStream, "audio/invalid, audio/x-wav");
            assertThat("The response media type was not as expected", response.getMediaType(), is("audio/x-wav"));
    public void audioServletProcessesStreamFromOggContainer() throws Exception {
        AudioStream audioStream = getByteArrayAudioStream(testByteArray, AudioFormat.CONTAINER_OGG,
        assertThat("The response media type was not as expected", response.getMediaType(), is(MEDIA_TYPE_AUDIO_OGG));
    public void mimeTypeIsNullWhenNoContainerAndTheAudioFormatIsNotMp3() throws Exception {
        assertThat("The response media type was not as expected", response.getMediaType(), is(nullValue()));
    public void onlyOneRequestToOneTimeStreamsCanBeMade() throws Exception {
        ContentResponse response = getHttpRequest(url).send();
        assertThat("The response media type was not as expected", response.getMediaType(), is(MEDIA_TYPE_AUDIO_MPEG));
        response = getHttpRequest(url).send();
        assertThat("The response status was not as expected", response.getStatus(), is(HttpStatus.NOT_FOUND_404));
    public void requestToMultitimeStreamCannotBeDoneAfterTheTimeoutOfTheStreamHasExpired() throws Exception {
        final int streamTimeout = 3;
        final long beg = System.currentTimeMillis();
        String url = serveStream(audioStream, streamTimeout);
        final long end = System.currentTimeMillis();
        if (response.getStatus() == HttpStatus.NOT_FOUND_404) {
            assertThat("Response status 404 is only allowed if streamTimeout is exceeded.",
                    TimeUnit.MILLISECONDS.toSeconds(end - beg), greaterThan((long) streamTimeout));
                    is(MEDIA_TYPE_AUDIO_MPEG));
            assertThat("The audio stream was not added to the multitime streams", audioServlet.getServedStreams()
                    .values().stream().map(StreamServed::audioStream).toList().contains(audioStream), is(true));
        waitForAssert(() -> {
                getHttpRequest(url).send();
                throw new IllegalStateException(e);
            assertThat("The audio stream was not removed from multitime streams", audioServlet.getServedStreams()
                    .values().stream().map(StreamServed::audioStream).toList().contains(audioStream), is(false));
    public void oneTimeStreamIsRecreatedAsAClonable() throws Exception {
        AudioStream audioStream = mock(AudioStream.class);
        AudioFormat audioFormat = mock(AudioFormat.class);
        when(audioStream.getFormat()).thenReturn(audioFormat);
        when(audioFormat.getCodec()).thenReturn(AudioFormat.CODEC_MP3);
        when(audioStream.readNBytes(anyInt())).thenReturn(testByteArray);
        String url = serveStream(audioStream, 10);
        String uuid = url.substring(url.lastIndexOf("/") + 1);
        StreamServed servedStream = audioServlet.getServedStreams().get(uuid);
        // does not contain directly the stream because it is now a new stream wrapper
        assertThat(servedStream.audioStream(), not(audioStream));
        // it is now a ByteArrayAudioStream wrapper :
        assertThat(servedStream.audioStream(), instanceOf(ByteArrayAudioStream.class));
        verify(audioStream).close();
    public void oneTimeStreamIsClosedAndRemovedAfterServed() throws Exception {
        when(audioStream.readNBytes(anyInt())).thenReturn(new byte[] { 1, 2, 3 });
        assertThat(audioServlet.getServedStreams().values().stream().map(StreamServed::audioStream).toList(),
                contains(audioStream));
                not(contains(audioStream)));
    public void multiTimeStreamIsClosedAfterExpired() throws Exception {
        AtomicInteger cloneCounter = new AtomicInteger();
        ByteArrayAudioStream audioStream = mock(ByteArrayAudioStream.class);
        AudioStream clonedStream = mock(AudioStream.class);
        when(audioStream.getClonedStream()).thenAnswer(answer -> {
            cloneCounter.getAndIncrement();
            return clonedStream;
        when(clonedStream.readNBytes(anyInt())).thenReturn(new byte[] { 1, 2, 3 });
        String url = serveStream(audioStream, 2);
                ContentResponse resp = getHttpRequest(url).send();
                assertThat(resp.getStatus(), is(404));
        verify(clonedStream, times(cloneCounter.get())).close();
    public void streamsAreClosedOnDeactivate() throws Exception {
        AudioStream oneTimeStream = mock(AudioStream.class);
        ByteArrayAudioStream multiTimeStream = mock(ByteArrayAudioStream.class);
        serveStream(oneTimeStream);
        serveStream(multiTimeStream, 10);
        audioServlet.deactivate();
        verify(oneTimeStream).close();
        verify(multiTimeStream).close();
