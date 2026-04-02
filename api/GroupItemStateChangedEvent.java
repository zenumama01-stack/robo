 * {@link GroupItemStateChangedEvent}s can be used to deliver group item state changes through the openHAB event bus. In
 * contrast to the {@link GroupStateUpdatedEvent} the {@link GroupItemStateChangedEvent} is only sent if the state
 * State events must be created with the {@link ItemEventFactory}.
public class GroupItemStateChangedEvent extends ItemStateChangedEvent {
     * The group item state changed event type.
    public static final String TYPE = GroupItemStateChangedEvent.class.getSimpleName();
    private final String memberName;
    protected GroupItemStateChangedEvent(String topic, String payload, String itemName, String memberName,
            State newItemState, State oldItemState, @Nullable ZonedDateTime lastStateUpdate,
            @Nullable ZonedDateTime lastStateChange) {
        super(topic, payload, itemName, newItemState, oldItemState, lastStateUpdate, lastStateChange, null);
        this.memberName = memberName;
     * @return the name of the changed group member
    public String getMemberName() {
        return this.memberName;
        String result = String.format("%s through %s", super.toString(), memberName);
        String source = getSource();
            result = String.format("%s (source: %s)", result, source);
