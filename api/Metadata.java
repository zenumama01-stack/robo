 * This is a data class for storing meta-data for a given item and namespace.
 * It is the entity used for within the {@link MetadataRegistry}.
public final class Metadata implements Identifiable<MetadataKey> {
    private final MetadataKey key;
    private final Map<String, Object> configuration;
    Metadata() {
        key = new MetadataKey();
        configuration = Map.of();
    public Metadata(MetadataKey key, String value, @Nullable Map<String, Object> configuration) {
        this.configuration = configuration != null ? Collections.unmodifiableMap(new HashMap<>(configuration))
                : Map.of();
    public MetadataKey getUID() {
     * Provides the configuration meta-data.
     * @return configuration as a map of key-value pairs
    public Map<String, Object> getConfiguration() {
     * Provides the main value of the meta-data.
     * @return the main meta-data as a string
        Metadata other = (Metadata) obj;
        return key.equals(other.key);
        builder.append("Metadata [key=");
        builder.append(key);
        builder.append(", value=");
        builder.append(value);
        builder.append(", configuration=");
        builder.append(Arrays.toString(configuration.entrySet().toArray()));
