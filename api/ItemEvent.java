 * {@link ItemEvent} is an abstract super class for all command and state item events.
public abstract class ItemEvent extends AbstractEvent {
    protected final String itemName;
     * Constructs a new item state event.
    public ItemEvent(String topic, String payload, String itemName, @Nullable String source) {
     * Gets the item name.
