import org.openhab.core.automation.module.script.action.Timer;
import org.openhab.core.scheduler.Scheduler;
 * This allows a script to call another script, which is available as a file.
@Component(immediate = true, service = ScriptExecution.class)
public class ScriptExecutionImpl implements ScriptExecution {
    private final Scheduler scheduler;
    public ScriptExecutionImpl(@Reference Scheduler scheduler) {
        this.scheduler = scheduler;
    public Timer createTimer(ZonedDateTime zonedDateTime, Runnable runnable) {
        return createTimer(null, zonedDateTime, runnable);
    public Timer createTimer(@Nullable String identifier, ZonedDateTime zonedDateTime, Runnable runnable) {
        return new TimerImpl(scheduler, zonedDateTime, runnable::run, identifier);
    public Timer createTimerWithArgument(ZonedDateTime zonedDateTime, Object arg1, Consumer<Object> consumer) {
        return createTimerWithArgument(null, zonedDateTime, arg1, consumer);
    public Timer createTimerWithArgument(@Nullable String identifier, ZonedDateTime zonedDateTime, Object arg1,
            Consumer<Object> consumer) {
        return new TimerImpl(scheduler, zonedDateTime, () -> consumer.accept(arg1), identifier);
