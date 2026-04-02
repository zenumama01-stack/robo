 * Scheduler that runs the same job at the given periods.
public interface PeriodicScheduler {
     * Schedule a runnable to be executed in definitely at the given delays.
     * Schedules the job based on the given delay. If no more delays are present,
     * the last value is re-used. The method returns a {@link ScheduledCompletableFuture}
     * that can be used to stop scheduling. This is a fixed rate scheduler. That
     * is, a base time is established when this method is called and subsequent
     * firings are always calculated relative to this start time.
     * @param runnable the runnable to run after each duration
     * @param delays subsequent delays
     * @return returns a {@link ScheduledCompletableFuture} to cancel the schedule
    <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable runnable, Duration... delays);
