 * The {@link TypeMigrator} interface allows the implementation of JSON storage type migrations
public interface TypeMigrator {
     * Get the name of the old (stored) type
     * @return Full class name
    String getOldType();
     * Get the name of the new type
    String getNewType();
     * Migrate the old type to the new type
     * The default implementation can be used if type is renamed only.
     * @param oldValue The {@link JsonElement} representation of the old type
     * @return The corresponding {@link JsonElement} representation of the new type
     * @throws TypeMigrationException if an error occurs
    default JsonElement migrate(JsonElement oldValue) throws TypeMigrationException {
