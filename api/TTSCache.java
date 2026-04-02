 * Cache system to avoid requesting {@link TTSService} for the same utterances.
public interface TTSCache {
     * Returns an {@link AudioStream} containing the TTS results. Note, one
     * can only request a supported {@code Voice} and {@link AudioStream} or
     * an exception is thrown.
     * The AudioStream is requested from the cache, or, if not found, from
     * the underlying TTS service
     * @param tts the TTS service
     * @param text The text to convert to speech
     * @param voice The voice to use for speech
     * @param requestedFormat The audio format to return the results in
     * @return AudioStream containing the TTS results
     * @throws TTSException If {@code voice} and/or {@code requestedFormat}
     *             are not supported or another error occurs while creating an
     *             {@link AudioStream}
    AudioStream get(CachedTTSService tts, String text, Voice voice, AudioFormat requestedFormat) throws TTSException;
