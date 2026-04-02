import org.openhab.core.events.TopicGlobEventFilter;
 * This is the implementation of an event condition which checks if inputs matches configured values.
 * @author Cody Cutrer - refactored to match configuration and semantics of GenericEventTriggerHandler
public class GenericEventConditionHandler extends BaseConditionModuleHandler {
    public static final String MODULETYPE_ID = "core.GenericEventCondition";
    public static final String CFG_TOPIC = "topic";
    public static final String CFG_TYPES = "types";
    public static final String CFG_SOURCE = "source";
    public static final String CFG_PAYLOAD = "payload";
    public final Logger logger = LoggerFactory.getLogger(GenericEventConditionHandler.class);
    private final String source;
    private final @Nullable TopicGlobEventFilter topicFilter;
    private final @Nullable Pattern payloadPattern;
    public GenericEventConditionHandler(Condition module) {
        this.source = (String) module.getConfiguration().get(CFG_SOURCE);
        String topic = (String) module.getConfiguration().get(CFG_TOPIC);
        if (!topic.isBlank()) {
            topicFilter = new TopicGlobEventFilter(topic);
            topicFilter = null;
        if (module.getConfiguration().get(CFG_TYPES) != null) {
            this.types = Set.of(((String) module.getConfiguration().get(CFG_TYPES)).split(","));
            this.types = Set.of();
        String payload = (String) module.getConfiguration().get(CFG_PAYLOAD);
        if (!payload.isBlank()) {
            payloadPattern = Pattern.compile(payload);
            payloadPattern = null;
        Event event = inputs.get("event") instanceof Event ? (Event) inputs.get("event") : null;
        if (event == null) {
        if (!types.isEmpty() && !types.contains(event.getType())) {
        TopicGlobEventFilter localTopicFilter = topicFilter;
        if (localTopicFilter != null && !localTopicFilter.apply(event)) {
        if (!source.isEmpty() && !source.equals(event.getSource())) {
        Pattern localPayloadPattern = payloadPattern;
        if (localPayloadPattern != null && !localPayloadPattern.matcher(event.getPayload()).find()) {
