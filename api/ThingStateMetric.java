 * The {@link ThingStateMetric} class implements a metric for the openHAB things
 * states.
 * @author Scott Hraban - Create Meter using thingUid instead of thingId during
 *         bind phase
public class ThingStateMetric implements OpenhabCoreMeterBinder, EventSubscriber {
    private final Logger logger = LoggerFactory.getLogger(ThingStateMetric.class);
    public static final String METRIC_NAME = "openhab.thing.state";
    private static final String THING_TAG_NAME = "thing";
    public ThingStateMetric(BundleContext bundleContext, ThingRegistry thingRegistry, Collection<Tag> tags) {
        commonMeterId = new Meter.Id(METRIC_NAME, Tags.of(tags), "state", "openHAB thing state", Meter.Type.GAUGE);
        logger.debug("ThingStateMetric is being bound...");
        thingRegistry.getAll().forEach(
                thing -> createOrUpdateMetricForBundleState(thing.getUID().getAsString(), thing.getStatus().ordinal()));
    private void createOrUpdateMetricForBundleState(String thingUid, int thingStatus) {
        Meter.Id uniqueId = commonMeterId.withTag(Tag.of(THING_TAG_NAME, thingUid));
        AtomicInteger thingStateHolder = registeredMeters.get(uniqueId);
        if (thingStateHolder == null) {
            thingStateHolder = new AtomicInteger();
            Gauge.builder(uniqueId.getName(), thingStateHolder, AtomicInteger::get).baseUnit(uniqueId.getBaseUnit())
            registeredMeters.put(uniqueId, thingStateHolder);
        thingStateHolder.set(thingStatus);
        if (event instanceof ThingStatusInfoEvent thingEvent) {
            logger.trace("Received ThingStatusInfo(Changed)Event...");
            String thingUid = thingEvent.getThingUID().getAsString();
            ThingStatus status = gson.fromJson(event.getPayload(), ThingStatusInfo.class).getStatus();
            createOrUpdateMetricForBundleState(thingUid, status.ordinal());
            logger.trace("Received unsubscribed for event type {}", event.getClass().getSimpleName());
