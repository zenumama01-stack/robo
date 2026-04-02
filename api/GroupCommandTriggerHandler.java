import org.openhab.core.items.events.ItemAddedEvent;
import org.openhab.core.items.events.ItemCommandEvent;
import org.openhab.core.items.events.ItemRemovedEvent;
 * if a member of an item group receives a command.
 * The group name and command value can be set with the configuration.
public class GroupCommandTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    private final Logger logger = LoggerFactory.getLogger(GroupCommandTriggerHandler.class);
    private final String groupName;
    private final @Nullable String command;
    public static final String MODULE_TYPE_ID = "core.GroupCommandTrigger";
    public static final String CFG_GROUPNAME = "groupName";
    public static final String CFG_COMMAND = "command";
    public GroupCommandTriggerHandler(Trigger module, String ruleUID, BundleContext bundleContext,
            ItemRegistry itemRegistry) {
        this.groupName = ConfigParser.valueAsOrElse(module.getConfiguration().get(CFG_GROUPNAME), String.class, "");
        if (this.groupName.isBlank()) {
            logger.warn("GroupCommandTrigger {} of rule {} has no groupName configured and will not work.",
        this.command = (String) module.getConfiguration().get(CFG_COMMAND);
        this.types = Set.of(ItemCommandEvent.TYPE, ItemAddedEvent.TYPE, ItemRemovedEvent.TYPE);
        if (itemRegistry.get(groupName) == null) {
            logger.warn("Group '{}' needed for rule '{}' is missing. Trigger '{}' will not work.", groupName, ruleUID,
        if (event instanceof ItemAddedEvent addedEvent) {
            if (groupName.equals(addedEvent.getItem().name)) {
                logger.info("Group '{}' needed for rule '{}' added. Trigger '{}' will now work.", groupName, ruleUID,
        } else if (event instanceof ItemRemovedEvent removedEvent) {
            if (groupName.equals(removedEvent.getItem().name)) {
                logger.warn("Group '{}' needed for rule '{}' removed. Trigger '{}' will no longer work.", groupName,
                        ruleUID, module.getId());
        if (callback instanceof TriggerHandlerCallback cb) {
            if (event instanceof ItemCommandEvent icEvent) {
                String itemName = icEvent.getItemName();
                Item item = itemRegistry.get(itemName);
                Item group = itemRegistry.get(groupName);
                if (item != null && item.getGroupNames().contains(groupName)) {
                    String command = this.command;
                    Command itemCommand = icEvent.getItemCommand();
                    if (command == null || command.equals(itemCommand.toFullString())) {
                        if (group != null) {
                            values.put("triggeringGroup", group);
                        values.put("triggeringItem", item);
                        values.put("command", itemCommand);
                        cb.triggered(this.module, values);
