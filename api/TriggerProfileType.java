 * Describes a {@link TriggerProfile} type.
public interface TriggerProfileType extends ProfileType {
     * @return a collection of ChannelTypeUIDs (may be empty if all are supported).
    Collection<ChannelTypeUID> getSupportedChannelTypeUIDs();
