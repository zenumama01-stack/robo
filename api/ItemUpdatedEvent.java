 * An {@link ItemUpdatedEvent} notifies subscribers that an item has been updated.
 * Item updated events must be created with the {@link ItemEventFactory}.
public class ItemUpdatedEvent extends AbstractItemRegistryEvent {
    private final ItemDTO oldItem;
     * The item updated event type.
    public static final String TYPE = ItemUpdatedEvent.class.getSimpleName();
     * Constructs a new item updated event object.
     * @param oldItem the old item data transfer object
    protected ItemUpdatedEvent(String topic, String payload, ItemDTO item, ItemDTO oldItem) {
        this.oldItem = oldItem;
     * Gets the old item.
     * @return the oldItem
    public ItemDTO getOldItem() {
        return "Item '" + getItem().name + "' has been updated.";
