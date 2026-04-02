import org.openhab.core.scheduler.PeriodicScheduler;
 * Implementation of a {@link PeriodicScheduler}.
 * @author Hilbrand Bouwkamp - moved periodic scheduling to it's own interface
@Component(service = PeriodicScheduler.class)
public class PeriodicSchedulerImpl implements PeriodicScheduler {
    public PeriodicSchedulerImpl(final @Reference Scheduler scheduler) {
    public <T> ScheduledCompletableFuture<T> schedule(SchedulerRunnable runnable, Duration... delays) {
        return scheduler.schedule(runnable, new PeriodicAdjuster(delays));
