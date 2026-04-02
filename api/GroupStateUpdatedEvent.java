 * {@link GroupStateUpdatedEvent}s can be used to deliver group item state updates through the openHAB event bus.
 * In contrast to the {@link GroupItemStateChangedEvent} it is always sent.
public class GroupStateUpdatedEvent extends ItemStateUpdatedEvent {
     * The group item state updated event type.
    public static final String TYPE = GroupStateUpdatedEvent.class.getSimpleName();
    protected GroupStateUpdatedEvent(String topic, String payload, String itemName, String memberName,
            State newItemState, @Nullable ZonedDateTime lastStateUpdate, @Nullable String source) {
        super(topic, payload, itemName, newItemState, lastStateUpdate, source);
     * @return the name of the updated group member
        return String.format("Group '%s' updated to %s through %s", itemName, itemState, memberName);
