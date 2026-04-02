 * {@link ItemStateChangedEvent}s can be used to deliver item state changes through the openHAB event bus. In contrast
 * to the {@link ItemStateEvent} the {@link ItemStateChangedEvent} is only sent if the state changed. State events must
 * be created with the {@link ItemEventFactory}.
public class ItemStateChangedEvent extends ItemEvent {
     * The item state changed event type.
    public static final String TYPE = ItemStateChangedEvent.class.getSimpleName();
    protected final State itemState;
    protected final State oldItemState;
    protected final @Nullable ZonedDateTime lastStateUpdate;
    protected final @Nullable ZonedDateTime lastStateChange;
     * Constructs a new item state changed event.
     * @param newItemState the new item state
     * @param oldItemState the old item state
     * @param lastStateUpdate the last state update
     * @param lastStateChange the last state change
    protected ItemStateChangedEvent(String topic, String payload, String itemName, State newItemState,
            State oldItemState, @Nullable ZonedDateTime lastStateUpdate, @Nullable ZonedDateTime lastStateChange,
        this.itemState = newItemState;
        this.oldItemState = oldItemState;
     * Gets the item state.
     * @return the item state
    public State getItemState() {
        return itemState;
     * Gets the old item state.
     * @return the old item state
    public State getOldItemState() {
        return oldItemState;
     * Gets the timestamp of the previous state update that occurred prior to this event.
     * @return the last state update
     * Gets the timestamp of the previous state change that occurred prior to this event.
     * @return the last state change
        String result = String.format("Item '%s' changed from %s to %s", itemName, oldItemState, itemState);
