public interface CachedTTSService extends TTSService {
     * Construct a uniquely identifying string for the request. Could be overridden by the TTS service if
     * it uses some unique external parameter and wants to identify variability in the cache.
     * @return A likely unique key identifying the combination of parameters and/or internal state,
     *         as a string suitable to be part of a filename. This will be used in the cache system to store the result.
    String getCacheKey(String text, Voice voice, AudioFormat requestedFormat);
     * The result will be cached if the TTSCacheService is activated.
    AudioStream synthesizeForCache(String text, Voice voice, AudioFormat requestedFormat) throws TTSException;
