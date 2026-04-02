import java.util.concurrent.Delayed;
 * Implementation of the {@link Scheduler}.
 * @author Simon Kaufmann - ported to CompletableFuture
 * @author Hilbrand Bouwkamp - improved implementation and moved cron and periodic to own implementations.
@Component(service = SchedulerImpl.class, immediate = true)
public class SchedulerImpl implements Scheduler {
    private static final String SCHEDULER_THREAD_POOL = "scheduler";
    private static final int ALLOWED_DEVIATION_MILLISECONDS = 2000;
    private final Logger logger = LoggerFactory.getLogger(SchedulerImpl.class);
    private final ScheduledExecutorService executor = ThreadPoolManager.getScheduledPool(SCHEDULER_THREAD_POOL);
    public ScheduledCompletableFuture<Instant> after(Duration duration) {
        final Instant start = Instant.now();
        return after(() -> start, duration);
    public <T> ScheduledCompletableFuture<T> after(Callable<T> callable, Duration duration) {
        return afterInternal(new ScheduledCompletableFutureOnce<>(null, duration), callable);
    private <T> ScheduledCompletableFutureOnce<T> afterInternal(ScheduledCompletableFutureOnce<T> deferred,
            Callable<T> callable) {
        final long duration = Math.max(100,
                deferred.getScheduledTime().minus(currentTimeMillis(), ChronoUnit.MILLIS).toInstant().toEpochMilli());
        final ScheduledFuture<?> future = executor.schedule(() -> {
                final long timeLeft = deferred.getDelay(TimeUnit.MILLISECONDS);
                if (timeLeft > ALLOWED_DEVIATION_MILLISECONDS) {
                    logger.trace("Scheduled task is re-scheduled because the scheduler ran {} milliseconds to early.",
                            timeLeft);
                    afterInternal(deferred, callable);
                    logger.trace("Scheduled task is run now.");
                    deferred.complete(callable.call());
                logger.warn("Scheduled job '{}' failed and stopped",
                        Objects.requireNonNullElse(deferred.identifier, "<unknown>"), e);
                deferred.completeExceptionally(e);
        }, duration, TimeUnit.MILLISECONDS);
            logger.trace("Scheduled a task to run in {} seconds.", TimeUnit.MILLISECONDS.toSeconds(duration));
        deferred.exceptionally(e -> {
            logger.trace("Scheduled task stopped with exception ", e);
            if (e instanceof CancellationException) {
        return deferred;
        final AtomicBoolean done = new AtomicBoolean();
        final Consumer<Runnable> runOnce = runnable -> {
            if (!done.getAndSet(true)) {
        final ScheduledCompletableFutureOnce<T> wrappedPromise = new ScheduledCompletableFutureOnce<>(null, timeout);
        Callable<T> callable = () -> {
            wrappedPromise.completeExceptionally(new TimeoutException());
        final ScheduledCompletableFutureOnce<T> afterPromise = afterInternal(wrappedPromise, callable);
        wrappedPromise.exceptionally(e -> {
                // Also cancel the scheduled timer if returned completable future is cancelled.
                afterPromise.cancel(true);
        promise.thenAccept(p -> runOnce.accept(() -> wrappedPromise.complete(p))) //
                .exceptionally(ex -> {
                    runOnce.accept(() -> wrappedPromise.completeExceptionally(ex));
        return wrappedPromise;
        return at(() -> instant, instant);
        return atInternal(
                new ScheduledCompletableFutureOnce<>(null, ZonedDateTime.ofInstant(instant, ZoneId.systemDefault())),
                callable);
    private <T> ScheduledCompletableFuture<T> atInternal(ScheduledCompletableFutureOnce<T> deferred,
        return afterInternal(deferred, callable);
        return schedule(runnable, null, temporalAdjuster);
        final ScheduledCompletableFutureRecurring<T> schedule = new ScheduledCompletableFutureRecurring<>(identifier,
                ZonedDateTime.now());
        schedule(schedule, runnable, identifier, temporalAdjuster);
        return schedule;
    private <T> void schedule(ScheduledCompletableFutureRecurring<T> recurringSchedule, SchedulerRunnable runnable,
            @Nullable String identifier, TemporalAdjuster temporalAdjuster) {
        final Temporal newTime = recurringSchedule.getScheduledTime().with(temporalAdjuster);
        final ScheduledCompletableFutureOnce<T> deferred = new ScheduledCompletableFutureOnce<>(identifier,
                ZonedDateTime.from(newTime));
        deferred.thenAccept(v -> {
            if (temporalAdjuster instanceof SchedulerTemporalAdjuster schedulerTemporalAdjuster) {
                if (!schedulerTemporalAdjuster.isDone(newTime)) {
                    schedule(recurringSchedule, runnable, identifier, temporalAdjuster);
            recurringSchedule.complete(v);
        recurringSchedule.setScheduledPromise(deferred);
        atInternal(deferred, () -> {
     * {@link ScheduledCompletableFuture} that is intended to keep track of jobs that only run recurring.
     * Calling get() on this class will only return if the job is stopped or if the related scheduler
     * determines the job is done.
     * @param <T> Data the job returns when finished
    private static class ScheduledCompletableFutureRecurring<T> extends ScheduledCompletableFutureOnce<T> {
        private @Nullable volatile ScheduledCompletableFuture<T> scheduledPromise;
        public ScheduledCompletableFutureRecurring(@Nullable String identifier, ZonedDateTime scheduledTime) {
            super(identifier, scheduledTime);
            exceptionally(e -> {
                        if (scheduledPromise instanceof ScheduledCompletableFuture promise) {
                            promise.cancel(true);
        void setScheduledPromise(ScheduledCompletableFuture<T> future) {
                if (isCancelled()) {
                    // if already cancelled stop the new future directly.
                    scheduledPromise = future;
                    future.getPromise().exceptionally(ex -> {
                        // if an error occurs in the scheduled job propagate to parent
                        ScheduledCompletableFutureRecurring.this.completeExceptionally(ex);
        public long getDelay(@Nullable TimeUnit timeUnit) {
            return scheduledPromise instanceof ScheduledCompletableFuture promise ? promise.getDelay(timeUnit) : 0;
        public ZonedDateTime getScheduledTime() {
            return scheduledPromise instanceof ScheduledCompletableFuture promise ? promise.getScheduledTime()
                    : super.getScheduledTime();
     * {@link ScheduledCompletableFuture} that is intended to keep track of jobs that only run once.
     * @param <T> Data the job returns when finished.
    private static class ScheduledCompletableFutureOnce<T> extends CompletableFuture<T>
            implements ScheduledCompletableFuture<T> {
        private ZonedDateTime scheduledTime;
        private @Nullable String identifier;
        public ScheduledCompletableFutureOnce(@Nullable String identifier, Duration duration) {
            this(identifier, ZonedDateTime.now().plusNanos(duration.toNanos()));
        public ScheduledCompletableFutureOnce(@Nullable String identifier, ZonedDateTime scheduledTime) {
            this.scheduledTime = scheduledTime;
        public CompletableFuture<T> getPromise() {
            ZonedDateTime scheduledTime = this.scheduledTime;
            if (timeUnit == null) {
            long remaining = scheduledTime.toInstant().toEpochMilli() - System.currentTimeMillis();
            return timeUnit.convert(remaining, TimeUnit.MILLISECONDS);
        public int compareTo(@Nullable Delayed timeUnit) {
            return timeUnit == null ? -1
                    : Long.compare(getDelay(TimeUnit.MILLISECONDS), timeUnit.getDelay(TimeUnit.MILLISECONDS));
            return scheduledTime;
     * Wraps the system call to get the current time to be able to manipulate it in a unit test.
    protected long currentTimeMillis() {
        return System.currentTimeMillis();
