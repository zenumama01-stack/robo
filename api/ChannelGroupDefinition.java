 * The {@link ChannelGroupDefinition} class defines a {@link ChannelGroupType} of a {@link ThingType}.
 * A {@link ChannelGroupType} is part of a {@link Thing} that represents a set of channels (functionalities) of it.
public class ChannelGroupDefinition {
    private ChannelGroupTypeUID typeUID;
     * @param id the identifier of the channel group (must not be empty)
     * @param typeUID the type UID of the channel group
     * @param label the label for the channel group to override ChannelGroupType
     * @param description the description for the channel group to override ChannelGroupType
     * @throws IllegalArgumentException if the ID is empty
    public ChannelGroupDefinition(String id, ChannelGroupTypeUID typeUID, @Nullable String label,
            @Nullable String description) throws IllegalArgumentException {
            throw new IllegalArgumentException("The ID must not be empty!");
    public ChannelGroupDefinition(String id, ChannelGroupTypeUID typeUID) throws IllegalArgumentException {
        this(id, typeUID, null, null);
     * Returns the identifier of the channel group.
     * @return the identifier of the channel group (not empty)
     * Returns the type UID of the channel group.
     * @return the type UID of the channel group
    public ChannelGroupTypeUID getTypeUID() {
     * If no label is set, getLabel will return null and the default label for the {@link ChannelGroupType} is used.
     * @return the label for the channel group.
     * If no description is set, getDescription will return null and the default description for the
     * {@link ChannelGroupType} is used.
     * @return the description for the channel group.
        return "ChannelGroupDefinition [id=" + id + ", typeUID=" + typeUID + "]";
