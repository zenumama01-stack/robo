 * Abstract implementation of an item registry event which will be posted by the
 * {@link org.openhab.core.items.ItemRegistry} for added, removed
public abstract class AbstractItemRegistryEvent extends AbstractEvent {
    private final ItemDTO item;
     * Must be called in subclass constructor to create a new item registry event.
     * @param item the item data transfer object
    protected AbstractItemRegistryEvent(String topic, String payload, @Nullable String source, ItemDTO item) {
     * Gets the item.
     * @return the item
    public ItemDTO getItem() {
