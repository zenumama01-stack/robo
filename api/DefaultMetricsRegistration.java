package org.openhab.core.io.monitor.internal;
import org.openhab.core.io.monitor.MeterRegistryProvider;
import org.openhab.core.io.monitor.internal.metrics.BundleStateMetric;
import org.openhab.core.io.monitor.internal.metrics.EventCountMetric;
import org.openhab.core.io.monitor.internal.metrics.JVMMetric;
import org.openhab.core.io.monitor.internal.metrics.OpenhabCoreMeterBinder;
import org.openhab.core.io.monitor.internal.metrics.RuleMetric;
import org.openhab.core.io.monitor.internal.metrics.ThingStateMetric;
import org.openhab.core.io.monitor.internal.metrics.ThreadPoolMetric;
import io.micrometer.core.instrument.Metrics;
import io.micrometer.core.instrument.Tag;
 * The {@link DefaultMetricsRegistration} class registers all openHAB internal metrics with the global MeterRegistry.
@Component(immediate = true, service = MeterRegistryProvider.class)
public class DefaultMetricsRegistration implements ReadyService.ReadyTracker, MeterRegistryProvider {
    private final Logger logger = LoggerFactory.getLogger(DefaultMetricsRegistration.class);
    public static final Tag OH_CORE_METRIC_TAG = Tag.of("openhab_core_metric", "true");
    private final Set<OpenhabCoreMeterBinder> meters = new HashSet<>();
    private final CompositeMeterRegistry registry = Metrics.globalRegistry;
    public DefaultMetricsRegistration(BundleContext bundleContext, final @Reference ReadyService readyService,
            final @Reference ThingRegistry thingRegistry, final @Reference RuleRegistry ruleRegistry) {
        logger.trace("Activating DefaultMetricsRegistration...");
        unregisterMeters();
    private void registerMeters() {
        logger.debug("Registering meters...");
        Set<Tag> tags = Set.of(OH_CORE_METRIC_TAG);
        meters.add(new JVMMetric(tags));
        meters.add(new ThreadPoolMetric(tags));
        meters.add(new BundleStateMetric(bundleContext, tags));
        meters.add(new ThingStateMetric(bundleContext, thingRegistry, tags));
        meters.add(new EventCountMetric(bundleContext, tags));
        meters.add(new RuleMetric(bundleContext, tags, ruleRegistry));
        meters.forEach(m -> m.bindTo(registry));
    private void unregisterMeters() {
        meters.forEach(OpenhabCoreMeterBinder::unbind);
        registerMeters();
    public CompositeMeterRegistry getOHMeterRegistry() {
