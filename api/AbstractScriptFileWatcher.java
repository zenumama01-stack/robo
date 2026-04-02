import java.util.concurrent.Executors;
import javax.script.ScriptEngine;
import org.openhab.core.automation.module.script.ScriptEngineContainer;
import org.openhab.core.automation.module.script.rulesupport.internal.loader.ScriptFileReference;
import org.openhab.core.service.ReadyMarker;
import org.openhab.core.service.ReadyMarkerFilter;
 * The {@link AbstractScriptFileWatcher} is default implementation for watching a directory for files. If a new/modified
 * file is detected, the script is read and passed to the {@link ScriptEngineManager}. It needs to be sub-classed for
 * actual use.
 * @author Kai Kreuzer - improved logging and removed thread pool
 * @author Jonathan Gilbert - added dependency tracking and per-script start levels; made reusable
 * @author Jan N. Klug - Refactored dependency tracking to script engine factories
public abstract class AbstractScriptFileWatcher implements WatchService.WatchEventListener, ReadyService.ReadyTracker,
        ScriptDependencyTracker.Listener, ScriptEngineManager.FactoryChangeListener, ScriptFileWatcher {
    private static final Set<String> EXCLUDED_FILE_EXTENSIONS = Set.of("txt", "old", "example", "backup", "md", "swp",
            "tmp", "bak");
    private static final String REGEX_SEPARATOR = "\\".equals(File.separator) ? "\\\\" : File.separator;
    private static final List<Pattern> START_LEVEL_PATTERNS = List.of( //
            // script in immediate slXX directory
            Pattern.compile(".*" + REGEX_SEPARATOR + "sl(\\d{2})" + REGEX_SEPARATOR + "[^" + REGEX_SEPARATOR + "]+"),
            // script named <name>.slXX.<ext>
            Pattern.compile(".*" + REGEX_SEPARATOR + "[^" + REGEX_SEPARATOR + "]+\\.sl(\\d{2})\\.[^" + REGEX_SEPARATOR
                    + ".]+"));
    private final Logger logger = LoggerFactory.getLogger(AbstractScriptFileWatcher.class);
    private final ScriptEngineManager manager;
    private final ReadyService readyService;
    private final Path watchPath;
    private final boolean watchSubDirectories;
    protected final ScheduledExecutorService scheduler;
    private final Map<String, ScriptFileReference> scriptMap = new ConcurrentHashMap<>();
    private final Map<String, Lock> scriptLockMap = new ConcurrentHashMap<>();
    private final CompletableFuture<@Nullable Void> initialized = new CompletableFuture<>();
    private volatile int currentStartLevel;
    protected AbstractScriptFileWatcher(final WatchService watchService, final ScriptEngineManager manager,
            final ReadyService readyService, final StartLevelService startLevelService, final String fileDirectory,
            boolean watchSubDirectories) {
        this.manager = manager;
        this.readyService = readyService;
        this.watchSubDirectories = watchSubDirectories;
        this.watchPath = watchService.getWatchPath().resolve(fileDirectory);
        this.scheduler = getScheduler();
        // Start with the lowest level to ensure the code in onReadyMarkerAdded runs
        this.currentStartLevel = StartLevelService.STARTLEVEL_OSGI;
     * Get the base path that is used by this {@link ScriptFileWatcher}
     * @return the {@link Path} used
    protected Path getWatchPath() {
        return watchPath;
     * Can be overridden by subclasses (e.g. for testing)
     * @return a {@link ScheduledExecutorService}
    protected ScheduledExecutorService getScheduler() {
        return Executors.newSingleThreadScheduledExecutor(new NamedThreadFactory("scriptwatcher"));
    public void activate() {
        if (!Files.exists(watchPath)) {
                Files.createDirectories(watchPath);
                logger.warn("Failed to create watched directory: {}", watchPath);
        } else if (!Files.isDirectory(watchPath)) {
            logger.warn("Trying to watch directory {}, however it is a file", watchPath);
        manager.addFactoryChangeListener(this);
        readyService.registerTracker(this, new ReadyMarkerFilter().withType(StartLevelService.STARTLEVEL_MARKER_TYPE));
        watchService.registerListener(this, watchPath, watchSubDirectories);
        manager.removeFactoryChangeListener(this);
        readyService.unregisterTracker(this);
        CompletableFuture.allOf(
                Set.copyOf(scriptMap.keySet()).stream().map(this::removeFile).toArray(CompletableFuture<?>[]::new))
                .thenRun(scheduler::shutdownNow);
    public CompletableFuture<@Nullable Void> ifInitialized() {
     * Get the scriptType (file-extension or MIME-type) for a given file
     * <p />
     * The scriptType is determined by the file extension. The extensions in {@link #EXCLUDED_FILE_EXTENSIONS} are
     * ignored. Implementations should override this
     * method if they provide a MIME-type instead of the file extension.
     * @param scriptFilePath the {@link Path} to the script
     * @return an {@link Optional<String>} containing the script type
    protected Optional<String> getScriptType(Path scriptFilePath) {
        String fileName = scriptFilePath.toString();
        int index = fileName.lastIndexOf(".");
            return Optional.empty();
        String fileExtension = fileName.substring(index + 1);
        // ignore known file extensions for "temp" files
        if (EXCLUDED_FILE_EXTENSIONS.contains(fileExtension) || fileExtension.endsWith("~")) {
        return Optional.of(fileExtension);
     * Gets the individual start level for a given file
     * The start level is derived from the name and location of
     * the file by {@link #START_LEVEL_PATTERNS}. If no match is found, {@link StartLevelService#STARTLEVEL_RULEENGINE}
     * is used.
     * @return the start level for this script
    protected int getStartLevel(Path scriptFilePath) {
        for (Pattern p : START_LEVEL_PATTERNS) {
            Matcher m = p.matcher(scriptFilePath.toString());
            if (m.find() && m.groupCount() > 0) {
                    return Integer.parseInt(m.group(1));
                } catch (NumberFormatException nfe) {
                    logger.warn("Extracted start level {} from {}, but it's not an integer. Ignoring.", m.group(1),
                            scriptFilePath);
        return StartLevelService.STARTLEVEL_RULEENGINE;
    private List<Path> listFiles(Path path, boolean includeSubDirectory) {
        try (Stream<Path> stream = Files.walk(path, includeSubDirectory ? Integer.MAX_VALUE : 1)) {
            return stream.filter(file -> !Files.isDirectory(file)).toList();
        if (!initialized.isDone()) {
            // discard events if the initial import has not finished
        // Subdirectory events are filtered out by WatchService, so we only need to deal with files
        if (kind == DELETE) {
            String scriptIdentifier = ScriptFileReference.getScriptIdentifier(fullPath);
            if (scriptMap.containsKey(scriptIdentifier)) {
                removeFile(scriptIdentifier);
        } else if (!file.isHidden() && file.canRead() && (kind == CREATE || kind == MODIFY)) {
            addFiles(listFiles(fullPath, watchSubDirectories));
    private CompletableFuture<Void> addFiles(Collection<Path> files) {
        return CompletableFuture.allOf(files.stream().map(this::getScriptFileReference).flatMap(Optional::stream)
                .sorted().map(this::addScriptFileReference).toArray(CompletableFuture<?>[]::new));
    private CompletableFuture<Void> addScriptFileReference(ScriptFileReference newRef) {
        ScriptFileReference ref = Objects
                .requireNonNull(scriptMap.computeIfAbsent(newRef.getScriptIdentifier(), k -> newRef));
        // check if we are ready to load the script, otherwise we don't need to queue it
        if (currentStartLevel >= ref.getStartLevel() && !ref.getQueueStatus().getAndSet(true)) {
            return importFileWhenReady(ref.getScriptIdentifier());
    private Optional<ScriptFileReference> getScriptFileReference(Path path) {
        return getScriptType(path).map(scriptType -> new ScriptFileReference(path, scriptType, getStartLevel(path)));
    private CompletableFuture<Void> removeFile(String scriptIdentifier) {
        return CompletableFuture.runAsync(() -> {
            Lock lock = getLockForScript(scriptIdentifier);
                scriptMap.computeIfPresent(scriptIdentifier, (id, ref) -> {
                    if (ref.getLoadedStatus().get()) {
                        manager.removeEngine(scriptIdentifier);
                        logger.debug("Unloaded script '{}'", scriptIdentifier);
                        logger.debug("Dequeued script '{}'", scriptIdentifier);
                    logger.warn("Failed to unload script '{}'", scriptIdentifier);
                scriptLockMap.remove(scriptIdentifier);
                lock.unlock();
        }, scheduler);
    private Lock getLockForScript(String scriptIdentifier) {
        Lock lock = Objects.requireNonNull(scriptLockMap.computeIfAbsent(scriptIdentifier, k -> new ReentrantLock()));
        lock.lock();
        return lock;
    private CompletableFuture<Void> importFileWhenReady(String scriptIdentifier) {
            ScriptFileReference ref = scriptMap.get(scriptIdentifier);
            if (ref == null) {
                logger.warn("Failed to import script '{}': script reference not found", scriptIdentifier);
            ref.getQueueStatus().set(false);
                if (manager.isSupported(ref.getScriptType()) && ref.getStartLevel() <= currentStartLevel) {
                    logger.info("(Re-)Loading script '{}'", scriptIdentifier);
                    if (createAndLoad(ref)) {
                        ref.getLoadedStatus().set(true);
                        // make sure script engine is successfully closed and the loading is re-tried
                        manager.removeEngine(ref.getScriptIdentifier());
                        ref.getLoadedStatus().set(false);
                    logger.debug("Enqueued script '{}'", ref.getScriptIdentifier());
    private boolean createAndLoad(ScriptFileReference ref) {
        String scriptIdentifier = ref.getScriptIdentifier();
        try (InputStreamReader reader = new InputStreamReader(Files.newInputStream(ref.getScriptFilePath()),
                StandardCharsets.UTF_8)) {
            ScriptEngineContainer container = manager.createScriptEngine(ref.getScriptType(),
                    ref.getScriptIdentifier());
            if (container != null) {
                container.getScriptEngine().put(ScriptEngine.FILENAME, scriptIdentifier);
                if (manager.loadScript(container.getIdentifier(), reader)) {
            logger.warn("Script loading error, ignoring file '{}'", scriptIdentifier);
            logger.warn("Failed to load file '{}': {}", ref.getScriptFilePath(), e.getMessage());
    public void onDependencyChange(String scriptIdentifier) {
        logger.debug("Reimporting {}...", scriptIdentifier);
        if (ref != null && !ref.getQueueStatus().getAndSet(true)) {
            importFileWhenReady(scriptIdentifier);
    public synchronized void onReadyMarkerAdded(ReadyMarker readyMarker) {
        int previousLevel = currentStartLevel;
        int curStartLevel = Integer.parseInt(readyMarker.getIdentifier());
        currentStartLevel = curStartLevel;
        logger.trace("Added ready marker {}: start level changed from {} to {}. watchPath: {}", readyMarker,
                previousLevel, curStartLevel, watchPath);
        if (curStartLevel < StartLevelService.STARTLEVEL_STATES) {
            // ignore start level less than 30
            addFiles(listFiles(watchPath, watchSubDirectories)).thenRun(() -> initialized.complete(null));
            scriptMap.values().stream().sorted()
                    .filter(ref -> needsStartLevelProcessing(ref, previousLevel, curStartLevel))
                    .forEach(ref -> importFileWhenReady(ref.getScriptIdentifier()));
    private boolean needsStartLevelProcessing(ScriptFileReference ref, int previousLevel, int newLevel) {
        int refStartLevel = ref.getStartLevel();
        return !ref.getLoadedStatus().get() && newLevel >= refStartLevel && previousLevel < refStartLevel
                && !ref.getQueueStatus().getAndSet(true);
    public void onReadyMarkerRemoved(ReadyMarker readyMarker) {
        // we don't need to process this, as openHAB only reduces its start level when the system is shutdown
        // in this case the service is de-activated anyway and al scripts are removed by {@link #deactivate}
    public void factoryAdded(@Nullable String scriptType) {
        scriptMap.forEach((scriptIdentifier, ref) -> {
            if (ref.getScriptType().equals(scriptType) && !ref.getQueueStatus().getAndSet(true)) {
    public void factoryRemoved(@Nullable String scriptType) {
        if (scriptType == null) {
        Set<String> toRemove = scriptMap.values().stream()
                .filter(ref -> ref.getLoadedStatus().get() && scriptType.equals(ref.getScriptType()))
                .map(ScriptFileReference::getScriptIdentifier).collect(Collectors.toSet());
        toRemove.forEach(this::removeFile);
