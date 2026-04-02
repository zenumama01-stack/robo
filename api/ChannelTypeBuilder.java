import org.openhab.core.thing.internal.type.StateChannelTypeBuilderImpl;
 * Interface for {@link ChannelTypeBuilder}.
public interface ChannelTypeBuilder<@NonNull T extends ChannelTypeBuilder<T>> {
     * Specify whether this is an advanced channel, default is false
     * @param advanced true if this is an advanced {@link ChannelType}
    T isAdvanced(boolean advanced);
     * Sets the Description for the ChannelType
     * @param description StateDescription for the ChannelType
    T withDescription(String description);
     * Sets the Category for the ChannelType
     * @param category Category for the ChannelType
    T withCategory(String category);
     * Adds a tag to the ChannelType
     * @param tag Tag to be added to the ChannelType
    T withTag(String tag);
     * Sets the Tags for the ChannelType
     * @param tags Collection of tags to be added to the ChannelType
    T withTags(Collection<String> tags);
    T withTags(SemanticTag... tags);
     * Sets the ConfigDescriptionURI for the ChannelType
     * @param configDescriptionURI URI that references the ConfigDescription of the ChannelType
    T withConfigDescriptionURI(URI configDescriptionURI);
     * Build the ChannelType with the given values
     * @return the created ChannelType
    ChannelType build();
     * Create an instance of a ChannelTypeBuilder for {@link ChannelType}s of type STATE
     * @param channelTypeUID UID of the ChannelType
     * @param label Label for the ChannelType
     * @param itemType ItemType that can be linked to the ChannelType
     * @return ChannelTypeBuilder for {@link ChannelType}s of type STATE
    static StateChannelTypeBuilder state(ChannelTypeUID channelTypeUID, String label, String itemType) {
        return new StateChannelTypeBuilderImpl(channelTypeUID, label, itemType);
     * Create an instance of a ChannelTypeBuilder for {@link ChannelType}s of type TRIGGER
     * @return ChannelTypeBuilder for {@link ChannelType}s of type TRIGGER
    static TriggerChannelTypeBuilder trigger(ChannelTypeUID channelTypeUID, String label) {
        return new TriggerChannelTypeBuilderImpl(channelTypeUID, label);
