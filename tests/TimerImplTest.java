import org.openhab.core.automation.module.script.internal.action.TimerImpl;
import org.openhab.core.internal.scheduler.SchedulerImpl;
 * This are tests for {@link TimerImpl}.
public class TimerImplTest {
    // timer expires in 2 sec
    private static final long DEFAULT_TIMEOUT_SECONDS = 2;
    // runnable is running for 1 sec
    private static final long DEFAULT_RUNTIME_SECONDS = 1;
    private final SchedulerImpl scheduler = new SchedulerImpl();
    private @NonNullByDefault({}) Timer subject;
        subject = createTimer(ZonedDateTime.now().plusSeconds(DEFAULT_TIMEOUT_SECONDS), () -> {
            Thread.sleep(TimeUnit.SECONDS.toMillis(DEFAULT_RUNTIME_SECONDS));
    public void testTimerIsActiveAndCancel() {
        assertThat(subject.isActive(), is(true));
        assertThat(subject.hasTerminated(), is(false));
        assertThat(subject.isCancelled(), is(false));
        subject.cancel();
        assertThat(subject.isActive(), is(false));
        assertThat(subject.hasTerminated(), is(true));
        assertThat(subject.isCancelled(), is(true));
        subject.reschedule(ZonedDateTime.now().plusSeconds(DEFAULT_TIMEOUT_SECONDS));
    public void testTimerIsActiveAndTerminate() throws InterruptedException {
        Thread.sleep(TimeUnit.SECONDS.toMillis(DEFAULT_TIMEOUT_SECONDS + DEFAULT_RUNTIME_SECONDS + 3));
    public void testTimerHasTerminatedAndReschedule() throws InterruptedException {
    public void testTimerIsRunning() throws InterruptedException {
        assertThat(subject.isRunning(), is(false));
        Thread.sleep(TimeUnit.SECONDS.toMillis(DEFAULT_TIMEOUT_SECONDS) + 500);
        assertThat(subject.isRunning(), is(true));
        Thread.sleep(TimeUnit.SECONDS.toMillis(DEFAULT_RUNTIME_SECONDS + 3));
    private Timer createTimer(ZonedDateTime instant, SchedulerRunnable runnable) {
        return new TimerImpl(scheduler, instant, runnable);
