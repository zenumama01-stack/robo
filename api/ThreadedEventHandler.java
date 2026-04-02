 * Handle openHAB events encapsulated by OSGi events in a separate thread.
public class ThreadedEventHandler implements Closeable {
    private final Logger logger = LoggerFactory.getLogger(ThreadedEventHandler.class);
    private final Thread thread;
    private final Event notifyEvent = new Event("notify", Map.of());
    private final BlockingQueue<Event> queue = new LinkedBlockingQueue<>();
    private final AtomicBoolean running = new AtomicBoolean(true);
     * Create a new threaded event handler.
     * @param typedEventSubscribers the event subscribers
    ThreadedEventHandler(Map<String, Set<EventSubscriber>> typedEventSubscribers,
        thread = new Thread(() -> {
            try (EventHandler worker = new EventHandler(typedEventSubscribers, typedEventFactories)) {
                while (running.get()) {
                        logger.trace("wait for event");
                        final Event event = queue.poll(1, TimeUnit.HOURS);
                        logger.trace("inspect event: {}", event);
                            logger.debug("Hey, you have really very few events.");
                        } else if (event.equals(notifyEvent)) { // NOPMD
                            // received an internal notification
                            worker.handleEvent(event);
                        logger.error("Error on event handling.", ex);
        }, "OH-OSGiEventManager");
    void open() {
        running.set(false);
        queue.add(notifyEvent);
        thread.interrupt();
            thread.join();
    void handleEvent(Event event) {
        queue.add(event);
