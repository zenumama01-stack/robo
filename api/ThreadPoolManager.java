import org.openhab.core.common.PoolBasedSequentialScheduledExecutorService.BasePoolExecutor;
 * This class provides a general mechanism to create thread pools. In general, no code of openHAB
 * should deal with its own pools, but rather use this class.
 * The created thread pools have named threads, so that it is easy to find them in the debugger. Additionally, it is
 * possible to configure the pool sizes through the configuration admin service, so that solutions have the chance to
 * tweak the pool sizes according to their needs.
 * The configuration can be done as
 * {@code org.openhab.core.threadpool:<poolName>=<poolSize>}
 * All threads will time out after {@link #THREAD_TIMEOUT}.
@Component(configurationPid = ThreadPoolManager.CONFIGURATION_PID)
public class ThreadPoolManager {
    public static final String CONFIGURATION_PID = "org.openhab.threadpool";
     * The common thread pool is reserved for occasional, light weight tasks that run quickly, and
     * use little resources to execute. Tasks that do not fit into this category should setup
     * their own dedicated pool or permanent thread.
    public static final String THREAD_POOL_NAME_COMMON = "common";
    private static final Logger LOGGER = LoggerFactory.getLogger(ThreadPoolManager.class);
    protected static final int DEFAULT_THREAD_POOL_SIZE = 5;
    protected static final long THREAD_TIMEOUT = 65L;
    protected static final long THREAD_MONITOR_SLEEP = 60000;
    protected static Map<String, ExecutorService> pools = new ConcurrentHashMap<>();
    private static Map<String, Integer> configs = new ConcurrentHashMap<>();
    private static final Set<String> OSGI_PROPERTY_NAMES = Set.of(Constants.SERVICE_PID,
            ComponentConstants.COMPONENT_ID, ComponentConstants.COMPONENT_NAME, "osgi.ds.satisfying.condition.target");
        for (Entry<String, Object> entry : properties.entrySet()) {
            if (OSGI_PROPERTY_NAMES.contains(entry.getKey())) {
            String poolName = entry.getKey();
            Object config = entry.getValue();
                configs.remove(poolName);
            if (config instanceof String string) {
                    Integer poolSize = Integer.valueOf(string);
                    configs.put(poolName, poolSize);
                    ThreadPoolExecutor pool = (ThreadPoolExecutor) pools.get(poolName);
                    if (pool instanceof BasePoolExecutor basePool) {
                        basePool.setMinimumPoolSize(poolSize);
                        LOGGER.debug("Updated scheduled thread pool '{}' to minimum size {}", poolName, poolSize);
                    } else if (pool instanceof ScheduledThreadPoolExecutor) {
                        pool.setCorePoolSize(poolSize);
                        LOGGER.debug("Updated scheduled thread pool '{}' to size {}", poolName, poolSize);
                    } else if (pool instanceof QueueingThreadPoolExecutor) {
                        pool.setMaximumPoolSize(poolSize);
                        LOGGER.debug("Updated queuing thread pool '{}' to size {}", poolName, poolSize);
                    LOGGER.warn("Ignoring invalid configuration for pool '{}': {} - value must be an integer", poolName,
                            config);
     * Returns an instance of a scheduled service, which will sequentially execute submitted tasks. If a task is
     * currently running the task is queued until the previous one is completed, this also applies for scheduled tasks.
     * The service might execute submitted task might in different threads, but still one after the other.
     * If it is the first request for the given pool name and a pool is used, the instance is newly created.
     * @param poolName a short name used to identify the pool, if a thread pool is used e.g. "bluetooth-discovery"
     * @param threadName a short name used to identify the thread if no thread pool is used, e.g. "bluetooth"
     * @return an instance to use
    public static ScheduledExecutorService getPoolBasedSequentialScheduledExecutorService(String poolName,
            String threadName) {
        if (configs.getOrDefault(poolName, 0) > 0) {
            ExecutorService pool = pools.computeIfAbsent(poolName, name -> {
                int cfg = getConfig(name);
                ScheduledThreadPoolExecutor executor = new BasePoolExecutor(name, cfg,
                        new NamedThreadFactory(name, true, Thread.NORM_PRIORITY));
                executor.setKeepAliveTime(THREAD_TIMEOUT, TimeUnit.SECONDS);
                executor.setRemoveOnCancelPolicy(true);
                LOGGER.debug("Created scheduled pool based thread pool '{}' of size {}", name, cfg);
            if (pool instanceof BasePoolExecutor service) {
                return new PoolBasedSequentialScheduledExecutorService(service);
                throw new IllegalArgumentException("Pool " + poolName + " is not a base pool!");
            return Executors.newSingleThreadScheduledExecutor(new NamedThreadFactory(threadName));
     * Returns an instance of a scheduled thread pool service. If it is the first request for the given pool name, the
     * instance is newly created.
     * @param poolName a short name used to identify the pool, e.g. "discovery"
    public static ScheduledExecutorService getScheduledPool(String poolName) {
            ScheduledThreadPoolExecutor executor = new WrappedScheduledExecutorService(cfg,
            LOGGER.debug("Created scheduled thread pool '{}' of size {}", name, cfg);
        if (pool instanceof ScheduledExecutorService service) {
            return new UnstoppableScheduledExecutorService(poolName, service);
            throw new IllegalArgumentException("Pool " + poolName + " is not a scheduled pool!");
     * Returns an instance of a cached thread pool service. If it is the first request for the given pool name, the
    public static ExecutorService getPool(String poolName) {
            ThreadPoolExecutor executor = QueueingThreadPoolExecutor.createInstance(name, cfg);
            LOGGER.debug("Created thread pool '{}' with size {}", name, cfg);
        return new UnstoppableExecutorService<>(poolName, pool);
    static ThreadPoolExecutor getPoolUnwrapped(String poolName) {
        UnstoppableExecutorService<?> ret = (UnstoppableExecutorService<?>) getPool(poolName);
        return (ThreadPoolExecutor) ret.getDelegate();
    static ScheduledThreadPoolExecutor getScheduledPoolUnwrapped(String poolName) {
        UnstoppableExecutorService<?> ret = (UnstoppableScheduledExecutorService) getScheduledPool(poolName);
        return (ScheduledThreadPoolExecutor) ret.getDelegate();
    protected static int getConfig(String poolName) {
        Integer cfg = configs.get(poolName);
        return cfg != null ? cfg : DEFAULT_THREAD_POOL_SIZE;
    public static Set<String> getPoolNames() {
        return new HashSet<>(pools.keySet());
    // needs to be of a class supported by micrometer-core, see ExecutorServiceMetrics.java,
    // originally this class was intended to be defined as "implements ExecutorService"
    static class UnstoppableExecutorService<T extends ExecutorService> extends ThreadPoolExecutor {
        protected final T delegate;
        protected final String threadPoolName;
        private UnstoppableExecutorService(String threadPoolName, T delegate) {
            // although nearly all methods of ThreadPoolExecutor are overwritten, super() needs to be
            // called with valid parameters
            super(0, 1, 0, TimeUnit.SECONDS, new ArrayBlockingQueue<Runnable>(1));
        public <T> T invokeAny(Collection<? extends Callable<T>> tasks)
        T getDelegate() {
            return delegate;
        // the following methods of ThreadPoolExecutor will be queried for collection of metrics,
        // they need to be replaced and the query is to be delegated to out ThreadPoolExecutor
        // referenced in variable "delegate"
        // not part of monitoring
        // public long getTaskCount() {}
        public long getCompletedTaskCount() {
            // executor_completed_tasks_total
            return ((ThreadPoolExecutor) delegate).getCompletedTaskCount();
        public int getActiveCount() {
            // executor_active_threads
            return ((ThreadPoolExecutor) delegate).getActiveCount();
        public int getMaximumPoolSize() {
            // executor_pool_max_threads
            return ((ThreadPoolExecutor) delegate).getMaximumPoolSize();
        // public int getLargestPoolSize() {}
        public int getCorePoolSize() {
            // executor_pool_core_threads
            return ((ThreadPoolExecutor) delegate).getCorePoolSize();
        public int getPoolSize() {
            // executor_pool_size_threads
            return ((ThreadPoolExecutor) delegate).getPoolSize();
            return new ArrayBlockingQueue<Runnable>(1) {
                public int remainingCapacity() {
                    // executor_queue_remaining_tasks
                    return ((ThreadPoolExecutor) delegate).getQueue().remainingCapacity();
                    // executor_queued_tasks
                    return ((ThreadPoolExecutor) delegate).getQueue().size();
    static class UnstoppableScheduledExecutorService extends UnstoppableExecutorService<ScheduledExecutorService>
            implements ScheduledExecutorService {
        private UnstoppableScheduledExecutorService(String threadPoolName, ScheduledExecutorService delegate) {
            super(threadPoolName, delegate);
        public ScheduledFuture<?> scheduleWithFixedDelay(Runnable command, long initialDelay, long delay,
                TimeUnit unit) {
