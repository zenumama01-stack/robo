import java.util.Iterator;
import java.util.function.Supplier;
import org.openhab.core.automation.module.script.rulesupport.shared.ValueCache;
 * The {@link CacheScriptExtension} extends scripts to use a cache shared between rules or subsequent runs of the same
 * rule
public class CacheScriptExtension implements ScriptExtensionProvider {
    static final String PRESET_NAME = "cache";
    static final String SHARED_CACHE_NAME = "sharedCache";
    static final String PRIVATE_CACHE_NAME = "privateCache";
    private final Logger logger = LoggerFactory.getLogger(CacheScriptExtension.class);
    private final Lock cacheLock = new ReentrantLock();
    private final Map<String, Object> sharedCache = new HashMap<>();
    private final Map<String, Set<String>> sharedCacheKeyAccessors = new ConcurrentHashMap<>();
    private final Map<String, ValueCacheImpl> privateCaches = new ConcurrentHashMap<>();
    public CacheScriptExtension() {
        return Set.of(PRIVATE_CACHE_NAME, SHARED_CACHE_NAME);
        if (SHARED_CACHE_NAME.equals(type)) {
            return new TrackingValueCacheImpl(scriptIdentifier);
        } else if (PRIVATE_CACHE_NAME.equals(type)) {
            return privateCaches.computeIfAbsent(scriptIdentifier, ValueCacheImpl::new);
            Object privateCache = Objects
                    .requireNonNull(privateCaches.computeIfAbsent(scriptIdentifier, ValueCacheImpl::new));
            return Map.of(SHARED_CACHE_NAME, new TrackingValueCacheImpl(scriptIdentifier), PRIVATE_CACHE_NAME,
                    privateCache);
        cacheLock.lock();
            // remove the scriptIdentifier from cache-key access list
            sharedCacheKeyAccessors.values().forEach(cacheKey -> cacheKey.remove(scriptIdentifier));
            // remove the key from access list and cache if no accessor left
            Iterator<Map.Entry<String, Set<String>>> it = sharedCacheKeyAccessors.entrySet().iterator();
            while (it.hasNext()) {
                Map.Entry<String, Set<String>> element = it.next();
                if (element.getValue().isEmpty()) {
                    // accessor list is empty
                    it.remove();
                    // remove from cache and cancel ScheduledFutures or Timer tasks
                    asyncCancelJob(sharedCache.remove(element.getKey()));
            cacheLock.unlock();
        // remove private cache
        ValueCacheImpl privateCache = privateCaches.remove(scriptIdentifier);
        if (privateCache != null) {
            // cancel ScheduledFutures or Timer tasks
            privateCache.values().forEach(this::asyncCancelJob);
     * Check if object is {@link ScheduledFuture}, {@link java.util.Timer} or
     * {@link org.openhab.core.automation.module.script.action.Timer} and schedule cancellation of those jobs
     * @param o the {@link Object} to check
    private void asyncCancelJob(@Nullable Object o) {
        Runnable cancelJob = null;
        if (o instanceof ScheduledFuture future) {
            cancelJob = () -> future.cancel(true);
        } else if (o instanceof java.util.Timer timer) {
            cancelJob = () -> timer.cancel();
        } else if (o instanceof org.openhab.core.automation.module.script.action.Timer timer) {
        if (cancelJob != null) {
            // not using execute so ensure this operates in another thread and we don't block here
            scheduler.schedule(cancelJob, 0, TimeUnit.SECONDS);
    private static class ValueCacheImpl implements ValueCache {
        private final Map<String, Object> cache = new HashMap<>();
        public ValueCacheImpl(String scriptIdentifier) {
        public @Nullable Object put(String key, Object value) {
            return cache.put(key, value);
        public @Nullable Object remove(String key) {
            return cache.remove(key);
        public @Nullable Object get(String key) {
            return cache.get(key);
        public Object get(String key, Supplier<Object> supplier) {
            return Objects.requireNonNull(cache.computeIfAbsent(key, k -> supplier.get()));
        public Object compute(String key, BiFunction<String, @Nullable Object, @Nullable Object> remappingFunction) {
            return cache.compute(key, (k, v) -> remappingFunction.apply(k, v));
        private Collection<Object> values() {
            return cache.values();
    private class TrackingValueCacheImpl implements ValueCache {
        private final String scriptIdentifier;
        public TrackingValueCacheImpl(String scriptIdentifier) {
            this.scriptIdentifier = scriptIdentifier;
                rememberAccessToKey(key);
                Object oldValue = sharedCache.put(key, value);
                logger.trace("PUT to cache from '{}': '{}' -> '{}' (was: '{}')", scriptIdentifier, key, value,
                        oldValue);
                sharedCacheKeyAccessors.remove(key);
                Object oldValue = sharedCache.remove(key);
                logger.trace("REMOVE from cache from '{}': '{}' -> '{}'", scriptIdentifier, key, oldValue);
                Object value = sharedCache.get(key);
                logger.trace("GET to cache from '{}': '{}' -> '{}'", scriptIdentifier, key, value);
                Object value = Objects.requireNonNull(sharedCache.computeIfAbsent(key, k -> supplier.get()));
                logger.trace("GET with supplier to cache from '{}': '{}' -> '{}'", scriptIdentifier, key, value);
                Object value = sharedCache.compute(key, (k, v) -> remappingFunction.apply(k, v));
                logger.trace("COMPUTE to cache from '{}': '{}' -> '{}'", scriptIdentifier, key, value);
        private void rememberAccessToKey(String key) {
            Objects.requireNonNull(sharedCacheKeyAccessors.computeIfAbsent(key, k -> new HashSet<>()))
                    .add(scriptIdentifier);
