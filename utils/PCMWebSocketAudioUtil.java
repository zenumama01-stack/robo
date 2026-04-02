 * Audio utils.
public final class PCMWebSocketAudioUtil {
    private PCMWebSocketAudioUtil() {
     * Ensure right PCM format by converting if needed (sample rate, channel)
     * @param sampleRate Stream sample rate
     * @param stream PCM input stream
     * @return A PCM normalized stream at the desired format
    public static AudioInputStream getPCMStreamNormalized(InputStream stream, int sampleRate, int bitDepth,
            int channels, int targetSampleRate, int targetBitDepth, int targetChannels) {
        javax.sound.sampled.AudioFormat jFormat = new javax.sound.sampled.AudioFormat( //
                (float) sampleRate, //
                bitDepth, //
                channels, //
                true, //
                false //
        javax.sound.sampled.AudioFormat fixedJFormat = new javax.sound.sampled.AudioFormat( //
                (float) targetSampleRate, //
                targetBitDepth, //
                targetChannels, //
        return AudioSystem.getAudioInputStream(fixedJFormat, new AudioInputStream(stream, jFormat, -1));
