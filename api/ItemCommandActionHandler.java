 * This is an implementation of an ActionHandler. It sends commands for items.
 * @author Stefan Triller - use command from input first and if not set, use command from configuration
public class ItemCommandActionHandler extends BaseActionModuleHandler {
    public static final String ITEM_COMMAND_ACTION = "core.ItemCommandAction";
    public static final String ITEM_NAME = "itemName";
    public static final String COMMAND = "command";
    private final Logger logger = LoggerFactory.getLogger(ItemCommandActionHandler.class);
     * constructs a new ItemCommandActionHandler
    public ItemCommandActionHandler(Action module, EventPublisher eventPublisher, ItemRegistry itemRegistry) {
    public @Nullable Map<String, @Nullable Object> execute(Map<String, Object> inputs) {
        String itemName = (String) module.getConfiguration().get(ITEM_NAME);
        String command = (String) module.getConfiguration().get(COMMAND);
        if (itemName != null) {
                Command commandObj = null;
                    commandObj = TypeParser.parseCommand(item.getAcceptedCommandTypes(), command);
                    Object cmd = inputs.get(COMMAND);
                    if (cmd instanceof Command command1) {
                        if (item.getAcceptedCommandTypes().contains(cmd.getClass())) {
                            commandObj = command1;
                if (commandObj != null) {
                    ItemCommandEvent itemCommandEvent = ItemEventFactory.createCommandEvent(itemName, commandObj);
                    logger.debug("Executing ItemCommandAction on Item {} with Command {}",
                            itemCommandEvent.getItemName(), itemCommandEvent.getItemCommand());
                    eventPublisher.post(itemCommandEvent);
                    logger.debug("Command '{}' is not valid for item '{}'.", command, itemName);
                logger.error("Item with name {} not found in ItemRegistry.", itemName);
                    "Command was not posted because either the configuration was not correct or a service was missing: ItemName: {}, Command: {}, eventPublisher: {}, ItemRegistry: {}",
                    itemName, command, eventPublisher, itemRegistry);
