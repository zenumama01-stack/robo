 * The {@link EventCountMetric} class implements a gauge metric for the openHAB events count (per topic)
 * topic.
public class EventCountMetric implements OpenhabCoreMeterBinder, EventSubscriber {
    public static final String METRIC_NAME = "event_count";
    private final Logger logger = LoggerFactory.getLogger(EventCountMetric.class);
    private static final Tag CORE_EVENT_COUNT_METRIC_TAG = Tag.of("metric", "openhab.core.metric.eventcount");
    private static final String TOPIC_TAG_NAME = "topic";
    private final Set<Tag> tags = new HashSet<>();
    private BundleContext bundleContext;
    public EventCountMetric(BundleContext bundleContext, Collection<Tag> tags) {
        this.tags.addAll(tags);
        this.tags.add(CORE_EVENT_COUNT_METRIC_TAG);
        logger.debug("EventCountMetric is being bound...");
        this.eventSubscriberRegistration = this.bundleContext.registerService(EventSubscriber.class.getName(), this,
        for (Meter meter : meterRegistry.getMeters()) {
            if (meter.getId().getTags().contains(CORE_EVENT_COUNT_METRIC_TAG)) {
                meterRegistry.remove(meter);
        return Set.of(ItemCommandEvent.TYPE, ItemStateEvent.TYPE);
            logger.trace("Measurement not started. Skipping event processing");
        String topic = event.getTopic();
        logger.debug("Received event on topic {}.", topic);
        Set<Tag> tagsWithTopic = new HashSet<>(tags);
        tagsWithTopic.add(Tag.of(TOPIC_TAG_NAME, topic));
        meterRegistry.counter(METRIC_NAME, tagsWithTopic).increment();
