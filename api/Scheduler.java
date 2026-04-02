 * A Scheduler service provides timed semantics to CompletableFutures. A Scheduler can
 * delay a CompletableFutures, it can resolve a CompletableFutures at a certain time, or it can
 * provide a timeout to a CompletableFutures.
 * This scheduler has a millisecond resolution.
public interface Scheduler {
     * Return a {@link ScheduledCompletableFuture} that resolves after delaying with the result of the
     * call that is executed after the delay.
     * @param callable Provides the result
     * @param delay The duration to wait
     * @return A {@link ScheduledCompletableFuture}
    <T> ScheduledCompletableFuture<T> after(Callable<T> callable, Duration delay);
     * Return a {@link ScheduledCompletableFuture} that resolves at the given epochTime
     * @param instant The epoch time
    ScheduledCompletableFuture<Instant> at(Instant instant);
     * Return a {@link ScheduledCompletableFuture} that resolves at the given epochTime with the result of
     * the call.
    <T> ScheduledCompletableFuture<T> at(Callable<T> callable, Instant instant);
     * Return a {@link ScheduledCompletableFuture} that resolves at the given epochTime and runs the runnable.
     * @param runnable Runs at the given epochTime
    default ScheduledCompletableFuture<@Nullable Void> at(SchedulerRunnable runnable, Instant instant) {
        return at(() -> {
        }, instant);
     * Return a {@link ScheduledCompletableFuture} that fails with a {@link java.util.concurrent.TimeoutException}
     * when the given {@link CompletableFuture} is not resolved before the given timeout. If the
     * given {@link CompletableFuture} fails or is resolved before the timeout then the returned
     * {@link ScheduledCompletableFuture} will be treated accordingly. The cancellation does not influence
     * the final result of the given {@link CompletableFuture} since a {@link CompletableFuture}
     * can only be failed or resolved by its creator.
     * If the timeout is in the past then the {@link CompletableFuture} will be resolved
     * immediately
     * @param promise The {@link CompletableFuture} to base the returned {@link ScheduledCompletableFuture} on
     * @param timeout The number of milliseconds to wait.
    <T> ScheduledCompletableFuture<T> before(CompletableFuture<T> promise, Duration timeout);
     * Return a {@link ScheduledCompletableFuture} that will resolve after the given duration.
     * This {@link ScheduledCompletableFuture} can be cancelled.
     * @param delay The delay to wait
    ScheduledCompletableFuture<Instant> after(Duration delay);
     * Schedules the callable once or repeating using the temporalAdjuster to determine the
     * time the callable should run. Runs until the job is cancelled or if the temporalAdjuster
     * method {@link SchedulerTemporalAdjuster#isDone} returns true.
     * @param temporalAdjuster the temperalAdjuster to return the time the callable should run
    <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable callable, TemporalAdjuster temporalAdjuster);
     * @param identifier an optional identifier for this job
    <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable callable, @Nullable String identifier,
            TemporalAdjuster temporalAdjuster);
