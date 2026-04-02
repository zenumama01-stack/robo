 * {@link ItemStateUpdatedEvent}s can be used to report item status updates through the openHAB event bus.
 * State update events must be created with the {@link ItemEventFactory}.
public class ItemStateUpdatedEvent extends ItemEvent {
    public static final String TYPE = ItemStateUpdatedEvent.class.getSimpleName();
    protected ItemStateUpdatedEvent(String topic, String payload, String itemName, State itemState,
        return String.format("Item '%s' updated to %s", itemName, itemState);
