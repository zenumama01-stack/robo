 * The {@link ConfigStatusSource} represents a source which would like to propagate its new configuration status. It is
 * used as input for {@link ConfigStatusCallback#configUpdated(ConfigStatusSource)}.
public abstract class ConfigStatusSource {
    /** The id of the entity whose new configuration status is to be propagated. */
    public final String entityId;
     * Creates a new config status source object.
     * @param entityId the id of the entity whose new configuration status is to be propagated
    public ConfigStatusSource(String entityId) {
        this.entityId = entityId;
     * @return the topic over which the new configuration status is to be propagated
    public abstract String getTopic();
