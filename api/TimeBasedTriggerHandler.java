import org.openhab.core.scheduler.SchedulerTemporalAdjuster;
 * Marker Interface for a {@link TriggerHandler} that contains a time based execution.
public interface TimeBasedTriggerHandler extends TriggerHandler {
     * Returns the {@link SchedulerTemporalAdjuster} which can be used to determine the next execution times.
    SchedulerTemporalAdjuster getTemporalAdjuster();
