 * This class is an implementation of {@link WatchService.WatchEventListener} which is responsible for tracking file
 * system changes.
 * It provides functionality for tracking {@link #watchingDir} changes to import or remove the automation objects.
 * @author Arne Seime - Fixed watch event handling
public class AutomationWatchService implements WatchService.WatchEventListener {
    private final Path watchingDir;
    private AbstractFileProvider provider;
    private final Logger logger = LoggerFactory.getLogger(AutomationWatchService.class);
    public AutomationWatchService(AbstractFileProvider provider, WatchService watchService, String watchingDir) {
        this.watchingDir = Path.of(watchingDir);
        watchService.registerListener(this, watchingDir);
    public Path getSourcePath() {
        return watchingDir;
            if (kind == WatchService.Kind.DELETE) {
                provider.removeResources(fullPath.toFile());
            } else if (!Files.isHidden(fullPath)
                    && (kind == WatchService.Kind.CREATE || kind == WatchService.Kind.MODIFY)) {
                provider.importResources(fullPath.toFile());
            logger.error("Failed to process automation watch event {} for \"{}\": {}", kind, fullPath, e.getMessage());
