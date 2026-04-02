import io.micrometer.core.instrument.binder.jvm.ExecutorServiceMetrics;
 * The {@link ThreadPoolMetric} class implements a set of metrics for ThreadManager thread pool stats
public class ThreadPoolMetric implements OpenhabCoreMeterBinder {
    private final Logger logger = LoggerFactory.getLogger(ThreadPoolMetric.class);
    public static final Tag CORE_THREADPOOL_METRIC_TAG = Tag.of("metric", "openhab.core.metric.threadpools");
    private static final String POOLNAME_TAG_NAME = "pool";
    private Set<ExecutorServiceMetrics> executorServiceMetricsSet = new HashSet<>();
    public ThreadPoolMetric(Collection<Tag> tags) {
        this.tags.add(CORE_THREADPOOL_METRIC_TAG);
        logger.debug("ThreadPoolMetric is being bound...");
            ThreadPoolManager.getPoolNames().forEach(this::addPoolMetrics);
        } catch (NoSuchMethodError nsme) {
            logger.info("A newer version of openHAB is required for thread pool metrics to work.");
    private void addPoolMetrics(String poolName) {
        ExecutorService es = ThreadPoolManager.getPool(poolName);
        if (es == null) {
        Set<Tag> tagsWithPoolname = new HashSet<>(tags);
        tagsWithPoolname.add(Tag.of(POOLNAME_TAG_NAME, poolName));
        ExecutorServiceMetrics metrics = new ExecutorServiceMetrics(es, poolName, tagsWithPoolname);
        metrics.bindTo(meterRegistry);
        executorServiceMetricsSet.add(metrics);
            if (meter.getId().getTags().contains(CORE_THREADPOOL_METRIC_TAG)) {
        executorServiceMetricsSet.clear();
