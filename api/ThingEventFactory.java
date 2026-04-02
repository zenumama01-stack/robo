import org.openhab.core.thing.events.ChannelDescriptionChangedEvent.CommonChannelDescriptionField;
 * A {@link ThingEventFactory} is responsible for creating thing event instances:
 * <li>{@link ThingStatusInfoEvent#TYPE}</li>
 * @author Dennis Nobel - Added status changed event
 * @author Christoph Weitkamp - Added ChannelDescriptionChangedEvent
public class ThingEventFactory extends AbstractEventFactory {
    static final String THING_SOURCE = "org.openhab.core.thing";
    static final String THING_STATUS_INFO_EVENT_TOPIC = "openhab/things/{thingUID}/status";
    static final String THING_STATUS_INFO_CHANGED_EVENT_TOPIC = "openhab/things/{thingUID}/statuschanged";
    static final String THING_ADDED_EVENT_TOPIC = "openhab/things/{thingUID}/added";
    static final String THING_REMOVED_EVENT_TOPIC = "openhab/things/{thingUID}/removed";
    static final String THING_UPDATED_EVENT_TOPIC = "openhab/things/{thingUID}/updated";
    static final String CHANNEL_DESCRIPTION_CHANGED_TOPIC = "openhab/channels/{channelUID}/descriptionchanged";
    static final String CHANNEL_TRIGGERED_EVENT_TOPIC = "openhab/channels/{channelUID}/triggered";
     * Constructs a new ThingEventFactory.
    public ThingEventFactory() {
        super(Set.of(ThingStatusInfoEvent.TYPE, ThingStatusInfoChangedEvent.TYPE, ThingAddedEvent.TYPE,
                ThingRemovedEvent.TYPE, ThingUpdatedEvent.TYPE, ChannelDescriptionChangedEvent.TYPE,
                ChannelTriggeredEvent.TYPE));
        if (ThingStatusInfoEvent.TYPE.equals(eventType)) {
        } else if (ThingStatusInfoChangedEvent.TYPE.equals(eventType)) {
            return createStatusInfoChangedEvent(topic, payload);
        } else if (ThingAddedEvent.TYPE.equals(eventType)) {
        } else if (ThingRemovedEvent.TYPE.equals(eventType)) {
        } else if (ThingUpdatedEvent.TYPE.equals(eventType)) {
        } else if (ChannelDescriptionChangedEvent.TYPE.equals(eventType)) {
            return createChannelDescriptionChangedEvent(topic, payload);
        } else if (ChannelTriggeredEvent.TYPE.equals(eventType)) {
            return createTriggerEvent(topic, payload, source);
    public static class ChannelDescriptionChangedEventPayloadBean {
        public @NonNullByDefault({}) CommonChannelDescriptionField field;
        public @NonNullByDefault({}) String channelUID;
        public Set<String> linkedItemNames = Set.of();
        public @Nullable String oldValue;
        protected ChannelDescriptionChangedEventPayloadBean() {
        public ChannelDescriptionChangedEventPayloadBean(CommonChannelDescriptionField field, String channelUID,
                Set<String> linkedItemNames, String value, @Nullable String oldValue) {
    public interface CommonChannelDescriptionFieldPayloadBean {
    public static class ChannelDescriptionPatternPayloadBean implements CommonChannelDescriptionFieldPayloadBean {
        public @NonNullByDefault({}) String pattern;
        protected ChannelDescriptionPatternPayloadBean() {
        public ChannelDescriptionPatternPayloadBean(String pattern) {
    public static class ChannelDescriptionStateOptionsPayloadBean implements CommonChannelDescriptionFieldPayloadBean {
        public @NonNullByDefault({}) List<StateOption> options;
        protected ChannelDescriptionStateOptionsPayloadBean() {
        public ChannelDescriptionStateOptionsPayloadBean(List<StateOption> options) {
    public static class ChannelDescriptionCommandOptionsPayloadBean
            implements CommonChannelDescriptionFieldPayloadBean {
        public @NonNullByDefault({}) List<CommandOption> options;
        protected ChannelDescriptionCommandOptionsPayloadBean() {
        public ChannelDescriptionCommandOptionsPayloadBean(List<CommandOption> options) {
     * Creates a {@link ChannelDescriptionChangedEvent} for a changed {@link StateDescription}. New and optional old
     * value will be serialized to a JSON string from the {@link StateDescriptionFragment} object.
     * @param stateDescription the new {@link StateDescriptionFragment}
     * @param oldStateDescription the old {@link StateDescriptionFragment}
     * @return Created {@link ChannelDescriptionChangedEvent}
    public static ChannelDescriptionChangedEvent createChannelDescriptionChangedEvent(ChannelUID channelUID,
            Set<String> linkedItemNames, StateDescriptionFragment stateDescription,
            @Nullable StateDescriptionFragment oldStateDescription) {
        checkNotNull(linkedItemNames, "linkedItemNames");
        checkNotNull(channelUID, "channelUID");
        checkNotNull(stateDescription, "stateDescription");
        String stateDescriptionPayload = serializePayload(stateDescription);
        String oldStateDescriptionPayload = oldStateDescription != null ? serializePayload(oldStateDescription) : null;
        ChannelDescriptionChangedEventPayloadBean bean = new ChannelDescriptionChangedEventPayloadBean(
                CommonChannelDescriptionField.ALL, channelUID.getAsString(), linkedItemNames, stateDescriptionPayload,
                oldStateDescriptionPayload);
        String payload = serializePayload(bean);
        String topic = buildTopic(CHANNEL_DESCRIPTION_CHANGED_TOPIC, channelUID);
        return new ChannelDescriptionChangedEvent(topic, payload, CommonChannelDescriptionField.ALL, channelUID,
                linkedItemNames, stateDescriptionPayload, oldStateDescriptionPayload);
     * Creates a {@link ChannelDescriptionChangedEvent} for a changed pattern. New and optional old value will be
     * serialized to a JSON string from the {@link ChannelDescriptionPatternPayloadBean} object.
     * @param pattern the new pattern
     * @param oldPattern the old pattern
    public static ChannelDescriptionChangedEvent createChannelDescriptionPatternChangedEvent(ChannelUID channelUID,
            Set<String> linkedItemNames, String pattern, @Nullable String oldPattern) {
        checkNotNull(pattern, "pattern");
        String patternPayload = serializePayload(new ChannelDescriptionPatternPayloadBean(pattern));
        String oldPatternPayload = oldPattern != null
                ? serializePayload(new ChannelDescriptionPatternPayloadBean(oldPattern))
                CommonChannelDescriptionField.PATTERN, channelUID.getAsString(), linkedItemNames, patternPayload,
                oldPatternPayload);
        return new ChannelDescriptionChangedEvent(topic, payload, CommonChannelDescriptionField.PATTERN, channelUID,
                linkedItemNames, patternPayload, oldPatternPayload);
     * Creates a {@link ChannelDescriptionChangedEvent} for changed {@link StateOption}s. New and optional old value
     * will be serialized to a JSON string from the {@link ChannelDescriptionStateOptionsPayloadBean} object.
     * @param options the new {@link StateOption}s
     * @param oldOptions the old {@link StateOption}s
    public static ChannelDescriptionChangedEvent createChannelDescriptionStateOptionsChangedEvent(ChannelUID channelUID,
            Set<String> linkedItemNames, List<StateOption> options, @Nullable List<StateOption> oldOptions) {
        checkNotNull(options, "options");
        String stateOptionsPayload = serializePayload(new ChannelDescriptionStateOptionsPayloadBean(options));
        String oldStateOptionsPayload = oldOptions != null
                ? serializePayload(new ChannelDescriptionStateOptionsPayloadBean(oldOptions))
                CommonChannelDescriptionField.STATE_OPTIONS, channelUID.getAsString(), linkedItemNames,
                stateOptionsPayload, oldStateOptionsPayload);
        return new ChannelDescriptionChangedEvent(topic, payload, CommonChannelDescriptionField.STATE_OPTIONS,
                channelUID, linkedItemNames, stateOptionsPayload, oldStateOptionsPayload);
     * Creates a {@link ChannelDescriptionChangedEvent} for change {@link CommandOption}s. New and optional old value
     * will be serialized to a JSON string from the {@link ChannelDescriptionCommandOptionsPayloadBean} object.
     * @param options the new {@link CommandOption}s
     * @param oldOptions the old {@link CommandOption}s
    public static ChannelDescriptionChangedEvent createChannelDescriptionCommandOptionsChangedEvent(
            ChannelUID channelUID, Set<String> linkedItemNames, List<CommandOption> options,
            @Nullable List<CommandOption> oldOptions) {
        String commandOptionsPayload = serializePayload(new ChannelDescriptionCommandOptionsPayloadBean(options));
        String oldCommandOptionsPayload = oldOptions != null
                ? serializePayload(new ChannelDescriptionCommandOptionsPayloadBean(oldOptions))
                CommonChannelDescriptionField.COMMAND_OPTIONS, channelUID.getAsString(), linkedItemNames,
                commandOptionsPayload, oldCommandOptionsPayload);
        return new ChannelDescriptionChangedEvent(topic, payload, CommonChannelDescriptionField.COMMAND_OPTIONS,
                channelUID, linkedItemNames, commandOptionsPayload, oldCommandOptionsPayload);
    private ChannelDescriptionChangedEvent createChannelDescriptionChangedEvent(String topic, String payload) {
                    "ChannelDescriptionChangedEvent creation failed, invalid topic: " + topic);
        ChannelUID channelUID = new ChannelUID(topicElements[2]);
        ChannelDescriptionChangedEventPayloadBean bean = deserializePayload(payload,
                ChannelDescriptionChangedEventPayloadBean.class);
        return new ChannelDescriptionChangedEvent(topic, payload, bean.field, channelUID, bean.linkedItemNames,
                bean.value, bean.oldValue);
     * This is a java bean that is used to serialize/deserialize trigger event payload.
    public static class TriggerEventPayloadBean {
        private String event = "";
        private @NonNullByDefault({}) String channel;
        protected TriggerEventPayloadBean() {
        public TriggerEventPayloadBean(String event, String channel) {
        public String getChannel() {
     * Creates a {@link ChannelTriggeredEvent}
     * @return Created {@link ChannelTriggeredEvent}
    public static ChannelTriggeredEvent createTriggerEvent(String event, ChannelUID channelUID) {
        checkNotNull(event, "event");
        String topic = buildTopic(CHANNEL_TRIGGERED_EVENT_TOPIC, channelUID);
        TriggerEventPayloadBean bean = new TriggerEventPayloadBean(event, channelUID.getAsString());
        return new ChannelTriggeredEvent(topic, payload,
                AbstractEvent.buildSource(THING_SOURCE, channelUID.getAsString()), event, channelUID);
    private Event createTriggerEvent(String topic, String payload, @Nullable String source) {
            throw new IllegalArgumentException("ChannelTriggeredEvent creation failed, invalid topic: " + topic);
        ChannelUID channel = new ChannelUID(topicElements[2]);
        TriggerEventPayloadBean bean = deserializePayload(payload, TriggerEventPayloadBean.class);
        return new ChannelTriggeredEvent(topic, payload, source, bean.getEvent(), channel);
            throw new IllegalArgumentException("ThingStatusInfoEvent creation failed, invalid topic: " + topic);
        ThingUID thingUID = new ThingUID(topicElements[2]);
        ThingStatusInfo thingStatusInfo = deserializePayload(payload, ThingStatusInfo.class);
        return new ThingStatusInfoEvent(topic, payload, thingUID, thingStatusInfo);
    private Event createStatusInfoChangedEvent(String topic, String payload) throws Exception {
            throw new IllegalArgumentException("ThingStatusInfoChangedEvent creation failed, invalid topic: " + topic);
        ThingStatusInfo[] thingStatusInfo = deserializePayload(payload, ThingStatusInfo[].class);
        return new ThingStatusInfoChangedEvent(topic, payload, thingUID, thingStatusInfo[0], thingStatusInfo[1]);
    private Event createAddedEvent(String topic, String payload) throws Exception {
        ThingDTO thingDTO = deserializePayload(payload, ThingDTO.class);
        return new ThingAddedEvent(topic, payload, thingDTO);
    private Event createRemovedEvent(String topic, String payload) throws Exception {
        return new ThingRemovedEvent(topic, payload, thingDTO);
    private Event createUpdatedEvent(String topic, String payload) throws Exception {
        ThingDTO[] thingDTO = deserializePayload(payload, ThingDTO[].class);
        if (thingDTO.length != 2) {
            throw new IllegalArgumentException("ThingUpdateEvent creation failed, invalid payload: " + payload);
        return new ThingUpdatedEvent(topic, payload, thingDTO[0], thingDTO[1]);
     * Creates a new thing status info event based on a thing UID and a thing status info object.
     * @param thingStatusInfo the thing status info object
     * @return the created thing status info event
     * @throws IllegalArgumentException if thingUID or thingStatusInfo is null
    public static ThingStatusInfoEvent createStatusInfoEvent(ThingUID thingUID, ThingStatusInfo thingStatusInfo) {
        checkNotNull(thingUID, "thingUID");
        checkNotNull(thingStatusInfo, "thingStatusInfo");
        String topic = buildTopic(THING_STATUS_INFO_EVENT_TOPIC, thingUID);
        String payload = serializePayload(thingStatusInfo);
     * Creates a new thing status info changed event based on a thing UID, a thing status info and the old thing status
     * info object.
     * @param oldThingStatusInfo the old thing status info object
     * @return the created thing status info changed event
    public static ThingStatusInfoChangedEvent createStatusInfoChangedEvent(ThingUID thingUID,
            ThingStatusInfo thingStatusInfo, ThingStatusInfo oldThingStatusInfo) {
        checkNotNull(oldThingStatusInfo, "oldThingStatusInfo");
        String topic = buildTopic(THING_STATUS_INFO_CHANGED_EVENT_TOPIC, thingUID);
        String payload = serializePayload(new ThingStatusInfo[] { thingStatusInfo, oldThingStatusInfo });
        return new ThingStatusInfoChangedEvent(topic, payload, thingUID, thingStatusInfo, oldThingStatusInfo);
     * Creates a thing added event.
     * @return the created thing added event
     * @throws IllegalArgumentException if thing is null
    public static ThingAddedEvent createAddedEvent(Thing thing) {
        assertValidThing(thing);
        String topic = buildTopic(THING_ADDED_EVENT_TOPIC, thing.getUID());
        ThingDTO thingDTO = map(thing);
        String payload = serializePayload(thingDTO);
     * Creates a thing removed event.
     * @return the created thing removed event
    public static ThingRemovedEvent createRemovedEvent(Thing thing) {
        String topic = buildTopic(THING_REMOVED_EVENT_TOPIC, thing.getUID());
     * Creates a thing updated event.
     * @param oldThing the old thing
     * @return the created thing updated event
     * @throws IllegalArgumentException if thing or oldThing is null
    public static ThingUpdatedEvent createUpdateEvent(Thing thing, Thing oldThing) {
        assertValidThing(oldThing);
        String topic = buildTopic(THING_UPDATED_EVENT_TOPIC, thing.getUID());
        ThingDTO oldThingDTO = map(oldThing);
        List<ThingDTO> thingDTOs = new LinkedList<>();
        thingDTOs.add(thingDTO);
        thingDTOs.add(oldThingDTO);
        String payload = serializePayload(thingDTOs);
        return new ThingUpdatedEvent(topic, payload, thingDTO, oldThingDTO);
    private static void assertValidThing(Thing thing) {
        checkNotNull(thing, "thing");
        checkNotNull(thing.getUID(), "thingUID of the thing");
    private static String buildTopic(String topic, ThingUID thingUID) {
        return topic.replace("{thingUID}", thingUID.getAsString());
    private static String buildTopic(String topic, ChannelUID channelUID) {
        return topic.replace("{channelUID}", channelUID.getAsString());
    private static ThingDTO map(Thing thing) {
        return ThingDTOMapper.map(thing);
