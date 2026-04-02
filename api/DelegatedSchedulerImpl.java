import java.time.temporal.TemporalAdjuster;
 * Wraps the actual Scheduler and keeps track of scheduled jobs.
 * It shuts down jobs in case the service is deactivated.
@Component(service = Scheduler.class, immediate = true)
public class DelegatedSchedulerImpl implements Scheduler {
    private final Set<ScheduledCompletableFuture<?>> scheduledJobs = new HashSet<>();
    private final SchedulerImpl delegate;
    public DelegatedSchedulerImpl(final @Reference SchedulerImpl scheduler) {
        this.delegate = scheduler;
        while (!scheduledJobs.isEmpty()) {
            final ScheduledCompletableFuture<?> scheduledJob;
            synchronized (scheduledJobs) {
                if (scheduledJobs.isEmpty()) {
                Iterator<ScheduledCompletableFuture<?>> iterator = scheduledJobs.iterator();
                scheduledJob = iterator.next();
            scheduledJob.cancel(true);
    public ScheduledCompletableFuture<Instant> after(Duration delay) {
        return add(delegate.after(delay));
    public <T> ScheduledCompletableFuture<T> after(Callable<T> callable, Duration delay) {
        return add(delegate.after(callable, delay));
    public <T> ScheduledCompletableFuture<T> before(CompletableFuture<T> promise, Duration timeout) {
        return add(delegate.before(promise, timeout));
    public ScheduledCompletableFuture<Instant> at(Instant instant) {
        return add(delegate.at(instant));
    public <T> ScheduledCompletableFuture<T> at(Callable<T> callable, Instant instant) {
        return add(delegate.at(callable, instant));
    public <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable runnable, TemporalAdjuster temporalAdjuster) {
        return add(delegate.schedule(runnable, temporalAdjuster));
    public <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable runnable, @Nullable String identifier,
            TemporalAdjuster temporalAdjuster) {
        return add(delegate.schedule(runnable, identifier, temporalAdjuster));
    private <T> ScheduledCompletableFuture<T> add(ScheduledCompletableFuture<T> t) {
            scheduledJobs.add(t);
        t.getPromise().handle((v, e) -> {
                scheduledJobs.remove(t);
