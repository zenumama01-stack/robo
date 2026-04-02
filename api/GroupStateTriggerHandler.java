import org.openhab.core.items.events.GroupItemStateChangedEvent;
import org.openhab.core.items.events.ItemStateUpdatedEvent;
 * if state event of a member of an item group occurs.
 * The group name and state value can be set with the configuration.
public class GroupStateTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    public static final String UPDATE_MODULE_TYPE_ID = "core.GroupStateUpdateTrigger";
    public static final String CHANGE_MODULE_TYPE_ID = "core.GroupStateChangeTrigger";
    public static final String CFG_STATE = "state";
    public static final String CFG_PREVIOUS_STATE = "previousState";
    private final Logger logger = LoggerFactory.getLogger(GroupStateTriggerHandler.class);
    private final @Nullable String state;
    private final String previousState;
    public GroupStateTriggerHandler(Trigger module, String ruleUID, BundleContext bundleContext,
            logger.warn("GroupStateTrigger {} of rule {} has no groupName configured and will not work.",
        this.state = (String) module.getConfiguration().get(CFG_STATE);
        this.previousState = (String) module.getConfiguration().get(CFG_PREVIOUS_STATE);
        if (UPDATE_MODULE_TYPE_ID.equals(module.getTypeUID())) {
            this.types = Set.of(ItemStateUpdatedEvent.TYPE, ItemAddedEvent.TYPE, ItemRemovedEvent.TYPE);
            this.types = Set.of(ItemStateChangedEvent.TYPE, GroupItemStateChangedEvent.TYPE, ItemAddedEvent.TYPE,
                    ItemRemovedEvent.TYPE);
            if (event instanceof ItemStateUpdatedEvent isEvent && UPDATE_MODULE_TYPE_ID.equals(module.getTypeUID())) {
                String itemName = isEvent.getItemName();
                    State state = isEvent.getItemState();
                    if ((this.state == null || state.toFullString().equals(this.state))) {
                        Map<String, @Nullable Object> values = new HashMap<>();
                        values.put("state", state);
                        values.put("lastStateUpdate", isEvent.getLastStateUpdate());
            } else if (event instanceof ItemStateChangedEvent iscEvent
                    && CHANGE_MODULE_TYPE_ID.equals(module.getTypeUID())) {
                String itemName = iscEvent.getItemName();
                    State state = iscEvent.getItemState();
                    State oldState = iscEvent.getOldItemState();
                    if (stateMatches(this.state, state) && stateMatches(this.previousState, oldState)) {
                        values.put("oldState", oldState);
                        values.put("newState", state);
                        values.put("lastStateUpdate", iscEvent.getLastStateUpdate());
                        values.put("lastStateChange", iscEvent.getLastStateChange());
    private boolean stateMatches(@Nullable String requiredState, State state) {
        if (requiredState == null) {
        String reqState = requiredState.trim();
        return reqState.isEmpty() || reqState.equals(state.toFullString());
