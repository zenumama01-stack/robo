import java.nio.file.FileStore;
 * Cache system for media files, and their metadata
 * is exceeded).
public class LRUMediaCache<V> {
    private final Logger logger = LoggerFactory.getLogger(LRUMediaCache.class);
    private static final String CACHE_FOLDER_NAME = "cache";
    final Map<String, @Nullable LRUMediaCacheEntry<V>> cachedResults;
     * Lock to handle concurrent access to the same entry
    private final Map<String, Lock> lockByEntry = new ConcurrentHashMap<>();
    protected final long maxCacheSize;
    private final Path cacheFolder;
     * Store for additional informations along the media file
    private Storage<V> storage;
    protected boolean cacheIsOK = true;
     * Constructs a cache system.
     * @param storageService Storage service to store metadata
     * @param maxCacheSize Limit size, in byte
     * @param pid A pid identifying the cache on disk
    public LRUMediaCache(@Reference StorageService storageService, long maxCacheSize, String pid,
            @Nullable ClassLoader clazzLoader) {
        this.storage = storageService.getStorage(pid, clazzLoader);
        this.cachedResults = Collections.synchronizedMap(new LinkedHashMap<>(20, .75f, true));
        this.cacheFolder = Path.of(OpenHAB.getUserDataFolder(), CACHE_FOLDER_NAME, pid);
        this.maxCacheSize = maxCacheSize;
        // creating directory if needed :
        logger.debug("Creating cache folder '{}'", cacheFolder);
            Files.createDirectories(cacheFolder);
            cleanCacheDirectory();
            loadAll();
            this.cacheIsOK = false;
            logger.warn("Cannot use cache directory", e);
        // check if we have enough space :
        if (getFreeSpace() < (maxCacheSize * 2)) {
            cacheIsOK = false;
            logger.warn("Not enough space for the cache");
        logger.debug("Using cache folder '{}'", cacheFolder);
    private void cleanCacheDirectory() throws IOException {
        try (Stream<Path> files = Files.list(cacheFolder)) {
            List<Path> filesInCacheFolder = new ArrayList<>(files.toList());
            // 1 delete empty files
            Iterator<Path> fileDeleterIterator = filesInCacheFolder.iterator();
            while (fileDeleterIterator.hasNext()) {
                Path path = fileDeleterIterator.next();
                File file = path.toFile();
                if (file.length() == 0) {
                    file.delete();
                    storage.remove(fileName);
                    fileDeleterIterator.remove();
            // 2 clean orphan (part of a pair (file + metadata) without a corresponding partner)
            // 2-a delete a file without its metadata
            for (Path path : filesInCacheFolder) {
                // check corresponding metadata in storage
                V metadata = storage.get(fileName);
            // 2-b delete metadata without corresponding file
            for (Entry<String, @Nullable V> entry : storage.stream().toList()) {
                Path correspondingFile = cacheFolder.resolve(entry.getKey());
                if (!Files.exists(correspondingFile)) {
                    storage.remove(entry.getKey());
            logger.warn("Cannot load the cache directory : {}", e.getMessage());
    private long getFreeSpace() {
            Path rootPath = Path.of(new URI("file:///"));
            Path dirPath = rootPath.resolve(cacheFolder.getParent());
            FileStore dirFileStore = Files.getFileStore(dirPath);
            return dirFileStore.getUsableSpace();
        } catch (URISyntaxException | IOException e) {
            logger.error("Cannot compute free disk space for the cache. Reason: {}", e.getMessage());
     * Returns a {@link LRUMediaCacheEntry} from the cache, or if not already in the cache :
     * resolve it, stores it, and returns it.
     * key A unique key identifying the result
     * supplier the data and metadata supplier. It is OK to launch a DataRetrievalException from this, as it will be
     * rethrown.
    public LRUMediaCacheEntry<V> get(String key, Supplier<LRUMediaCacheEntry<V>> supplier) {
        if (!cacheIsOK) {
        // we use a lock with fine granularity, by key, to not lock the entire cache
        // when resolving the supplier (which could be time consuming)
        Lock lockForCurrentEntry = lockByEntry.computeIfAbsent(key, k -> new ReentrantLock());
        if (lockForCurrentEntry == null) {
            logger.error("Cannot compute lock within cache system. Shouldn't happen");
        lockForCurrentEntry.lock();
            // try to get from cache
            LRUMediaCacheEntry<V> result = cachedResults.get(key);
            if (result != null && result.isFaulty()) { // if previously marked as faulty
                result.deleteFile();
                cachedResults.remove(key);
            if (result == null) { // it's a cache miss or a faulty result, we must (re)create it
                logger.debug("Cache miss {}", key);
                result = supplier.get();
                put(result);
            lockForCurrentEntry.unlock();
    protected void put(LRUMediaCacheEntry<V> result) {
        result.setCacheContext(cacheFolder, storage);
        cachedResults.put(result.getKey(), result);
        makeSpace();
     * Load all {@link LRUMediaCacheEntry} cached to the disk.
    private void loadAll() throws IOException {
        storage.stream().map(entry -> new LRUMediaCacheEntry<V>(entry.getKey())).forEach(this::put);
     * Check if the cache is not already full and make space if needed.
     * We don't use the removeEldestEntry test method from the linkedHashMap because it can only remove one element.
    protected void makeSpace() {
            Iterator<@Nullable LRUMediaCacheEntry<V>> iterator = cachedResults.values().iterator();
            Long currentCacheSize = cachedResults.values().stream()
                    .map(result -> result == null ? 0 : result.getCurrentSize()).reduce(0L, (Long::sum));
            int attemptToDelete = 0;
            while (currentCacheSize > maxCacheSize && cachedResults.size() > 1 && attemptToDelete < 10) {
                attemptToDelete++;
                LRUMediaCacheEntry<V> oldestEntry = iterator.next();
                if (oldestEntry != null) {
                    oldestEntry.deleteFile();
                    currentCacheSize -= oldestEntry.getCurrentSize();
                    lockByEntry.remove(oldestEntry.getKey());
