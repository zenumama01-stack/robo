 * An {@link ItemAddedEvent} notifies subscribers that an item has been added.
 * Item added events must be created with the {@link ItemEventFactory}.
public class ItemAddedEvent extends AbstractItemRegistryEvent {
     * The item added event type.
    public static final String TYPE = ItemAddedEvent.class.getSimpleName();
     * Constructs a new item added event object.
    protected ItemAddedEvent(String topic, String payload, ItemDTO item) {
        super(topic, payload, null, item);
        return "Item '" + getItem().name + "' has been added.";
