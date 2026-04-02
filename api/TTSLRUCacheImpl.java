import org.openhab.core.cache.lru.LRUMediaCache;
import org.openhab.core.cache.lru.LRUMediaCacheEntry;
import org.openhab.core.voice.TTSCache;
import org.openhab.core.voice.internal.VoiceManagerImpl;
 * This is a LRU cache (least recently used entry is evicted if the size
 * is exceeded)
 * Size is based on the size on disk (in bytes)
@Component(configurationPid = VoiceManagerImpl.CONFIGURATION_PID)
public class TTSLRUCacheImpl implements TTSCache {
    private final Logger logger = LoggerFactory.getLogger(TTSLRUCacheImpl.class);
    // a small default cache size for all the TTS services (in kB)
    private static final long DEFAULT_CACHE_SIZE_TTS = 10240;
    private static final int DEFAULT_MAX_TEXT_LENGTH_CACHE_TTS = 150;
    static final String CONFIG_CACHE_SIZE_TTS = "cacheSizeTTS";
    static final String CONFIG_ENABLE_CACHE_TTS = "enableCacheTTS";
    static final String CONFIG_MAX_TEXTLENGTH_CACHE_TTS = "maxTextLengthCacheTTS";
    static final String VOICE_TTS_CACHE_PID = "org.openhab.voice.tts";
    private @Nullable LRUMediaCache<AudioFormatInfo> lruMediaCache;
     * The size limit, in bytes. The size is not a hard one, because the final size of the
     * current request is not known and may or may not exceed the limit.
    protected long cacheSizeTTS = DEFAULT_CACHE_SIZE_TTS * 1024;
     * The maximum length of texts handled by the TTS cache (in character). If exceeded, will pass the text to
     * the TTS without storing it. (One can safely assume that long TTS are generated for report, probably not meant to
     * be repeated)
    private long maxTextLengthCacheTTS = DEFAULT_MAX_TEXT_LENGTH_CACHE_TTS;
    protected boolean enableCacheTTS = true;
    private StorageService storageService;
     * Constructs a cache system for TTS result.
    public TTSLRUCacheImpl(@Reference StorageService storageService, Map<String, Object> config) {
        this.storageService = storageService;
     * @param config Informations about the size of the cache in kB, and to enable the cache or not. The size is not a
     *            hard one, because the final size of the current request is not known and may exceed the limit.
     * @throws IOException when we cannot create the cache directory or if we have not enough space (*2 security margin)
        this.enableCacheTTS = ConfigParser.valueAsOrElse(config.get(CONFIG_ENABLE_CACHE_TTS), Boolean.class, true);
        this.cacheSizeTTS = ConfigParser.valueAsOrElse(config.get(CONFIG_CACHE_SIZE_TTS), Long.class,
                DEFAULT_CACHE_SIZE_TTS) * 1024;
        this.maxTextLengthCacheTTS = ConfigParser.valueAsOrElse(config.get(CONFIG_MAX_TEXTLENGTH_CACHE_TTS),
                Integer.class, DEFAULT_MAX_TEXT_LENGTH_CACHE_TTS);
        if (enableCacheTTS) {
            this.lruMediaCache = new LRUMediaCache<>(storageService, cacheSizeTTS, VOICE_TTS_CACHE_PID,
    public AudioStream get(CachedTTSService tts, String text, Voice voice, AudioFormat requestedFormat)
            throws TTSException {
        LRUMediaCache<AudioFormatInfo> lruMediaCacheLocal = lruMediaCache;
        if (!enableCacheTTS || lruMediaCacheLocal == null
                || (maxTextLengthCacheTTS > 0 && text.length() > maxTextLengthCacheTTS)) {
            return tts.synthesizeForCache(text, voice, requestedFormat);
        String key = tts.getClass().getSimpleName() + "_" + tts.getCacheKey(text, voice, requestedFormat);
        LRUMediaCacheEntry<AudioFormatInfo> fileAndMetadata;
            fileAndMetadata = lruMediaCacheLocal.get(key, () -> {
                    AudioStream audioInputStream = tts.synthesizeForCache(text, voice, requestedFormat);
                    return new LRUMediaCacheEntry<>(key, audioInputStream,
                            new AudioFormatInfo(audioInputStream.getFormat()));
        } catch (IllegalStateException ise) {
            if (ise.getCause() != null && ise.getCause() instanceof TTSException ttse) {
                throw ttse;
                throw ise;
            InputStream inputStream = fileAndMetadata.getInputStream();
            AudioFormatInfo metadata = fileAndMetadata.getMetadata();
            if (metadata == null) {
                throw new IllegalStateException("Cannot have an audio input stream without audio format information");
            if (inputStream instanceof InputStreamCacheWrapper inputStreamCacheWrapper) {
                // we are sure that the cache is used, and so we can use an AudioStream
                // implementation that use convenient methods for some client, like getClonedStream()
                // or mark /reset
                return new AudioStreamFromCache(inputStreamCacheWrapper, metadata, key);
                // the cache is not used, we can use the original response AudioStream
                return (AudioStream) fileAndMetadata.getInputStream();
            logger.debug("Cannot get audio from cache, fallback to TTS service", e);
