 * Watches file-system events and passes them to our {@link ConfigDispatcher}
 * @author Stefan Triller - factored out this code from {@link ConfigDispatcher}
public class ConfigDispatcherFileWatcher implements WatchService.WatchEventListener {
    public static final String SERVICEDIR_PROG_ARGUMENT = "openhab.servicedir";
    /** The default folder name of the configuration folder of services */
    public static final String SERVICES_FOLDER = "services";
    private final Logger logger = LoggerFactory.getLogger(ConfigDispatcherFileWatcher.class);
    private final ConfigDispatcher configDispatcher;
     * Creates and activates the ConfigDispatcherFileWatcher.
     * This constructor is called by the OSGi framework during component activation.
     * It registers this component as a file system watch listener for the services configuration
     * directory and performs an initial processing of all existing configuration files.
     * @param configDispatcher the ConfigDispatcher service used to process configuration files
     * @param watchService the WatchService used to monitor file system changes in the configuration directory
    public ConfigDispatcherFileWatcher(final @Reference ConfigDispatcher configDispatcher,
            final @Reference(target = WatchService.CONFIG_WATCHER_FILTER) WatchService watchService) {
        this.configDispatcher = configDispatcher;
        String servicesFolder = System.getProperty(SERVICEDIR_PROG_ARGUMENT, SERVICES_FOLDER);
        watchService.registerListener(this, Path.of(servicesFolder), false);
        configDispatcher.processConfigFile(Path.of(OpenHAB.getConfigFolder(), servicesFolder).toFile());
     * Deactivates the ConfigDispatcherFileWatcher.
     * This method is called by the OSGi framework during component deactivation.
     * It unregisters this component from the watch service to stop receiving file system events.
     * Processes file system watch events for configuration files.
     * This method is called by the WatchService when a file is created, modified, or deleted
     * in the monitored services directory. It filters events to process only .cfg files that
     * are not hidden, and delegates the actual processing to the ConfigDispatcher.
     * @param kind the type of file system event (CREATE, MODIFY, or DELETE)
     * @param fullPath the full path to the file that triggered the event
            if (kind == WatchService.Kind.CREATE || kind == WatchService.Kind.MODIFY) {
                if (!Files.isHidden(fullPath) && fullPath.toString().endsWith(".cfg")) {
                    configDispatcher.processConfigFile(fullPath.toFile());
            } else if (kind == WatchService.Kind.DELETE) {
                // Detect if a service specific configuration file was removed. We want to
                // notify the service in this case with an updated empty configuration.
                if (!fullPath.toString().endsWith(".cfg")) {
                configDispatcher.fileRemoved(fullPath.toString());
            logger.error("Failed to process watch event {} for {}", kind, fullPath, e);
