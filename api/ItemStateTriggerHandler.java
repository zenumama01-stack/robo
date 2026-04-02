import org.openhab.core.items.events.GroupStateUpdatedEvent;
 * if an item state event occurs. The eventType and state value can be set with the
public class ItemStateTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    public static final String UPDATE_MODULE_TYPE_ID = "core.ItemStateUpdateTrigger";
    public static final String CHANGE_MODULE_TYPE_ID = "core.ItemStateChangeTrigger";
    private final Logger logger = LoggerFactory.getLogger(ItemStateTriggerHandler.class);
    private Set<String> types;
    public ItemStateTriggerHandler(Trigger module, String ruleUID, BundleContext bundleContext,
            this.types = isWildcard ? Set.of(ItemStateUpdatedEvent.TYPE, GroupStateUpdatedEvent.TYPE)
                    : Set.of(ItemStateUpdatedEvent.TYPE, GroupStateUpdatedEvent.TYPE, ItemAddedEvent.TYPE,
            this.types = isWildcard ? Set.of(ItemStateChangedEvent.TYPE, GroupItemStateChangedEvent.TYPE)
                    : Set.of(ItemStateChangedEvent.TYPE, GroupItemStateChangedEvent.TYPE, ItemAddedEvent.TYPE,
            if (event instanceof ItemStateUpdatedEvent updatedEvent
                    && UPDATE_MODULE_TYPE_ID.equals(module.getTypeUID())) {
                String state = this.state;
                State itemState = updatedEvent.getItemState();
                if ((state == null || state.equals(itemState.toFullString()))) {
                    values.put("state", itemState);
                    values.put("lastStateUpdate", updatedEvent.getLastStateUpdate());
            } else if (event instanceof ItemStateChangedEvent changedEvent
                State itemState = changedEvent.getItemState();
                State oldItemState = changedEvent.getOldItemState();
                if (stateMatches(this.state, itemState) && stateMatches(this.previousState, oldItemState)) {
                    values.put("oldState", oldItemState);
                    values.put("newState", itemState);
                    values.put("lastStateUpdate", changedEvent.getLastStateUpdate());
                    values.put("lastStateChange", changedEvent.getLastStateChange());
            if (!values.isEmpty()) {
