 * Internal data structure of the {@link JsonStorage}
public class StorageEntry {
    @SerializedName("class") // in order to stay backwards compatible
    private final String entityClassName;
    private final Object value;
    public StorageEntry(String entityClassName, Object value) {
        this.entityClassName = entityClassName;
    public String getEntityClassName() {
        return entityClassName;
    public Object getValue() {
