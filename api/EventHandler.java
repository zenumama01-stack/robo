package org.openhab.core.internal.events;
 * Handle openHAB events encapsulated by OSGi events.
public class EventHandler implements AutoCloseable {
    private static final int EVENT_QUEUE_WARN_LIMIT = 5000;
    private static final long EVENTSUBSCRIBER_EVENTHANDLING_MAX_MS = TimeUnit.SECONDS.toMillis(5);
    private final Logger logger = LoggerFactory.getLogger(EventHandler.class);
    private final Map<String, Set<EventSubscriber>> typedEventSubscribers;
    private final Map<String, EventFactory> typedEventFactories;
    private final Map<Class<? extends EventSubscriber>, ExecutorRecord> executors = new HashMap<>();
     * Create a new event handler.
     * @param typedEventSubscribers the event subscribers indexed by the event type
     * @param typedEventFactories the event factories indexed by the event type
    public EventHandler(final Map<String, Set<EventSubscriber>> typedEventSubscribers,
            final Map<String, EventFactory> typedEventFactories) {
        this.typedEventSubscribers = typedEventSubscribers;
        this.typedEventFactories = typedEventFactories;
        watcher = Executors.newSingleThreadScheduledExecutor(new NamedThreadFactory("eventwatcher"));
    private synchronized ExecutorRecord createExecutorRecord(Class<? extends EventSubscriber> subscriber) {
        return new ExecutorRecord(ThreadPoolManager.getPoolBasedSequentialScheduledExecutorService("events",
                "eventexecutor-" + executors.size()), new AtomicInteger());
        executors.values().forEach(r -> {
            r.executor.shutdownNow();
    public void handleEvent(org.osgi.service.event.Event osgiEvent) {
        Object typeObj = osgiEvent.getProperty(OSGiEventPublisher.TYPE);
        Object payloadObj = osgiEvent.getProperty(OSGiEventPublisher.PAYLOAD);
        Object topicObj = osgiEvent.getProperty(OSGiEventPublisher.TOPIC);
        Object sourceObj = osgiEvent.getProperty(OSGiEventPublisher.SOURCE);
        if (typeObj instanceof String typeStr && payloadObj instanceof String payloadStr
                && topicObj instanceof String topicStr) {
            String sourceStr = (sourceObj instanceof String s) ? s : null;
            if (!typeStr.isEmpty() && !payloadStr.isEmpty() && !topicStr.isEmpty()) {
                handleEvent(typeStr, payloadStr, topicStr, sourceStr);
                    "The handled OSGi event is invalid. Expect properties as string named 'type', 'payload' and 'topic'. Received event properties are: {}",
                    Arrays.toString(osgiEvent.getPropertyNames()));
    private void handleEvent(final String type, final String payload, final String topic,
            final @Nullable String source) {
        final EventFactory eventFactory = typedEventFactories.get(type);
        if (eventFactory == null) {
            logger.debug("Could not find an Event Factory for the event type '{}'.", type);
        final Set<EventSubscriber> eventSubscribers = getEventSubscribers(type);
        if (eventSubscribers.isEmpty()) {
        final Event event = createEvent(eventFactory, type, payload, topic, source);
        dispatchEvent(eventSubscribers, event);
    private Set<EventSubscriber> getEventSubscribers(String eventType) {
        Set<EventSubscriber> eventTypeSubscribers = typedEventSubscribers.get(eventType);
        Set<EventSubscriber> allEventTypeSubscribers = typedEventSubscribers.get(EventSubscriber.ALL_EVENT_TYPES);
        Set<EventSubscriber> subscribers = new HashSet<>();
        if (eventTypeSubscribers != null) {
            subscribers.addAll(eventTypeSubscribers);
        if (allEventTypeSubscribers != null) {
            subscribers.addAll(allEventTypeSubscribers);
    private @Nullable Event createEvent(final EventFactory eventFactory, final String type, final String payload,
            final String topic, final @Nullable String source) {
            return eventFactory.createEvent(type, topic, payload, source);
                    "Creation of event failed, because one of the registered event factories has thrown an exception: {}",
    private synchronized void dispatchEvent(final Set<EventSubscriber> eventSubscribers, final Event event) {
        for (final EventSubscriber eventSubscriber : eventSubscribers) {
            EventFilter filter = eventSubscriber.getEventFilter();
            if (filter == null || filter.apply(event)) {
                logger.trace("Delegate event to subscriber ({}).", eventSubscriber.getClass());
                ExecutorRecord executorRecord = Objects.requireNonNull(
                        executors.computeIfAbsent(eventSubscriber.getClass(), this::createExecutorRecord));
                int queueSize = executorRecord.count.incrementAndGet();
                if (queueSize > EVENT_QUEUE_WARN_LIMIT) {
                    logger.warn("The queue for a subscriber of type '{}' exceeds {} elements. System may be unstable.",
                            eventSubscriber.getClass(), EVENT_QUEUE_WARN_LIMIT);
                CompletableFuture.runAsync(() -> {
                    ScheduledFuture<?> logTimeout = watcher.schedule(
                            () -> logger.warn("Dispatching event to subscriber '{}' takes more than {}ms.",
                                    eventSubscriber, EVENTSUBSCRIBER_EVENTHANDLING_MAX_MS),
                            EVENTSUBSCRIBER_EVENTHANDLING_MAX_MS, TimeUnit.MILLISECONDS);
                        eventSubscriber.receive(event);
                        logger.warn("Dispatching/filtering event for subscriber '{}' failed: {}",
                                EventSubscriber.class.getName(), ex.getMessage(), ex);
                    logTimeout.cancel(false);
                }, executorRecord.executor).thenRun(executorRecord.count::decrementAndGet);
                logger.trace("Skip event subscriber ({}) because of its filter.", eventSubscriber.getClass());
    private record ExecutorRecord(ExecutorService executor, AtomicInteger count) {
