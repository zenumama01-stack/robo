package org.openhab.core.audio.internal.fake;
 * An {@link AudioSink} fake used for the tests.
 * @author Christoph Weitkamp - Added examples for getSupportedFormats() and getSupportedStreams()
public class AudioSinkFake implements AudioSink {
    public @Nullable AudioStream audioStream;
    public @Nullable AudioFormat audioFormat;
    public boolean isStreamProcessed;
    public boolean isStreamStopped;
    public @Nullable PercentType volume;
    public boolean isIOExceptionExpected;
    public boolean isUnsupportedAudioFormatExceptionExpected;
    public boolean isUnsupportedAudioStreamExceptionExpected;
    private static final Set<Class<? extends AudioStream>> SUPPORTED_AUDIO_STREAMS = Set.of(ByteArrayAudioStream.class,
            URLAudioStream.class);
        return "testSinkId";
        return "testSinkLabel";
        if (isUnsupportedAudioFormatExceptionExpected) {
            throw new UnsupportedAudioFormatException("Expected audio format exception", null);
        if (isUnsupportedAudioStreamExceptionExpected) {
            throw new UnsupportedAudioStreamException("Expected audio stream exception", null);
            audioFormat = audioStream.getFormat();
            isStreamStopped = true;
        isStreamProcessed = true;
    public @Nullable AudioFormat getAudioFormat() {
        PercentType localVolume = volume;
        if (localVolume == null || isIOExceptionExpected) {
            throw new IOException();
        return localVolume;
    public void setVolume(PercentType volume) throws IOException {
        if (isIOExceptionExpected) {
        this.volume = volume;
