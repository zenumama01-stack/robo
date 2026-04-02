 * OSGi test for {@link AudioFormat}
public class AudioFormatTest {
    private final String testContainer = AudioFormat.CONTAINER_WAVE;
    private final String testCodec = AudioFormat.CODEC_PCM_SIGNED;
    private final boolean testBigEndian = true;
    private final Integer testBitDepth = Integer.valueOf(16);
    private final Integer testBitRate = Integer.valueOf(1000);
    private final Long testFrequency = Long.valueOf(1024);
    public void thereIsNoBestMatchForAnAudioFormatIfOneOfTheFieldsIsNull() {
        Set<AudioFormat> inputs = new HashSet<>();
        Set<AudioFormat> outputs = new HashSet<>();
        AudioFormat nullContainerAudioFormat = new AudioFormat(null, testCodec, testBigEndian, testBitDepth,
                testBitRate, testFrequency);
        AudioFormat nullCodecAudioFormat = new AudioFormat(testContainer, null, testBigEndian, testBitDepth,
        AudioFormat nullBitDepthAudioFormat = new AudioFormat(testContainer, testCodec, testBigEndian, null,
        AudioFormat nullBitRateAudioFormat = new AudioFormat(testContainer, testCodec, testBigEndian, testBitDepth,
                null, testFrequency);
        AudioFormat nullFrequencyAudioFormat = new AudioFormat(testContainer, testCodec, testBigEndian, testBitDepth,
                testBitRate, null);
        AudioFormat outputAudioFormat = new AudioFormat(testContainer, testCodec, testBigEndian, testBitDepth,
        inputs.add(nullContainerAudioFormat);
        inputs.add(nullCodecAudioFormat);
        inputs.add(nullBitDepthAudioFormat);
        inputs.add(nullBitRateAudioFormat);
        inputs.add(nullFrequencyAudioFormat);
        outputs.add(outputAudioFormat);
        AudioFormat bestMatch = AudioFormat.getBestMatch(inputs, outputs);
        assertThat("The best match for the audio format was not as expected", bestMatch, is(nullValue()));
    public void waveContainerIsPreferredWhenDeterminingABestMatch() {
        Set<AudioFormat> audioFormats = new HashSet<>();
        AudioFormat waveContainerAudioFormat = new AudioFormat(testContainer, testCodec, true, testBitDepth,
        AudioFormat oggContainerAudioFormat = new AudioFormat(AudioFormat.CONTAINER_OGG, testCodec, true, testBitDepth,
        audioFormats.add(waveContainerAudioFormat);
        audioFormats.add(oggContainerAudioFormat);
        AudioFormat preferredFormat = AudioFormat.getPreferredFormat(audioFormats);
        assertThat("The best match for the audio format was not as expected", preferredFormat,
                is(waveContainerAudioFormat));
    public void theCompatibleWaveContainerIsTheBestMatch() {
        AudioFormat alawAudioFormat = new AudioFormat(testContainer, AudioFormat.CODEC_PCM_ALAW, true, testBitDepth,
        AudioFormat ulawAudioFormat = new AudioFormat(testContainer, AudioFormat.CODEC_PCM_ULAW, true, testBitDepth,
        inputs.add(alawAudioFormat);
        inputs.add(ulawAudioFormat);
        outputs.add(alawAudioFormat);
        assertThat("The best match for the audio format was not as expected", bestMatch, is(alawAudioFormat));
    public void thereIsNoBestMatchIfNoWaveContainer() {
        AudioFormat nonContainerAudioFormat = new AudioFormat(AudioFormat.CONTAINER_NONE, AudioFormat.CODEC_MP3, true,
                testBitDepth, testBitRate, testFrequency);
        inputs.add(oggContainerAudioFormat);
        inputs.add(nonContainerAudioFormat);
        outputs.add(oggContainerAudioFormat);
        outputs.add(nonContainerAudioFormat);
