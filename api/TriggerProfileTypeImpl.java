 * Default implementation of a {@link TriggerProfileType}.
public class TriggerProfileTypeImpl implements TriggerProfileType {
    private final Collection<ChannelTypeUID> supportedChannelTypeUIDs;
    public TriggerProfileTypeImpl(ProfileTypeUID profileTypeUID, String label, Collection<String> supportedItemTypes,
            Collection<ChannelTypeUID> supportedChannelTypeUIDs) {
        this.supportedChannelTypeUIDs = Collections.unmodifiableCollection(supportedChannelTypeUIDs);
    public Collection<ChannelTypeUID> getSupportedChannelTypeUIDs() {
        return supportedChannelTypeUIDs;
