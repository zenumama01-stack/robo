public class EventLogger implements EventSubscriber, ReadyTracker {
    private final Map<String, Logger> eventLoggers = new HashMap<>();
    private final Set<String> subscribedEventTypes = Set.of(EventSubscriber.ALL_EVENT_TYPES);
    private boolean loggingActive = false;
    public EventLogger(@Reference ReadyService readyService) {
        return subscribedEventTypes;
        Logger logger = getLogger(event.getType());
        logger.trace("Received event of type '{}' under the topic '{}' with payload: '{}'", event.getType(),
                event.getTopic(), event.getPayload());
        if (loggingActive) {
            logger.info("{}", event);
    private Logger getLogger(String eventType) {
                eventLoggers.computeIfAbsent(eventType, type -> LoggerFactory.getLogger("openhab.event." + eventType)));
        loggingActive = true;
        loggingActive = false;
