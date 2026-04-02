import org.openhab.core.config.core.status.ConfigStatusSource;
 * An implementation of {@link ConfigStatusSource} for the {@link Thing} entity.
public final class ThingConfigStatusSource extends ConfigStatusSource {
    private static final String TOPIC = "openhab/things/{thingUID}/config/status";
     * Creates a new {@link ThingConfigStatusSource} for the given thing UID.
     * @param thingUID the UID of the thing
    public ThingConfigStatusSource(String thingUID) {
        return TOPIC.replace("{thingUID}", this.entityId);
