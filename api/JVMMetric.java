import io.micrometer.core.instrument.binder.jvm.ClassLoaderMetrics;
import io.micrometer.core.instrument.binder.jvm.JvmGcMetrics;
import io.micrometer.core.instrument.binder.jvm.JvmMemoryMetrics;
import io.micrometer.core.instrument.binder.jvm.JvmThreadMetrics;
import io.micrometer.core.instrument.binder.system.ProcessorMetrics;
 * The {@link JVMMetric} class implements JVM related metrics like class loading, memory, GC and thread figures
public class JVMMetric implements OpenhabCoreMeterBinder {
    private final Logger logger = LoggerFactory.getLogger(JVMMetric.class);
    private static final Tag CORE_JVM_METRIC_TAG = Tag.of("metric", "openhab.core.metric.jvm");
    public JVMMetric(Collection<Tag> tags) {
        this.tags.add(CORE_JVM_METRIC_TAG);
    public void bindTo(@NonNullByDefault({}) MeterRegistry registry) {
        logger.debug("JVMMetric is being bound...");
        this.meterRegistry = registry;
        new ClassLoaderMetrics(tags).bindTo(meterRegistry);
        new JvmMemoryMetrics(tags).bindTo(meterRegistry);
        new JvmGcMetrics(tags).bindTo(meterRegistry);
        new ProcessorMetrics(tags).bindTo(meterRegistry);
        new JvmThreadMetrics(tags).bindTo(meterRegistry);
            if (meter.getId().getTags().contains(CORE_JVM_METRIC_TAG)) {
