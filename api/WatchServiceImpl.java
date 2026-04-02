import java.nio.file.NoSuchFileException;
import io.methvin.watcher.DirectoryChangeEvent;
import io.methvin.watcher.DirectoryChangeListener;
import io.methvin.watcher.DirectoryWatcher;
import io.methvin.watcher.hashing.FileHash;
 * The {@link WatchServiceImpl} is the implementation of the {@link WatchService}
@Component(immediate = true, service = WatchService.class, configurationPid = WatchService.SERVICE_PID, configurationPolicy = ConfigurationPolicy.REQUIRE)
public class WatchServiceImpl implements WatchService, DirectoryChangeListener {
    public static final int PROCESSING_TIME = 1000;
    public @interface WatchServiceConfiguration {
        String name() default "";
        String path() default "";
    private final Logger logger = LoggerFactory.getLogger(WatchServiceImpl.class);
    private final List<Listener> dirPathListeners = new CopyOnWriteArrayList<>();
    private final List<Listener> subDirPathListeners = new CopyOnWriteArrayList<>();
    private final Map<Path, FileHash> hashCache = new ConcurrentHashMap<>();
    private final ExecutorService executor;
    private @Nullable Path basePath;
    private @Nullable DirectoryWatcher dirWatcher;
    private @Nullable ServiceRegistration<WatchService> reg;
    private final Map<Path, ScheduledFuture<?>> scheduledEvents = new HashMap<>();
    private final Map<Path, List<DirectoryChangeEvent>> scheduledEventKinds = new ConcurrentHashMap<>();
    public WatchServiceImpl(WatchServiceConfiguration config, BundleContext bundleContext) throws IOException {
        if (config.name().isBlank()) {
            throw new IllegalArgumentException("service name must not be blank");
        this.name = config.name();
        executor = Executors.newSingleThreadExecutor(r -> new Thread(r, name));
        scheduler = ThreadPoolManager.getScheduledPool("watchservice");
    public void modified(WatchServiceConfiguration config) throws IOException {
        logger.trace("Trying to setup WatchService '{}' with path '{}'", config.name(), config.path());
        Path basePath = Path.of(config.path()).toAbsolutePath();
        if (basePath.equals(this.basePath)) {
        this.basePath = basePath;
            closeWatcherAndUnregister();
            if (!Files.exists(basePath)) {
                logger.info("Watch directory '{}' does not exist. Trying to create it.", basePath);
                Files.createDirectories(basePath);
            DirectoryWatcher newDirWatcher = DirectoryWatcher.builder().listener(this).path(basePath).build();
            CompletableFuture
                    .runAsync(
                            () -> newDirWatcher.watchAsync(executor)
                                    .thenRun(() -> logger.debug("WatchService '{}' has been shut down.", name)),
                            ThreadPoolManager.getScheduledPool(ThreadPoolManager.THREAD_POOL_NAME_COMMON))
                    .thenRun(this::registerWatchService);
            this.dirWatcher = newDirWatcher;
        } catch (NoSuchFileException e) {
            // log message here, otherwise it'll be swallowed by the call to newInstance in the factory
            // also re-throw the exception to indicate that we failed
            logger.warn("Could not instantiate WatchService '{}', directory '{}' is missing.", name, e.getMessage());
            logger.warn("Could not instantiate WatchService '{}':", name, e);
            logger.warn("Failed to shutdown WatchService '{}'", name, e);
    private void registerWatchService() {
        properties.put(WatchService.SERVICE_PROPERTY_NAME, name);
        this.reg = bundleContext.registerService(WatchService.class, this, properties);
        logger.debug("WatchService '{}' completed initialization and registered itself as service.", name);
    private void closeWatcherAndUnregister() throws IOException {
        DirectoryWatcher localDirWatcher = this.dirWatcher;
        if (localDirWatcher != null) {
            localDirWatcher.close();
            this.dirWatcher = null;
        ServiceRegistration<?> localReg = this.reg;
        if (localReg != null) {
                localReg.unregister();
                logger.debug("WatchService '{}' was already unregistered.", name, e);
            this.reg = null;
        hashCache.clear();
    public Path getWatchPath() {
        Path basePath = this.basePath;
        if (basePath == null) {
            throw new IllegalStateException("Trying to access WatchService before initialization completed.");
    public void registerListener(WatchEventListener watchEventListener, List<Path> paths, boolean withSubDirectories) {
            throw new IllegalStateException("Trying to register listener before initialization completed.");
        for (Path path : paths) {
            Path absolutePath = path.isAbsolute() ? path : basePath.resolve(path).toAbsolutePath();
            if (absolutePath.startsWith(basePath)) {
                if (withSubDirectories) {
                    subDirPathListeners.add(new Listener(absolutePath, watchEventListener));
                    dirPathListeners.add(new Listener(absolutePath, watchEventListener));
                logger.warn("Tried to add path '{}' to listener '{}', but the base path of this listener is '{}'", path,
                        name, basePath);
    public void unregisterListener(WatchEventListener watchEventListener) {
        subDirPathListeners.removeIf(Listener.isListener(watchEventListener));
        dirPathListeners.removeIf(Listener.isListener(watchEventListener));
    public void onEvent(@Nullable DirectoryChangeEvent directoryChangeEvent) throws IOException {
        logger.trace("onEvent {}", directoryChangeEvent);
        if (directoryChangeEvent == null || directoryChangeEvent.isDirectory()
                || directoryChangeEvent.eventType() == DirectoryChangeEvent.EventType.OVERFLOW) {
            // exit early, we are neither interested in directory events nor in OVERFLOW events
        Path path = directoryChangeEvent.path();
        synchronized (scheduledEvents) {
            ScheduledFuture<?> future = scheduledEvents.remove(path);
            if (future != null && !future.isDone()) {
            future = scheduler.schedule(() -> notifyListeners(path), PROCESSING_TIME, TimeUnit.MILLISECONDS);
            scheduledEventKinds.computeIfAbsent(path, k -> new CopyOnWriteArrayList<>()).add(directoryChangeEvent);
            scheduledEvents.put(path, future);
    private void notifyListeners(Path path) {
        List<DirectoryChangeEvent> events = scheduledEventKinds.remove(path);
        if (events == null || events.isEmpty()) {
            logger.debug("Tried to notify listeners of change events for '{}', but the event list is empty.", path);
        DirectoryChangeEvent firstElement = events.getFirst();
        DirectoryChangeEvent lastElement = events.getLast();
        // determine final event
        if (lastElement.eventType() == DirectoryChangeEvent.EventType.DELETE) {
            if (firstElement.eventType() == DirectoryChangeEvent.EventType.CREATE) {
                logger.debug("Discarding events for '{}' because file was immediately deleted after creation", path);
            hashCache.remove(lastElement.path());
            doNotify(path, Kind.DELETE);
        } else if (firstElement.eventType() == DirectoryChangeEvent.EventType.CREATE) {
            if (lastElement.hash() == null) {
                logger.warn("Detected invalid event (hash must not be null for CREATE/MODIFY): {}", lastElement);
            hashCache.put(lastElement.path(), lastElement.hash());
            doNotify(path, Kind.CREATE);
            FileHash oldHash = hashCache.put(lastElement.path(), lastElement.hash());
            if (!Objects.equals(oldHash, lastElement.hash())) {
                // only notify if hashes are different, otherwise the file content did not chnge
                doNotify(path, Kind.MODIFY);
    private void doNotify(Path path, Kind kind) {
        logger.trace("Notifying listeners of '{}' event for '{}'.", kind, path);
        subDirPathListeners.stream().filter(isChildOf(path)).forEach(l -> l.notify(path, kind));
        dirPathListeners.stream().filter(isDirectChildOf(path)).forEach(l -> l.notify(path, kind));
    public static Predicate<Listener> isChildOf(Path path) {
        return l -> path.startsWith(l.rootPath);
    public static Predicate<Listener> isDirectChildOf(Path path) {
        return l -> path.startsWith(l.rootPath) && l.rootPath.relativize(path).getNameCount() == 1;
    private record Listener(Path rootPath, WatchEventListener watchEventListener) {
        void notify(Path fullPath, Kind kind) {
            watchEventListener.processWatchEvent(kind, fullPath);
        static Predicate<Listener> isListener(WatchEventListener watchEventListener) {
            return l -> watchEventListener.equals(l.watchEventListener);
