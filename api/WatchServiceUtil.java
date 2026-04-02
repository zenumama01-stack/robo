 * This class isolates the java 1.7 functionality which tracks the file system changes.
public class WatchServiceUtil {
    private static final Map<AbstractFileProvider, Map<String, AutomationWatchService>> WATCH_SERVICES = new HashMap<>();
    public static void initializeWatchService(String watchingDir, AbstractFileProvider provider,
            WatchService watchService) {
        AutomationWatchService aws = null;
        synchronized (WATCH_SERVICES) {
            Map<String, AutomationWatchService> watchers = Objects
                    .requireNonNull(WATCH_SERVICES.computeIfAbsent(provider, k -> new HashMap<>()));
            if (watchers.get(watchingDir) == null) {
                aws = new AutomationWatchService(provider, watchService, watchingDir);
                watchers.put(watchingDir, aws);
        if (aws != null) {
            aws.activate();
            provider.importResources(new File(watchingDir));
    public static void deactivateWatchService(String watchingDir, AbstractFileProvider provider) {
            Map<String, AutomationWatchService> watchers = WATCH_SERVICES.get(provider);
            if (watchers != null) {
                aws = watchers.remove(watchingDir);
                if (watchers.isEmpty()) {
                    WATCH_SERVICES.remove(provider);
            aws.deactivate();
            Path sourcePath = aws.getSourcePath();
            provider.removeResources(sourcePath.toFile());
