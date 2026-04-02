 * This class is a factory that creates Timer and Execution Events.
public class AutomationEventFactory extends AbstractEventFactory {
    private static final String MODULE_IDENTIFIER = "{moduleId}";
    private static final String TIMER_EVENT_TOPIC = "openhab/timer/" + MODULE_IDENTIFIER + "/triggered";
    private static final String EXECUTION_EVENT_TOPIC = "openhab/execution/" + MODULE_IDENTIFIER + "/triggered";
    private final Logger logger = LoggerFactory.getLogger(AutomationEventFactory.class);
    private static final Set<String> SUPPORTED_TYPES = Set.of(TimerEvent.TYPE, ExecutionEvent.TYPE);
    public AutomationEventFactory() {
        super(SUPPORTED_TYPES);
        logger.trace("creating ruleEvent of type: {}", eventType);
        if (TimerEvent.TYPE.equals(eventType)) {
            return createTimerEvent(topic, payload, Objects.requireNonNullElse(source, "<unknown>"));
        } else if (ExecutionEvent.TYPE.equals(eventType)) {
                throw new IllegalArgumentException("'source' must not be null for execution events");
            return createExecutionEvent(topic, payload, source);
    private Event createTimerEvent(String topic, String payload, String source) {
        return new TimerEvent(topic, payload, source);
    private Event createExecutionEvent(String topic, String payload, String source) {
        return new ExecutionEvent(topic, payload, source);
     * Creates a {@link TimerEvent}
     * @param moduleId the module type id of this event
     * @param label The label (or id) of this object
     * @param configuration the configuration of the trigger
     * @return the created event
    public static TimerEvent createTimerEvent(String moduleId, @Nullable String label,
            Map<String, Object> configuration) {
        String topic = TIMER_EVENT_TOPIC.replace(MODULE_IDENTIFIER, moduleId);
        String payload = serializePayload(configuration);
        return new TimerEvent(topic, payload, label);
     * Creates an {@link ExecutionEvent}
     * @param payload A map with additional information like preceding events when rules are called from other rules
     *            (optional)
     * @param source The source of this event (e.g. "script" or "manual")
    public static ExecutionEvent createExecutionEvent(String moduleId, @Nullable Map<String, Object> payload,
            String source) {
        String topic = EXECUTION_EVENT_TOPIC.replace(MODULE_IDENTIFIER, moduleId);
        String serializedPayload = serializePayload(Objects.requireNonNullElse(payload, Map.of()));
        return new ExecutionEvent(topic, serializedPayload, source);
