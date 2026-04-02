import org.openhab.core.items.events.ItemStateEvent;
 * This is an implementation of an ActionHandler. It sends state updates for items.
public class ItemStateUpdateActionHandler extends BaseActionModuleHandler {
    public static final String ITEM_STATE_UPDATE_ACTION = "core.ItemStateUpdateAction";
    private final Logger logger = LoggerFactory.getLogger(ItemStateUpdateActionHandler.class);
    public ItemStateUpdateActionHandler(Action module, EventPublisher eventPublisher, ItemRegistry itemRegistry) {
        Configuration config = module.getConfiguration();
        String itemName = (String) config.get(ITEM_NAME);
        String state = (String) config.get(STATE);
                State stateObj = null;
                    stateObj = TypeParser.parseState(item.getAcceptedDataTypes(), state);
                    final Object st = inputs.get(STATE);
                    if (st instanceof State state1) {
                        if (item.getAcceptedDataTypes().contains(st.getClass())) {
                            stateObj = state1;
                if (stateObj != null) {
                    final ItemStateEvent itemStateEvent = (ItemStateEvent) ItemEventFactory.createStateEvent(itemName,
                            stateObj);
                    logger.debug("Executing ItemStateEvent on Item {} with State {}", itemStateEvent.getItemName(),
                            itemStateEvent.getItemState());
                    eventPublisher.post(itemStateEvent);
                    logger.debug("State '{}' is not valid for item '{}'.", state, itemName);
                    "Item state was not updated because the configuration was not correct: ItemName: {}, State: {}",
                    itemName, state);
