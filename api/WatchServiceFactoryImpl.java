import org.openhab.core.service.WatchServiceFactory;
 * The {@link WatchServiceFactoryImpl} is a
@Component(immediate = true, service = WatchServiceFactory.class)
public class WatchServiceFactoryImpl implements WatchServiceFactory {
    private final Logger logger = LoggerFactory.getLogger(WatchServiceFactoryImpl.class);
    private final ConfigurationAdmin cm;
    public WatchServiceFactoryImpl(@Reference ConfigurationAdmin cm) {
        this.cm = cm;
        // make sure we start with a clean configuration.
        clearConfigurationAdmin();
        createWatchService(WatchService.CONFIG_WATCHER_NAME, Path.of(OpenHAB.getConfigFolder()));
    public void createWatchService(String name, Path basePath) {
            String filter = "(&(name=" + name + ")" + "(service.factoryPid=" + WatchService.SERVICE_PID + "))";
            Configuration[] configurations = cm.listConfigurations(filter);
            if (configurations == null || configurations.length == 0) {
                Configuration config = cm.createFactoryConfiguration(WatchService.SERVICE_PID, "?");
                Dictionary<String, Object> map = new Hashtable<>();
                map.put("name", name);
                map.put("path", basePath.toString());
                config.update(map);
                Configuration config = configurations[0];
                Dictionary<String, Object> map = config.getProperties();
        } catch (IOException | InvalidSyntaxException e1) {
            logger.error("Failed to create configuration with name `{}' and path '{}'", name, basePath);
    public void removeWatchService(String name) {
            Configuration[] configurations = this.cm.listConfigurations(filter);
            if (configurations != null) {
                configurations[0].delete();
            logger.error("Failed to remove configuration with name '{}", name);
    private void clearConfigurationAdmin() {
            String filter = "(service.factoryPid=" + WatchService.SERVICE_PID + ")";
                for (Configuration configuration : configurations) {
                        logger.error("Failed to remove configuration with name '{}",
                                configuration.getProperties().get("name"));
            logger.error("Failed to remove services.");
