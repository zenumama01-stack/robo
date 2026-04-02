import org.openhab.core.scheduler.ScheduledCompletableFuture;
import org.openhab.core.scheduler.SchedulerRunnable;
 * This is an implementation of the {@link Timer} interface.
public class TimerImpl implements Timer {
    // All access must be guarded by "this"
    private final SchedulerRunnable runnable;
    private final @Nullable String identifier;
    private ScheduledCompletableFuture<?> future;
    public TimerImpl(Scheduler scheduler, ZonedDateTime startTime, SchedulerRunnable runnable) {
        this(scheduler, startTime, runnable, null);
    public TimerImpl(Scheduler scheduler, ZonedDateTime startTime, SchedulerRunnable runnable,
            @Nullable String identifier) {
        this.runnable = runnable;
        future = scheduler.schedule(runnable, identifier, startTime.toInstant());
    public synchronized boolean cancel() {
        return future.cancel(true);
    public synchronized boolean reschedule(ZonedDateTime newTime) {
        future = scheduler.schedule(runnable, identifier, newTime.toInstant());
    public synchronized @Nullable ZonedDateTime getExecutionTime() {
        return future.isCancelled() ? null : future.getScheduledTime();
    public synchronized boolean isActive() {
        return !future.isDone();
    public synchronized boolean isCancelled() {
        return future.isCancelled();
    public synchronized boolean isRunning() {
        return isActive() && ZonedDateTime.now().isAfter(future.getScheduledTime());
    public synchronized boolean hasTerminated() {
        return future.isDone();
