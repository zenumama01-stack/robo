package org.openhab.core.automation.module.script.internal.action;
import org.openhab.core.automation.module.script.action.BusEvent;
import org.openhab.core.items.events.ItemEventFactory;
import org.openhab.core.types.TypeParser;
 * The static methods of this class are made available as functions in the scripts.
 * This allows a script to write to the openHAB event bus.
 * @author Florian Hotze - Refactored to OSGi service
@Component(immediate = true, service = BusEvent.class)
public class BusEventImpl implements BusEvent {
    private static final String AUTOMATION_SOURCE = "org.openhab.core.automation.module.script";
    private final Logger logger = LoggerFactory.getLogger(BusEventImpl.class);
    private final EventPublisher publisher;
    public BusEventImpl(final @Reference ItemRegistry itemRegistry, final @Reference EventPublisher publisher) {
        this.publisher = publisher;
    public void sendCommand(Item item, String commandString) {
            sendCommand(item.getName(), commandString);
    public void sendCommand(Item item, String commandString, @Nullable String source) {
            sendCommand(item.getName(), commandString, source);
    public void sendCommand(Item item, Number command) {
        sendCommand(item, command, null);
    public void sendCommand(Item item, Number command, @Nullable String source) {
        if (item != null && command != null) {
            sendCommand(item.getName(), command.toString(), source);
    public void sendCommand(String itemName, String commandString) {
        sendCommand(itemName, commandString, null);
    public void sendCommand(String itemName, String commandString, @Nullable String source) {
            Item item = itemRegistry.getItem(itemName);
            Command command = TypeParser.parseCommand(item.getAcceptedCommandTypes(), commandString);
            if (command != null) {
                publisher.post(ItemEventFactory.createCommandEvent(itemName, command, buildSource(source)));
                logger.warn("Cannot convert '{}' to a command type which item '{}' accepts: {}.", commandString,
                        itemName, getAcceptedCommandNames(item));
        } catch (ItemNotFoundException e) {
            logger.warn("Item '{}' does not exist.", itemName);
    private static <T extends State> List<String> getAcceptedCommandNames(Item item) {
        return item.getAcceptedCommandTypes().stream().map(Class::getSimpleName).toList();
    public void sendCommand(Item item, Command command) {
    public void sendCommand(Item item, Command command, @Nullable String source) {
            publisher.post(ItemEventFactory.createCommandEvent(item.getName(), command, buildSource(source)));
    public void postUpdate(Item item, String stateString) {
        postUpdate(item, stateString, null);
    public void postUpdate(Item item, String stateString, @Nullable String source) {
            postUpdate(item.getName(), stateString, source);
    public void postUpdate(Item item, Number state) {
        postUpdate(item, state, null);
    public void postUpdate(Item item, Number state, @Nullable String source) {
        if (item != null && state != null) {
            postUpdate(item.getName(), state.toString(), source);
    private static <T extends State> List<String> getAcceptedDataTypeNames(Item item) {
        return item.getAcceptedDataTypes().stream().map(Class::getSimpleName).toList();
    public void postUpdate(String itemName, String stateString) {
        postUpdate(itemName, stateString, null);
    public void postUpdate(String itemName, String stateString, @Nullable String source) {
            State state = TypeParser.parseState(item.getAcceptedDataTypes(), stateString);
                publisher.post(ItemEventFactory.createStateEvent(itemName, state, buildSource(source)));
                logger.warn("Cannot convert '{}' to a state type which item '{}' accepts: {}.", stateString, itemName,
                        getAcceptedDataTypeNames(item));
    public void postUpdate(Item item, State state) {
    public void postUpdate(Item item, State state, @Nullable String source) {
            publisher.post(ItemEventFactory.createStateEvent(item.getName(), state, buildSource(source)));
    public void sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries) {
        sendTimeSeries(item, timeSeries, null);
    public void sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries, @Nullable String source) {
        if (item != null && timeSeries != null) {
            publisher.post(ItemEventFactory.createTimeSeriesEvent(item.getName(), timeSeries, buildSource(source)));
    public void sendTimeSeries(@Nullable String itemName, @Nullable Map<ZonedDateTime, State> values, String policy) {
        sendTimeSeries(itemName, values, policy, null);
    public void sendTimeSeries(@Nullable String itemName, @Nullable Map<ZonedDateTime, State> values, String policy,
            @Nullable String source) {
        if (itemName != null && values != null) {
                TimeSeries timeSeries = new TimeSeries(TimeSeries.Policy.valueOf(policy));
                values.forEach((key, value) -> timeSeries.add(key.toInstant(), value));
                publisher.post(ItemEventFactory.createTimeSeriesEvent(itemName, timeSeries, buildSource(source)));
                logger.warn("Policy '{}' does not exist.", policy);
    public Map<Item, State> storeStates(Item... items) {
        Map<Item, State> statesMap = new HashMap<>();
        if (items != null) {
            for (Item item : items) {
                if (item instanceof GroupItem groupItem) {
                    for (Item member : groupItem.getAllMembers()) {
                        statesMap.put(member, member.getState());
                    statesMap.put(item, item.getState());
        return statesMap;
    public void restoreStates(Map<Item, State> statesMap) {
        if (statesMap != null) {
            for (Map.Entry<Item, State> entry : statesMap.entrySet()) {
                if (entry.getValue() instanceof Command) {
                    sendCommand(entry.getKey(), (Command) entry.getValue());
                    postUpdate(entry.getKey(), entry.getValue());
    private String buildSource(@Nullable String source) {
        return Objects.requireNonNullElse(source, AUTOMATION_SOURCE);
