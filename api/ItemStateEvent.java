 * {@link ItemStateEvent}s can be used to deliver item status updates through the openHAB event bus.
public class ItemStateEvent extends ItemEvent {
     * The item state event type.
    public static final String TYPE = ItemStateEvent.class.getSimpleName();
     * @param itemState the item state
    protected ItemStateEvent(String topic, String payload, String itemName, State itemState, @Nullable String source) {
        return String.format("Item '%s' shall update to %s", itemName, itemState);
