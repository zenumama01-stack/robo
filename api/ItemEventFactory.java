 * An {@link ItemEventFactory} is responsible for creating item event instances, e.g. {@link ItemCommandEvent}s and
 * {@link ItemStateEvent}s.
public class ItemEventFactory extends AbstractEventFactory {
    private static final String CORE_LIBRARY_PACKAGE = "org.openhab.core.library.types.";
    private static final String ITEM_COMAND_EVENT_TOPIC = "openhab/items/{itemName}/command";
    private static final String ITEM_STATE_EVENT_TOPIC = "openhab/items/{itemName}/state";
    private static final String ITEM_STATE_UPDATED_EVENT_TOPIC = "openhab/items/{itemName}/stateupdated";
    private static final String ITEM_TIME_SERIES_EVENT_TOPIC = "openhab/items/{itemName}/timeseries";
    private static final String ITEM_TIME_SERIES_UPDATED_EVENT_TOPIC = "openhab/items/{itemName}/timeseriesupdated";
    private static final String ITEM_STATE_PREDICTED_EVENT_TOPIC = "openhab/items/{itemName}/statepredicted";
    private static final String ITEM_STATE_CHANGED_EVENT_TOPIC = "openhab/items/{itemName}/statechanged";
    private static final String GROUPITEM_STATE_UPDATED_EVENT_TOPIC = "openhab/items/{itemName}/{memberName}/stateupdated";
    private static final String GROUPITEM_STATE_CHANGED_EVENT_TOPIC = "openhab/items/{itemName}/{memberName}/statechanged";
    private static final String ITEM_ADDED_EVENT_TOPIC = "openhab/items/{itemName}/added";
    private static final String ITEM_REMOVED_EVENT_TOPIC = "openhab/items/{itemName}/removed";
    private static final String ITEM_UPDATED_EVENT_TOPIC = "openhab/items/{itemName}/updated";
     * Constructs a new ItemEventFactory.
    public ItemEventFactory() {
        super(Set.of(ItemCommandEvent.TYPE, ItemStateEvent.TYPE, ItemStatePredictedEvent.TYPE,
                ItemStateUpdatedEvent.TYPE, ItemStateChangedEvent.TYPE, ItemAddedEvent.TYPE, ItemUpdatedEvent.TYPE,
                ItemRemovedEvent.TYPE, GroupStateUpdatedEvent.TYPE, GroupItemStateChangedEvent.TYPE,
                ItemTimeSeriesEvent.TYPE, ItemTimeSeriesUpdatedEvent.TYPE));
        if (ItemCommandEvent.TYPE.equals(eventType)) {
            return createCommandEvent(topic, payload, source);
        } else if (ItemStateEvent.TYPE.equals(eventType)) {
            return createStateEvent(topic, payload, source);
        } else if (ItemStatePredictedEvent.TYPE.equals(eventType)) {
            return createStatePredictedEvent(topic, payload);
        } else if (ItemStateUpdatedEvent.TYPE.equals(eventType)) {
            return createStateUpdatedEvent(topic, payload);
        } else if (ItemStateChangedEvent.TYPE.equals(eventType)) {
            return createStateChangedEvent(topic, payload, source);
        } else if (ItemTimeSeriesEvent.TYPE.equals(eventType)) {
            return createTimeSeriesEvent(topic, payload);
        } else if (ItemTimeSeriesUpdatedEvent.TYPE.equals(eventType)) {
            return createTimeSeriesUpdatedEvent(topic, payload);
        } else if (ItemAddedEvent.TYPE.equals(eventType)) {
        } else if (ItemUpdatedEvent.TYPE.equals(eventType)) {
        } else if (ItemRemovedEvent.TYPE.equals(eventType)) {
        } else if (GroupStateUpdatedEvent.TYPE.equals(eventType)) {
            return createGroupStateUpdatedEvent(topic, payload);
        } else if (GroupItemStateChangedEvent.TYPE.equals(eventType)) {
            return createGroupStateChangedEvent(topic, payload);
    private Event createGroupStateUpdatedEvent(String topic, String payload) {
        String itemName = getItemName(topic);
        String memberName = getMemberName(topic);
        ItemStateUpdatedEventPayloadBean bean = deserializePayload(payload, ItemStateUpdatedEventPayloadBean.class);
        State state = getState(bean.getType(), bean.getValue());
        ZonedDateTime lastStateUpdate = bean.getLastStateUpdate();
        return new GroupStateUpdatedEvent(topic, payload, itemName, memberName, state, lastStateUpdate, null);
    private Event createGroupStateChangedEvent(String topic, String payload) {
        ItemStateChangedEventPayloadBean bean = deserializePayload(payload, ItemStateChangedEventPayloadBean.class);
        State oldState = getState(bean.getOldType(), bean.getOldValue());
        ZonedDateTime lastStateChange = bean.getLastStateChange();
        return new GroupItemStateChangedEvent(topic, payload, itemName, memberName, state, oldState, lastStateUpdate,
                lastStateChange);
    private Event createCommandEvent(String topic, String payload, @Nullable String source) {
        ItemEventPayloadBean bean = deserializePayload(payload, ItemEventPayloadBean.class);
        Command command = parseType(bean.getType(), bean.getValue(), Command.class);
        return new ItemCommandEvent(topic, payload, itemName, command, source);
    private Event createStateEvent(String topic, String payload, @Nullable String source) {
        return new ItemStateEvent(topic, payload, itemName, state, source);
    private Event createStatePredictedEvent(String topic, String payload) {
        ItemStatePredictedEventPayloadBean bean = deserializePayload(payload, ItemStatePredictedEventPayloadBean.class);
        State state = getState(bean.getPredictedType(), bean.getPredictedValue());
        return new ItemStatePredictedEvent(topic, payload, itemName, state, bean.isConfirmation());
    private Event createStateUpdatedEvent(String topic, String payload) {
        return new ItemStateUpdatedEvent(topic, payload, itemName, state, lastStateUpdate, null);
    private Event createStateChangedEvent(String topic, String payload, @Nullable String source) {
        return new ItemStateChangedEvent(topic, payload, itemName, state, oldState, lastStateUpdate, lastStateChange,
                source);
    private Event createTimeSeriesEvent(String topic, String payload) {
        ItemTimeSeriesEventPayloadBean bean = deserializePayload(payload, ItemTimeSeriesEventPayloadBean.class);
        TimeSeries timeSeries = bean.getTimeSeries();
        return new ItemTimeSeriesEvent(topic, payload, itemName, timeSeries, null);
    private Event createTimeSeriesUpdatedEvent(String topic, String payload) {
        return new ItemTimeSeriesUpdatedEvent(topic, payload, itemName, timeSeries, null);
    private State getState(String type, String value) {
        return parseType(type, value, State.class);
    private String getItemName(String topic) {
        if (topicElements.length < 4) {
    private String getMemberName(String topic) {
        if (topicElements.length < 5) {
        return topicElements[3];
    private static <T> T parseType(String typeName, String valueToParse, Class<T> desiredClass) {
        Object parsedObject;
        String simpleClassName = typeName + TYPE_POSTFIX;
        parsedObject = parseSimpleClassName(simpleClassName, valueToParse);
        if (parsedObject == null || !desiredClass.isAssignableFrom(parsedObject.getClass())) {
            String parsedObjectClassName = parsedObject != null ? parsedObject.getClass().getName() : "<undefined>";
            throw new IllegalArgumentException("Error parsing simpleClasssName '" + simpleClassName + "' with value '"
                    + valueToParse + "'. Desired type was '" + desiredClass.getName() + "' but got '"
                    + parsedObjectClassName + "'.");
        return desiredClass.cast(parsedObject);
    private static @Nullable Object parseSimpleClassName(String simpleClassName, String valueToParse) {
            return UnDefType.valueOf(valueToParse);
        if (simpleClassName.equals(RefreshType.class.getSimpleName())) {
            return RefreshType.valueOf(valueToParse);
            Class<?> stateClass = Class.forName(CORE_LIBRARY_PACKAGE + simpleClassName);
            Method valueOfMethod = stateClass.getMethod("valueOf", String.class);
            return valueOfMethod.invoke(stateClass, valueToParse);
            throw new IllegalArgumentException("Error getting class for simple name: '" + simpleClassName
                    + "' using package name '" + CORE_LIBRARY_PACKAGE + "'.", e);
        } catch (NoSuchMethodException | SecurityException e) {
                    "Error getting method #valueOf(String) of class '" + CORE_LIBRARY_PACKAGE + simpleClassName + "'.",
            throw new IllegalStateException("Error invoking #valueOf(String) on class '" + CORE_LIBRARY_PACKAGE
                    + simpleClassName + "' with value '" + valueToParse + "'.", e);
        ItemDTO itemDTO = deserializePayload(payload, ItemDTO.class);
        return new ItemAddedEvent(topic, payload, itemDTO);
        return new ItemRemovedEvent(topic, payload, itemDTO);
        ItemDTO[] itemDTOs = deserializePayload(payload, ItemDTO[].class);
        if (itemDTOs.length != 2) {
            throw new IllegalArgumentException("ItemUpdateEvent creation failed, invalid payload: " + payload);
        return new ItemUpdatedEvent(topic, payload, itemDTOs[0], itemDTOs[1]);
     * Creates an item command event.
     * @param itemName the name of the item to send the command for
     * @param source the name of the source identifying the sender (can be null)
     * @return the created item command event
     * @throws IllegalArgumentException if itemName or command is null
    public static ItemCommandEvent createCommandEvent(String itemName, Command command, @Nullable String source) {
        assertValidArguments(itemName, command, "command");
        String topic = buildTopic(ITEM_COMAND_EVENT_TOPIC, itemName);
        ItemEventPayloadBean bean = new ItemEventPayloadBean(getCommandType(command), command.toString());
    public static ItemCommandEvent createCommandEvent(String itemName, Command command) {
        return createCommandEvent(itemName, command, null);
     * Creates an item state event.
     * @param itemName the name of the item to send the state update for
     * @param state the new state to send
     * @return the created item state event
     * @throws IllegalArgumentException if itemName or state is null
    public static ItemStateEvent createStateEvent(String itemName, State state, @Nullable String source) {
        assertValidArguments(itemName, state, "state");
        String topic = buildTopic(ITEM_STATE_EVENT_TOPIC, itemName);
        ItemEventPayloadBean bean = new ItemEventPayloadBean(getStateType(state), state.toFullString());
    public static ItemEvent createStateEvent(String itemName, State state) {
        return createStateEvent(itemName, state, null);
     * Creates an item state updated event.
     * @param itemName the name of the item to report the state update for
     * @param lastStateUpdate the time of the last state update
     * @return the created item state update event
    public static ItemStateUpdatedEvent createStateUpdatedEvent(String itemName, State state,
            @Nullable ZonedDateTime lastStateUpdate) {
        return createStateUpdatedEvent(itemName, state, lastStateUpdate, null);
            @Nullable ZonedDateTime lastStateUpdate, @Nullable String source) {
        String topic = buildTopic(ITEM_STATE_UPDATED_EVENT_TOPIC, itemName);
        ItemStateUpdatedEventPayloadBean bean = new ItemStateUpdatedEventPayloadBean(getStateType(state),
                state.toFullString(), lastStateUpdate);
        return new ItemStateUpdatedEvent(topic, payload, itemName, state, lastStateUpdate, source);
    public static ItemTimeSeriesEvent createTimeSeriesEvent(String itemName, TimeSeries timeSeries,
        String topic = buildTopic(ITEM_TIME_SERIES_EVENT_TOPIC, itemName);
        ItemTimeSeriesEventPayloadBean bean = new ItemTimeSeriesEventPayloadBean(timeSeries);
        return new ItemTimeSeriesEvent(topic, payload, itemName, timeSeries, source);
    public static ItemTimeSeriesUpdatedEvent createTimeSeriesUpdatedEvent(String itemName, TimeSeries timeSeries,
        String topic = buildTopic(ITEM_TIME_SERIES_UPDATED_EVENT_TOPIC, itemName);
        return new ItemTimeSeriesUpdatedEvent(topic, payload, itemName, timeSeries, source);
     * Creates a group item state updated event.
     * @param groupName the name of the group to report the state update for
     * @param member the name of the item that updated the group state
     * @return the created group item state update event
     * @throws IllegalArgumentException if groupName or state is null
    public static GroupStateUpdatedEvent createGroupStateUpdatedEvent(String groupName, String member, State state,
        assertValidArguments(groupName, member, state, "state");
        String topic = buildGroupTopic(GROUPITEM_STATE_UPDATED_EVENT_TOPIC, groupName, member);
        return new GroupStateUpdatedEvent(topic, payload, groupName, member, state, lastStateUpdate, source);
     * Creates an item state predicted event.
     * @param state the predicted state to send
     * @param isConfirmation whether this is a confirmation of a previous state
     * @return the created item state predicted event
    public static ItemStatePredictedEvent createStatePredictedEvent(String itemName, State state,
            boolean isConfirmation) {
        String topic = buildTopic(ITEM_STATE_PREDICTED_EVENT_TOPIC, itemName);
        ItemStatePredictedEventPayloadBean bean = new ItemStatePredictedEventPayloadBean(getStateType(state),
                state.toFullString(), isConfirmation);
        return new ItemStatePredictedEvent(topic, payload, itemName, state, isConfirmation);
     * Creates an item state changed event.
     * @param itemName the name of the item to send the state changed event for
     * @param newState the new state to send
     * @param oldState the old state of the item
     * @param lastStateChange the time of the last state change
     * @return the created item state changed event
    public static ItemStateChangedEvent createStateChangedEvent(String itemName, State newState, State oldState,
            @Nullable ZonedDateTime lastStateUpdate, @Nullable ZonedDateTime lastStateChange, @Nullable String source) {
        assertValidArguments(itemName, newState, "state");
        String topic = buildTopic(ITEM_STATE_CHANGED_EVENT_TOPIC, itemName);
        ItemStateChangedEventPayloadBean bean = new ItemStateChangedEventPayloadBean(getStateType(newState),
                newState.toFullString(), getStateType(oldState), oldState.toFullString(), lastStateUpdate,
        return new ItemStateChangedEvent(topic, payload, itemName, newState, oldState, lastStateUpdate, lastStateChange,
        return createStateChangedEvent(itemName, newState, oldState, lastStateUpdate, lastStateChange, null);
     * Creates a group item state changed event.
     * @param itemName the name of the group item to send the state changed event for
     * @param memberName the name of the member causing the group item state change
     * @param oldState the old state of the group item
     * @return the created group item state changed event
    public static GroupItemStateChangedEvent createGroupStateChangedEvent(String itemName, String memberName,
            State newState, State oldState, @Nullable ZonedDateTime lastStateUpdate,
        assertValidArguments(itemName, memberName, newState, "state");
        String topic = buildGroupTopic(GROUPITEM_STATE_CHANGED_EVENT_TOPIC, itemName, memberName);
        return new GroupItemStateChangedEvent(topic, payload, itemName, memberName, newState, oldState, lastStateUpdate,
     * Creates an item added event.
     * @return the created item added event
     * @throws IllegalArgumentException if item is null
    public static ItemAddedEvent createAddedEvent(Item item) {
        assertValidArgument(item, "item");
        String topic = buildTopic(ITEM_ADDED_EVENT_TOPIC, item.getName());
        ItemDTO itemDTO = map(item);
        String payload = serializePayload(itemDTO);
     * Creates an item removed event.
     * @return the created item removed event
    public static ItemRemovedEvent createRemovedEvent(Item item) {
        String topic = buildTopic(ITEM_REMOVED_EVENT_TOPIC, item.getName());
     * Creates an item updated event.
     * @param oldItem the old item
     * @return the created item updated event
     * @throws IllegalArgumentException if item or oldItem is null
    public static ItemUpdatedEvent createUpdateEvent(Item item, Item oldItem) {
        assertValidArgument(oldItem, "oldItem");
        String topic = buildTopic(ITEM_UPDATED_EVENT_TOPIC, item.getName());
        ItemDTO oldItemDTO = map(oldItem);
        List<ItemDTO> itemDTOs = new LinkedList<>();
        itemDTOs.add(itemDTO);
        itemDTOs.add(oldItemDTO);
        String payload = serializePayload(itemDTOs);
        return new ItemUpdatedEvent(topic, payload, itemDTO, oldItemDTO);
    private static String buildTopic(String topic, String itemName) {
        return topic.replace("{itemName}", itemName);
    private static String buildGroupTopic(String topic, String itemName, String memberName) {
        return buildTopic(topic, itemName).replace("{memberName}", memberName);
    private static ItemDTO map(Item item) {
        return ItemDTOMapper.map(item);
        return stateClassName.substring(0, stateClassName.length() - TYPE_POSTFIX.length());
    private static String getCommandType(Command command) {
        String commandClassName = command.getClass().getSimpleName();
        return commandClassName.substring(0, commandClassName.length() - TYPE_POSTFIX.length());
    private static void assertValidArguments(String itemName, Type type, String typeArgumentName) {
        checkNotNullOrEmpty(itemName, "itemName");
        checkNotNull(type, typeArgumentName);
    private static void assertValidArguments(String itemName, String memberName, Type type, String typeArgumentName) {
        checkNotNullOrEmpty(memberName, "memberName");
    private static void assertValidArgument(Item item, String argumentName) {
        checkNotNull(item, argumentName);
     * This is a java bean that is used to serialize/deserialize item event payload.
        protected ItemEventPayloadBean() {
        public ItemEventPayloadBean(String type, String value) {
     * This is a java bean that is used to serialize/deserialize item state updated event payload.
    private static class ItemStateUpdatedEventPayloadBean {
        private @Nullable ZonedDateTime lastStateUpdate;
        protected ItemStateUpdatedEventPayloadBean() {
        public ItemStateUpdatedEventPayloadBean(String type, String value, @Nullable ZonedDateTime lastStateUpdate) {
     * This is a java bean that is used to serialize/deserialize item state changed event payload.
    private static class ItemStatePredictedEventPayloadBean {
        private @NonNullByDefault({}) String predictedType;
        private @NonNullByDefault({}) String predictedValue;
        private boolean isConfirmation;
        protected ItemStatePredictedEventPayloadBean() {
        public ItemStatePredictedEventPayloadBean(String predictedType, String predictedValue, boolean isConfirmation) {
            this.predictedType = predictedType;
            this.predictedValue = predictedValue;
            this.isConfirmation = isConfirmation;
        public String getPredictedType() {
            return predictedType;
        public String getPredictedValue() {
            return predictedValue;
        public boolean isConfirmation() {
            return isConfirmation;
    private static class ItemStateChangedEventPayloadBean {
        private @NonNullByDefault({}) String oldType;
        private @NonNullByDefault({}) String oldValue;
        private @Nullable ZonedDateTime lastStateChange;
        protected ItemStateChangedEventPayloadBean() {
        public ItemStateChangedEventPayloadBean(String type, String value, String oldType, String oldValue,
        public String getOldValue() {
        private @NonNullByDefault({}) List<TimeSeriesPayload> timeSeries;
        private ItemTimeSeriesEventPayloadBean() {
            // do not remove, GSON needs it
        public ItemTimeSeriesEventPayloadBean(TimeSeries timeSeries) {
            this.timeSeries = timeSeries.getStates().map(TimeSeriesPayload::new).toList();
            this.policy = timeSeries.getPolicy().name();
        public TimeSeries getTimeSeries() {
            TimeSeries timeSeries1 = new TimeSeries(TimeSeries.Policy.valueOf(policy));
            timeSeries.forEach(e -> {
                State state = parseType(e.getType(), e.getValue(), State.class);
                Instant instant = Instant.parse(e.getTimestamp());
                timeSeries1.add(instant, state);
            return timeSeries1;
            private TimeSeriesPayload() {
            public TimeSeriesPayload(TimeSeries.Entry entry) {
                type = getStateType(entry.state());
                value = entry.state().toFullString();
                timestamp = entry.timestamp().toString();
            public String getTimestamp() {
