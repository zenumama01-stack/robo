 * This class wraps the ScheduledThreadPoolExecutor to implement the {@link #afterExecute(Runnable, Throwable)} method
 * and log the exception in case the scheduled runnable threw an exception. The error will otherwise go unnoticed
 * because an exception thrown in the runnable will simply end with no logging unless the user handles it. This
 * wrapper removes the burden for the user to always catch errors in scheduled runnables for logging, and it also
 * catches unchecked exceptions that can be the cause of very hard to catch bugs because no error is ever shown if the
 * user doesn't catch the error in the runnable itself.
 * @author Andrew Fiddian-Green - Added task duration logging
public class WrappedScheduledExecutorService extends ScheduledThreadPoolExecutor {
    final Logger logger = LoggerFactory.getLogger(WrappedScheduledExecutorService.class);
    private static final Duration DEFAULT_TIMEOUT = Duration.ofMillis(5000);
    private final Set<TimedAbstractTask> runningTasks;
    public WrappedScheduledExecutorService(int corePoolSize, ThreadFactory threadFactory) {
        runningTasks = ConcurrentHashMap.newKeySet(corePoolSize);
     * A base class that checks the time a scheduled task takes to complete, and if it takes too long,
     * it outputs a log message with the stack trace from whence the task was originally scheduled.
    private abstract class TimedAbstractTask {
        private final Exception stackTraceHolder;
        private Instant timeout;
        protected TimedAbstractTask() {
            this.stackTraceHolder = new Exception();
            this.timeout = Instant.MAX;
            logLongRunningTasks();
        protected void clockStart() {
            extendTimeout();
            runningTasks.add(this);
        protected void clockStop() {
            if (Instant.now().isAfter(timeout)) {
                logger.debug("Scheduled task took more than {}; it was created here: ", DEFAULT_TIMEOUT,
                        stackTraceHolder);
            runningTasks.remove(this);
        protected void extendTimeout() {
            timeout = Instant.now().plus(DEFAULT_TIMEOUT);
     * A running time checker for a {@link Runnable}
    private class TimedRunnable extends TimedAbstractTask implements Runnable {
        private final Runnable runnable;
        protected TimedRunnable(@Nullable Runnable runnable) {
            this.runnable = Objects.requireNonNull(runnable);
                clockStart();
                clockStop();
     * A running time checker for a {@link Callable}
    private class TimedCallable<V> extends TimedAbstractTask implements Callable<V> {
        private final Callable<V> callable;
        protected TimedCallable(@Nullable Callable<V> callable) {
            this.callable = Objects.requireNonNull(callable);
        public V call() throws Exception {
                return callable.call();
    protected void afterExecute(@Nullable Runnable r, @Nullable Throwable t) {
        Throwable actualThrowable = t;
        if (actualThrowable == null && r instanceof Future<?> f) {
            // The Future is the wrapper task around our scheduled Runnable. This is only "done" if an Exception
            // occurred, the Task was completed, or aborted. A periodic Task (scheduleWithFixedDelay etc.) is NEVER
            // "done" unless there was an Exception because the outer Task is always rescheduled.
            if (f.isDone()) {
                    // we are NOT interested in the result of the Future but we have to call get() to obtain a possible
                    // Exception from it
                    f.get();
                } catch (CancellationException ce) {
                    // ignore canceled tasks
                } catch (ExecutionException ee) {
                    actualThrowable = ee.getCause();
                } catch (InterruptedException ie) {
        if (actualThrowable != null) {
            logger.warn("Scheduled runnable ended with an exception: ", actualThrowable);
    public ScheduledFuture<?> schedule(@Nullable Runnable runnable, long delay, @Nullable TimeUnit unit) {
        Runnable r = logger.isDebugEnabled() ? new TimedRunnable(runnable) : runnable;
        return super.schedule(r, delay, unit);
    public ScheduledFuture<?> scheduleAtFixedRate(@Nullable Runnable runnable, long initialDelay, long period,
        return super.scheduleAtFixedRate(r, initialDelay, period, unit);
    public ScheduledFuture<?> scheduleWithFixedDelay(@Nullable Runnable runnable, long initialDelay, long delay,
        return super.scheduleWithFixedDelay(r, initialDelay, delay, unit);
        Callable<V> c = logger.isDebugEnabled() ? new TimedCallable<>(callable) : callable;
        return super.schedule(c, delay, unit);
     * On close, log all currently running tasks that have not yet finished.
        runningTasks.forEach(t -> {
            logger.debug("Scheduled task is still running during shutdown; it was created here: ", t.stackTraceHolder);
        runningTasks.clear();
     * Logs all currently running tasks that have exceeded the timeout. This method is called when either
     * a new {@link TimedAbstractTask} is created or when its respective task gets called. e.g. the task
     * is scheduled via 'scheduleWithFixedDelay()' (say) and its runnable is called repeatedly thereafter.
     * Using this approach provides more opportunities to log long running tasks. This method extends the
     * timeout of each logged task to reduce the frequency of messages in the log.
    private synchronized void logLongRunningTasks() {
        runningTasks.stream().filter(t -> Instant.now().isAfter(t.timeout)).forEach(t -> {
            logger.debug("Scheduled task is taking more than {}; it was created here: ", DEFAULT_TIMEOUT,
                    t.stackTraceHolder);
            t.extendTimeout();
