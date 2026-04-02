 * @author Stefan Bußweiler - Initial contribution ({@link org.openhab.core.items.events.ItemEventFactory})
public class ItemEventUtility {
    private static final Pattern TOPIC_PATTERN = Pattern.compile("openhab/items/(?<entity>\\w+)/(?<action>\\w+)");
    private static final String TYPE_POSTFIX = "Type";
    public ItemEventUtility(Gson gson, ItemRegistry itemRegistry) {
    public Event createCommandEvent(EventDTO eventDTO) throws EventProcessingException {
        Matcher matcher = getTopicMatcher(eventDTO.topic, "command");
        Item item = getItem(matcher.group("entity"));
        Type command = parseType(eventDTO.payload);
        if (command instanceof Command command1) {
            List<Class<? extends Command>> acceptedItemCommandTypes = item.getAcceptedCommandTypes();
            if (acceptedItemCommandTypes.contains(command.getClass())) {
                return ItemEventFactory.createCommandEvent(item.getName(), command1, eventDTO.source);
        throw new EventProcessingException("Incompatible datatype, rejected.");
    public Event createStateEvent(EventDTO eventDTO) throws EventProcessingException {
        Matcher matcher = getTopicMatcher(eventDTO.topic, "state");
        Type state = parseType(eventDTO.payload);
        if (state instanceof State state1) {
            List<Class<? extends State>> acceptedItemStateTypes = item.getAcceptedDataTypes();
            if (acceptedItemStateTypes.contains(state.getClass())) {
                return ItemEventFactory.createStateEvent(item.getName(), state1, eventDTO.source);
    public Event createTimeSeriesEvent(EventDTO eventDTO) throws EventProcessingException {
        Matcher matcher = getTopicMatcher(eventDTO.topic, "timeseries");
        TimeSeries timeSeries = parseTimeSeries(eventDTO.payload);
        return ItemEventFactory.createTimeSeriesEvent(item.getName(), timeSeries, eventDTO.source);
    private Matcher getTopicMatcher(@Nullable String topic, String action) throws EventProcessingException {
        if (topic == null) {
            throw new EventProcessingException("Topic must not be null");
        Matcher matcher = TOPIC_PATTERN.matcher(topic);
            throw new EventProcessingException(
                    "Topic must follow the format {namespace}/{entityType}/{entity}/{action}.");
        if (!action.equals(matcher.group("action"))) {
            throw new EventProcessingException("Topic does not match event type.");
        return matcher;
    private Item getItem(String itemName) throws EventProcessingException {
            return itemRegistry.getItem(itemName);
            throw new EventProcessingException("Could not find item '" + itemName + "' in registry.");
    private TimeSeries parseTimeSeries(@Nullable String payload) throws EventProcessingException {
        ItemTimeSeriesEventPayloadBean bean = null;
            bean = gson.fromJson(payload, ItemTimeSeriesEventPayloadBean.class);
        } catch (JsonParseException ignored) {
            throw new EventProcessingException("Failed to deserialize payload '" + payload + "'.");
        TimeSeries timeSeries = new TimeSeries(TimeSeries.Policy.valueOf(bean.policy));
        for (ItemTimeSeriesEventPayloadBean.TimeSeriesPayload timeSeriesPayload : bean.timeSeries) {
            Type value = parseType(timeSeriesPayload.type, timeSeriesPayload.value);
            if (value instanceof State state) {
                    Instant timestamp = Instant.parse(timeSeriesPayload.timestamp);
                    timeSeries.add(timestamp, state);
                            "Could not parse '" + timeSeriesPayload.timestamp + "' to an instant.");
                throw new EventProcessingException("Only states are allowed in timeseries events.");
    private Type parseType(@Nullable String payload) throws EventProcessingException {
        ItemEventPayloadBean bean = null;
            bean = gson.fromJson(payload, ItemEventPayloadBean.class);
        return parseType(bean.type, bean.value);
    private Type parseType(String type, String value) throws EventProcessingException {
        String simpleClassName = type + TYPE_POSTFIX;
        Type returnType;
        if (simpleClassName.equals(UnDefType.class.getSimpleName())) {
            returnType = UnDefType.valueOf(value);
        } else if (simpleClassName.equals(RefreshType.class.getSimpleName())) {
            returnType = RefreshType.valueOf(value);
            returnType = TypeParser.parseType(simpleClassName, value);
        if (returnType == null) {
                    "Error parsing simpleClassName '" + simpleClassName + "' with value '" + value + "'.");
    private static class ItemEventPayloadBean {
        public @NonNullByDefault({}) String type;
        public @NonNullByDefault({}) String value;
    private static class ItemTimeSeriesEventPayloadBean {
        private @NonNullByDefault({}) List<ItemTimeSeriesEventPayloadBean.TimeSeriesPayload> timeSeries;
        private @NonNullByDefault({}) String policy;
        private static class TimeSeriesPayload {
            private @NonNullByDefault({}) String timestamp;
