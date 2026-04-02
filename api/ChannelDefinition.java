 * The {@link ChannelDefinition} class defines a {@link Channel} of a {@link ThingType}.
 * A {@link Channel} is part of a {@link Thing} that represents a functionality of it.
 * @author Dennis Nobel - Introduced ChannelTypeRegistry and channel type references
public class ChannelDefinition {
    private final ChannelTypeUID channelTypeUID;
    private final @Nullable AutoUpdatePolicy autoUpdatePolicy;
     * @param id the identifier of the channel (must neither be null nor empty)
     * @param channelTypeUID the type UID of the channel (must not be null)
     * @param properties the properties this Channel provides (could be null)
     * @param label the label for the channel to override channelType (could be null)
     * @param description the description for the channel to override channelType (could be null)
     * @param autoUpdatePolicy the auto update policy for the channel to override from the thing type (could be null)
     * @throws IllegalArgumentException if the ID is null or empty, or the type is null
    ChannelDefinition(String id, ChannelTypeUID channelTypeUID, @Nullable String label, @Nullable String description,
            @Nullable Map<String, String> properties, @Nullable AutoUpdatePolicy autoUpdatePolicy)
        if (id.isEmpty()) {
     * Returns the identifier of the channel.
     * @return the identifier of the channel (neither null, nor empty)
     * Returns the type of the channel.
     * @return the type of the channel (not null)
    public ChannelTypeUID getChannelTypeUID() {
        return this.channelTypeUID;
     * If no label is set, getLabel will return null and the default label for the {@link ChannelType} is used.
     * If no description is set, getDescription will return null and the default description for the {@link ChannelType}
     * Returns the properties for this {@link ChannelDefinition}
     * @return the unmodifiable properties for this {@link ChannelDefinition} (not null)
     * Returns the {@link AutoUpdatePolicy} to use for this channel.
     * @return the auto update policy
        return "ChannelDefinition [id=" + id + ", type=" + channelTypeUID + ", properties=" + properties + "]";
