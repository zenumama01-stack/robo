 * Interface returned by all scheduled jobs. It can be used to wait for the value,
 * cancel the job or check how much time till the scheduled job will run.
public interface ScheduledCompletableFuture<T> extends ScheduledFuture<T> {
     * @return Returns the {@link CompletableFuture} associated with the scheduled job.
    CompletableFuture<T> getPromise();
     * @return Returns the timestamp the jobs is scheduled to run at.
    ZonedDateTime getScheduledTime();
