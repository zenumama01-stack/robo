 * An {@link ItemRemovedEvent} notifies subscribers that an item has been removed.
 * Item removed events must be created with the {@link ItemEventFactory}.
public class ItemRemovedEvent extends AbstractItemRegistryEvent {
     * The item removed event type.
    public static final String TYPE = ItemRemovedEvent.class.getSimpleName();
     * Constructs a new item removed event object.
    protected ItemRemovedEvent(String topic, String payload, ItemDTO item) {
        return "Item '" + getItem().name + "' has been removed.";
