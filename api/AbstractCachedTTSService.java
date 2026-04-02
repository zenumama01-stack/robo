package org.openhab.core.voice;
import org.openhab.core.voice.internal.cache.CachedTTSService;
 * Implements cache functionality for the TTS service extending this class.
public abstract class AbstractCachedTTSService implements CachedTTSService {
    private final TTSCache ttsCache;
    protected AbstractCachedTTSService(final @Reference TTSCache ttsCache) {
        this.ttsCache = ttsCache;
    public AudioStream synthesize(String text, Voice voice, AudioFormat requestedFormat) throws TTSException {
        return ttsCache.get(this, text, voice, requestedFormat);
    public String getCacheKey(String text, Voice voice, AudioFormat requestedFormat) {
        MessageDigest md;
            md = MessageDigest.getInstance("MD5");
            return "nomd5algorithm";
        byte[] binaryKey = ((text + voice.getUID() + requestedFormat.toString()).getBytes());
        return String.format("%032x", new BigInteger(1, md.digest(binaryKey)));
