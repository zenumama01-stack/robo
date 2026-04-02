 * This class represents the configuration that is used for group items.
public class PersistenceGroupConfig extends PersistenceConfig {
    private final String group;
    public PersistenceGroupConfig(final String group) {
        this.group = group;
    public String getGroup() {
        return String.format("%s [group=%s]", getClass().getSimpleName(), group);
