import org.openhab.core.storage.json.internal.migration.BridgeImplTypeMigrator;
import org.openhab.core.storage.json.internal.migration.PersistedTransformationTypeMigrator;
import org.openhab.core.storage.json.internal.migration.ThingImplTypeMigrator;
 * This implementation of {@link StorageService} provides a mechanism to store
 * data in JSON files.
@Component(name = "org.openhab.core.storage.json", configurationPid = "org.openhab.storage.json", property = { //
        Constants.SERVICE_PID + "=org.openhab.storage.json", //
        "storage.format=json" })
@ConfigurableService(category = "system", label = "Json Storage", description_uri = JsonStorageService.CONFIG_URI)
public class JsonStorageService implements StorageService {
    private static final int MAX_FILENAME_LENGTH = 127;
     * Contains a map of needed migrations, key is the storage name
    private static final Map<String, List<TypeMigrator>> MIGRATORS = Map.of( //
            "org.openhab.core.thing.Thing", List.of(new BridgeImplTypeMigrator(), new ThingImplTypeMigrator()), //
            "org.openhab.core.transform.TransformationConfiguration",
            List.of(new PersistedTransformationTypeMigrator()));
    private final Logger logger = LoggerFactory.getLogger(JsonStorageService.class);
    /** the folder name to store database ({@code jsondb} by default) */
    private String dbFolderName = "jsondb";
    protected static final String CONFIG_URI = "system:json_storage";
    private static final String CFG_MAX_BACKUP_FILES = "backup_files";
    private static final String CFG_WRITE_DELAY = "write_delay";
    private static final String CFG_MAX_DEFER_DELAY = "max_defer_delay";
    private int maxBackupFiles = 5;
    private int writeDelay = 500;
    private int maxDeferredPeriod = 60000;
    private final Map<String, JsonStorage<Object>> storageList = new HashMap<>();
        dbFolderName = OpenHAB.getUserDataFolder() + File.separator + dbFolderName;
        File folder = new File(dbFolderName);
        if (!folder.exists()) {
            folder.mkdirs();
        File backup = new File(dbFolderName, "backup");
        if (!backup.exists()) {
            backup.mkdirs();
        logger.debug("Json Storage Service: Activated.");
        Object value = properties.get(CFG_MAX_BACKUP_FILES);
                maxBackupFiles = Integer.parseInt((String) value);
            logger.error("Value {} for {} is invalid. Using {}.", value, CFG_MAX_BACKUP_FILES, maxBackupFiles);
        value = properties.get(CFG_WRITE_DELAY);
                writeDelay = Integer.parseInt((String) value);
            logger.error("Value {} for {} is invalid. Using {}.", value, CFG_WRITE_DELAY, writeDelay);
        value = properties.get(CFG_MAX_DEFER_DELAY);
                maxDeferredPeriod = Integer.parseInt((String) value);
            logger.error("Value {} for {} is invalid. Using {}.", value, CFG_MAX_DEFER_DELAY, maxDeferredPeriod);
        // Since we're using a delayed commit, we need to write out any data
        for (JsonStorage<Object> storage : storageList.values()) {
            storage.flush();
        logger.debug("Json Storage Service: Deactivated.");
    public <T> Storage<T> getStorage(String name, @Nullable ClassLoader classLoader) {
        File legacyFile = new File(dbFolderName, name + ".json");
        File file = new File(dbFolderName, urlEscapeUnwantedChars(name) + ".json");
        if (legacyFile.exists()) {
            file = legacyFile;
        JsonStorage<Object> oldStorage = storageList.get(name);
        if (oldStorage != null) {
            oldStorage.flush();
        JsonStorage<T> newStorage = new JsonStorage<>(file, classLoader, maxBackupFiles, writeDelay, maxDeferredPeriod,
                MIGRATORS.getOrDefault(name, List.of()));
        storageList.put(name, (JsonStorage<Object>) newStorage);
        return newStorage;
    public <T> Storage<T> getStorage(String name) {
        return getStorage(name, null);
     * Escapes all invalid url characters and strips the maximum length to 127 to be used as a file name
     * @param s the string to be escaped
     * @return url-encoded string or the original string if UTF-8 is not supported on the system
    protected String urlEscapeUnwantedChars(String s) {
        String result = URLEncoder.encode(s, StandardCharsets.UTF_8);
        int length = Math.min(result.length(), MAX_FILENAME_LENGTH);
        return result.substring(0, length);
