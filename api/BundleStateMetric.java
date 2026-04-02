package org.openhab.core.io.monitor.internal.metrics;
import org.osgi.framework.BundleListener;
import io.micrometer.core.instrument.Gauge;
import io.micrometer.core.instrument.Meter;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.Tags;
 * The {@link BundleStateMetric} class implements a set of gauge metrics for the OSGI bundles states
public class BundleStateMetric implements OpenhabCoreMeterBinder, BundleListener {
    private final Logger logger = LoggerFactory.getLogger(BundleStateMetric.class);
    public static final String METRIC_NAME = "openhab.bundle.state";
    private static final String BUNDLE_TAG_NAME = "bundle";
    private final Meter.Id commonMeterId;
    private final Map<Meter.Id, AtomicInteger> registeredMeters = new HashMap<>();
    private @Nullable MeterRegistry meterRegistry;
    public BundleStateMetric(BundleContext bundleContext, Collection<Tag> tags) {
        commonMeterId = new Meter.Id(METRIC_NAME, Tags.of(tags), "state", "openHAB OSGi bundles state",
                Meter.Type.GAUGE);
    public void bindTo(@NonNullByDefault({}) MeterRegistry meterRegistry) {
        unbind();
        logger.debug("BundleStateMetric is being bound...");
        this.meterRegistry = meterRegistry;
        Stream.of(bundleContext.getBundles()).forEach(bundle -> {
            createOrUpdateMetricForBundleState(bundle.getSymbolicName(), bundle.getState());
        bundleContext.addBundleListener(this);
    public void bundleChanged(@NonNullByDefault({}) BundleEvent bundleEvent) {
        if (meterRegistry == null) {
        String bundleName = bundleEvent.getBundle().getSymbolicName();
        int state = bundleEvent.getBundle().getState();
        createOrUpdateMetricForBundleState(bundleName, state);
    private void createOrUpdateMetricForBundleState(String bundleName, int state) {
        Meter.Id uniqueId = commonMeterId.withTag(Tag.of(BUNDLE_TAG_NAME, bundleName));
        AtomicInteger bundleStateHolder = registeredMeters.get(uniqueId);
        if (bundleStateHolder == null) {
            bundleStateHolder = new AtomicInteger();
            Gauge.builder(uniqueId.getName(), bundleStateHolder, AtomicInteger::get).baseUnit(uniqueId.getBaseUnit())
                    .description(uniqueId.getDescription()).tags(uniqueId.getTags()).register(meterRegistry);
            registeredMeters.put(uniqueId, bundleStateHolder);
        bundleStateHolder.set(state);
    public void unbind() {
        MeterRegistry meterRegistry = this.meterRegistry;
        bundleContext.removeBundleListener(this);
        registeredMeters.keySet().forEach(meterRegistry::remove);
        registeredMeters.clear();
        this.meterRegistry = null;
