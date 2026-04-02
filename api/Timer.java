 * A timer is a handle for a block of code that is scheduled for future execution. A timer
 * can be canceled or rescheduled.
 * The script action "createTimer" returns a {@link Timer} instance.
public interface Timer {
     * Cancels the timer
     * @return true, if cancellation was successful
    boolean cancel();
     * Gets the scheduled exection time
     * @return the scheduled execution time, or null if the timer was cancelled
    ZonedDateTime getExecutionTime();
     * Determines whether the scheduled execution is yet to happen.
     * @return true, if the code is still scheduled to execute, false otherwise
    boolean isActive();
     * Determines whether the timer has been cancelled
     * @return true, if the timer has been cancelled, false otherwise
    boolean isCancelled();
     * Determines whether the scheduled code is currently executed.
     * @return true, if the code is being executed, false otherwise
    boolean isRunning();
     * Determines whether the scheduled execution has already terminated.
     * @return true, if the scheduled execution has already terminated, false otherwise
    boolean hasTerminated();
     * Reschedules a timer to a new starting time.
     * This can also be called after a timer has terminated, which will result in another
     * execution of the same code.
     * @param newTime the new time to execute the code
     * @return true, if the rescheduling was done successful
    boolean reschedule(ZonedDateTime newTime);
 * The {@link Timer} is a wrapper for the {@link org.openhab.core.automation.module.script.action.Timer}
 * interface. This is necessary because the implementation methods of an interface can't be called from
 * the script engine if the implementation is not in a public package or internal to the model bundle
public class Timer {
    private final org.openhab.core.automation.module.script.action.Timer timer;
    public Timer(org.openhab.core.automation.module.script.action.Timer timer) {
        this.timer = timer;
    public boolean cancel() {
        return timer.cancel();
    public @Nullable ZonedDateTime getExecutionTime() {
        return timer.getExecutionTime();
    public boolean isActive() {
        return timer.isActive();
    public boolean isCancelled() {
        return timer.isCancelled();
        return timer.isRunning();
    public boolean hasTerminated() {
        return timer.hasTerminated();
    public boolean reschedule(ZonedDateTime newTime) {
        return timer.reschedule(newTime);
