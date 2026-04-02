 * {@link ItemCommandEvent}s can be used to deliver commands through the openHAB event bus.
 * Command events must be created with the {@link ItemEventFactory}.
public class ItemCommandEvent extends ItemEvent {
     * The item command event type.
    public static final String TYPE = ItemCommandEvent.class.getSimpleName();
    private final Command command;
     * Constructs a new item command event object.
    protected ItemCommandEvent(String topic, String payload, String itemName, Command command,
        super(topic, payload, itemName, source);
     * Gets the item command.
     * @return the item command
    public Command getItemCommand() {
        String result = String.format("Item '%s' received command %s", itemName, command);
