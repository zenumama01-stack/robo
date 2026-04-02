 * This class represents the configuration that identify item(s) by name.
public class PersistenceItemConfig extends PersistenceConfig {
    final String item;
    public PersistenceItemConfig(final String item) {
        this.item = item;
    public String getItem() {
        return String.format("%s [item=%s]", getClass().getSimpleName(), item);
