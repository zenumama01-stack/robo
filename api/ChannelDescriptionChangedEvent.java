 * {@link ChannelDescriptionChangedEvent}s will be delivered through the openHAB event bus if the
 * {@link CommandDescription} or {@link StateDescription} of a channel has changed. Instances must be created with the
 * {@link ThingEventFactory}.
public class ChannelDescriptionChangedEvent extends AbstractEvent {
    public enum CommonChannelDescriptionField {
        ALL,
        COMMAND_OPTIONS,
        PATTERN,
        STATE_OPTIONS
     * The channel description changed event type.
    public static final String TYPE = ChannelDescriptionChangedEvent.class.getSimpleName();
     * The changed field.
    private CommonChannelDescriptionField field;
     * The channel which triggered the event.
     * A {@link Set} of linked item names.
    private final Set<String> linkedItemNames;
     * The new value (represented as JSON string).
     * The old value (represented as JSON string).
    private final @Nullable String oldValue;
     * @param field the changed field
     * @param linkedItemNames a {@link Set} of linked item names
     * @param value the new value (represented as JSON string)
     * @param oldValue the old value represented as JSON string)
    protected ChannelDescriptionChangedEvent(String topic, String payload, CommonChannelDescriptionField field,
            ChannelUID channelUID, Set<String> linkedItemNames, String value, @Nullable String oldValue) {
        this.field = field;
        this.linkedItemNames = linkedItemNames;
     * Gets the changed field.
     * @return the changed field
    public CommonChannelDescriptionField getField() {
     * Gets the {@link ChannelUID}.
     * @return the {@link ChannelUID}
    public ChannelUID getChannelUID() {
        return channelUID;
     * Gets the linked item names.
     * @return a {@link Set} of linked item names
    public Set<String> getLinkedItemNames() {
        return linkedItemNames;
     * Gets the new value (represented as JSON string).
     * @return the new value.
     * Gets the old value (represented as JSON string).
     * @return the old value.
    public @Nullable String getOldValue() {
                "Description for field '%s' of channel '%s' changed from '%s' to '%s' for linked items: %s", field,
                channelUID, oldValue, value, linkedItemNames.stream().collect(Collectors.joining(",", "[", "]")));
