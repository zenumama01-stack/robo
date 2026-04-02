import static java.util.Collections.synchronizedMap;
import static org.openhab.core.config.core.ConfigUtil.normalizeTypes;
 * This class is a wrapper for configuration settings of {@code org.openhab.core.thing.Thing}s.
 * @author Kai Kreuzer - added constructors and normalization
 * @author Gerhard Riegler - added converting BigDecimal values to the type of the configuration class field
 * @author Chris Jackson - fix concurrent modification exception when removing properties
 * @author Markus Rathgeb - add copy constructor
 * @author Michael Riess - fix concurrent modification exception when setting properties
 * @author Michael Riess - fix equals() implementation
public class Configuration {
    public Configuration() {
        this(Map.of(), true);
     * Create a new configuration.
     * The new configuration is initialized with the values of the given configuration.
     * @param configuration the configuration that should be cloned (may be null)
    public Configuration(final @Nullable Configuration configuration) {
        this(configuration == null ? Map.of() : configuration.properties, true);
     * @param properties the properties the configuration should be filled. If null, an empty configuration is created.
    public Configuration(@Nullable Map<String, Object> properties) {
        this(properties == null ? Map.of() : properties, false);
     * @param properties the properties to initialize (may be null)
     * @param alreadyNormalized flag if the properties are already normalized
    private Configuration(final Map<String, Object> properties, final boolean alreadyNormalized) {
        this.properties = synchronizedMap(alreadyNormalized ? new HashMap<>(properties) : normalizeTypes(properties));
    public <T> T as(Class<T> configurationClass) {
        synchronized (properties) {
            return ConfigParser.configurationAs(properties, configurationClass);
     * Check if the given key is present in the configuration.
     * @param key the key that existence should be checked
     * @return true if the key is part of the configuration, false if not
    public boolean containsKey(String key) {
        return properties.containsKey(key);
    public Object get(String key) {
        return properties.get(key);
    public Object put(String key, @Nullable Object value) {
        Object normalizedValue = value == null ? null : ConfigUtil.normalizeType(value, null);
        return properties.put(key, normalizedValue);
    public Object remove(String key) {
        return properties.remove(key);
            return Collections.unmodifiableSet(new HashSet<>(properties.keySet()));
    public Collection<Object> values() {
            return Collections.unmodifiableCollection(new ArrayList<>(properties.values()));
            return Collections.unmodifiableMap(new HashMap<>(properties));
    public void setProperties(Map<String, Object> newProperties) {
            this.properties.clear();
            newProperties.entrySet().forEach(e -> put(e.getKey(), e.getValue()));
        return properties.hashCode();
        return (obj instanceof Configuration c) && this.properties.equals(c.properties);
        sb.append("Configuration[");
            boolean first = true;
            for (final Map.Entry<String, Object> prop : properties.entrySet()) {
                if (first) {
                Object value = prop.getValue();
                sb.append(String.format("{key=%s; type=%s; value=%s}", prop.getKey(),
                        value != null ? value.getClass().getSimpleName() : "?", value));
