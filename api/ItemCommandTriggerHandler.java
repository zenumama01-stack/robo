import org.openhab.core.events.TopicEventFilter;
 * if an item receives a command. The eventType and command value can be set with the
public class ItemCommandTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    public static final String MODULE_TYPE_ID = "core.ItemCommandTrigger";
    public static final String CFG_ITEMNAME = "itemName";
    private final Logger logger = LoggerFactory.getLogger(ItemCommandTriggerHandler.class);
    private final EventFilter eventFilter;
    public ItemCommandTriggerHandler(Trigger module, String ruleUID, BundleContext bundleContext,
        this.itemName = (String) module.getConfiguration().get(CFG_ITEMNAME);
        boolean isWildcard = itemName.contains("?") || itemName.contains("*");
        if (isWildcard) {
            this.eventFilter = new TopicEventFilter(
                    "^openhab/items/" + itemName.replace("?", ".?").replace("*", ".*?") + "/.*$");
            this.types = Set.of(ItemCommandEvent.TYPE);
        if (!isWildcard && itemRegistry.get(itemName) == null) {
            logger.warn("Item '{}' needed for rule '{}' is missing. Trigger '{}' will not work.", itemName, ruleUID,
            if (itemName.equals(addedEvent.getItem().name)) {
                logger.info("Item '{}' needed for rule '{}' added. Trigger '{}' will now work.", itemName, ruleUID,
            if (itemName.equals(removedEvent.getItem().name)) {
                logger.warn("Item '{}' needed for rule '{}' removed. Trigger '{}' will no longer work.", itemName,
            if (event instanceof ItemCommandEvent commandEvent) {
                Command itemCommand = commandEvent.getItemCommand();
