package org.openhab.core.id;
import java.io.BufferedReader;
 * This class provides a unique ID for the instance that can be used for identification, e.g. when
 * integrating with external systems. The UUID is generated only once and written to the file system, so that it does
 * not change over time.
public class InstanceUUID {
    private static final Logger LOGGER = LoggerFactory.getLogger(InstanceUUID.class);
    static final String UUID_FILE_NAME = "uuid";
    static String uuid;
     * Retrieves a unified unique id, based on {@link java.util.UUID#randomUUID()}
     * @return a UUID which identifies the instance or null, if uuid cannot be persisted
    public static synchronized @Nullable String get() {
        if (uuid == null) {
                File file = new File(OpenHAB.getUserDataFolder() + File.separator + UUID_FILE_NAME);
                if (!file.exists()) {
                    uuid = generateToFile(file);
                    String valueInFile = readFirstLine(file);
                    if (!valueInFile.isEmpty()) {
                        uuid = valueInFile;
                        LOGGER.debug("UUID '{}' has been restored from file '{}'", file.getAbsolutePath(), uuid);
                        LOGGER.warn("UUID file '{}' has no content, rewriting it now with '{}'", file.getAbsolutePath(),
                                uuid);
                LOGGER.error("Failed writing the UUID file: {}", e.getMessage());
     * Generates a new UUID and writes it to the specified file.
     * This method creates any necessary parent directories and writes the UUID to the file
     * using UTF-8 encoding.
     * @param file the file where the UUID will be stored
     * @return the newly generated UUID string
     * @throws IOException if the file cannot be written
    private static String generateToFile(File file) throws IOException {
        // create intermediary directories
        if (file.getParentFile() instanceof File parentFile) {
            parentFile.mkdirs();
        String newUuid = java.util.UUID.randomUUID().toString();
        Files.writeString(file.toPath(), newUuid, StandardCharsets.UTF_8);
        return newUuid;
     * Reads the first line from the UUID file.
     * This method opens the file, reads the first line, and properly closes the reader.
     * If the file cannot be read or is empty, an empty string is returned and a warning is logged.
     * @param file the file to read from
     * @return the first line of the file, or an empty string if the file cannot be read or is empty
    private static String readFirstLine(File file) {
        try (final BufferedReader reader = Files.newBufferedReader(file.toPath(), StandardCharsets.UTF_8)) {
            return reader.readLine() instanceof String line ? line : "";
        } catch (IOException ioe) {
            LOGGER.warn("Failed reading the UUID file '{}': {}", file.getAbsolutePath(), ioe.getMessage());
