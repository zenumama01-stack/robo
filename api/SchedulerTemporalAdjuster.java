 * Interface that extends {@link TemporalAdjuster} and adds more functionality.
 * This interface is passed to the scheduler for repeating schedules.
public interface SchedulerTemporalAdjuster extends TemporalAdjuster {
     * Used by the scheduler to determine if it should continue scheduling jobs.
     * If returns true the implementation of this interface determines the job
     * should not run again given. No new job will be scheduled.
     * @param temporal The temporal to determine if the next run should be scheduled
     * @return true if running is done and the job should not run anymore.
    boolean isDone(Temporal temporal);
