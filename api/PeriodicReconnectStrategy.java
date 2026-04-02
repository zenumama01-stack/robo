 * This is an implementation of the {@link AbstractReconnectStrategy}. This
 * strategy tries to reconnect after 10 seconds and then every 60 seconds
 * after a broker connection has been lost.
public class PeriodicReconnectStrategy extends AbstractReconnectStrategy {
    private final Logger logger = LoggerFactory.getLogger(PeriodicReconnectStrategy.class);
    private final int reconnectFrequency;
    private final int firstReconnectAfter;
    private @Nullable ScheduledExecutorService scheduler = null;
    private @Nullable ScheduledFuture<?> scheduledTask;
     * Use a default 60s reconnect frequency and try the first reconnect after 10s.
    public PeriodicReconnectStrategy() {
        this(60000, 10000);
     * Create a {@link PeriodicReconnectStrategy} with the given reconnect frequency and
     * first reconnect time parameters.
     * @param reconnectFrequency This strategy tries to reconnect in this frequency in ms.
     * @param firstReconnectAfter After a connection is lost, the very first reconnect attempt will be performed after
     *            this time in ms.
    public PeriodicReconnectStrategy(int reconnectFrequency, int firstReconnectAfter) {
        this.reconnectFrequency = reconnectFrequency;
        this.firstReconnectAfter = firstReconnectAfter;
    public synchronized void start() {
        if (scheduler == null) {
            scheduler = Executors.newScheduledThreadPool(1);
    public synchronized void stop() {
        if (scheduler != null) {
            scheduler.shutdownNow();
            scheduler = null;
        // If there is a scheduled task ensure it is canceled.
        if (scheduledTask != null) {
            scheduledTask.cancel(true);
            scheduledTask = null;
     * Returns if the reconnect strategy has been started.
     * @return true if started
    public synchronized boolean isStarted() {
        return scheduler != null;
    public synchronized void lostConnection() {
        // Check if we are running (has been started and not stopped) state.
        if (brokerConnection == null) {
        // If there is already a scheduled task, we continue only if it has been done (shouldn't be the case at all).
        if (scheduledTask != null && !scheduledTask.isDone()) {
        scheduledTask = Objects.requireNonNull(scheduler).scheduleWithFixedDelay(() -> {
            MqttBrokerConnection brokerConnection = this.brokerConnection;
            // If the broker connections is not available anymore, stop the timed reconnect.
            logger.info("Try to restore connection to '{}'. Next attempt in {}ms", brokerConnection.getHost(),
                    getReconnectFrequency());
            brokerConnection.start().exceptionally(e -> {
                logger.warn("Broker connection couldn't be started", e);
        }, getFirstReconnectAfter(), getReconnectFrequency(), TimeUnit.MILLISECONDS);
    public synchronized void connectionEstablished() {
        // Stop the reconnect task if existing.
    public synchronized boolean isReconnecting() {
        return scheduledTask != null;
    public int getReconnectFrequency() {
        return reconnectFrequency;
    public int getFirstReconnectAfter() {
        return firstReconnectAfter;
