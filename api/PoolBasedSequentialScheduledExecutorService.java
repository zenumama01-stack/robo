import java.util.IdentityHashMap;
import java.util.concurrent.Executor;
import java.util.concurrent.RejectedExecutionException;
import java.util.concurrent.RunnableFuture;
import org.openhab.core.internal.common.WrappedScheduledExecutorService;
 * A ScheduledExecutorService that will sequentially perform the tasks like a
 * {@link java.util.concurrent.Executors#newSingleThreadScheduledExecutor} backed by a thread pool.
 * This is a drop in replacement to a ScheduledExecutorService with one thread to avoid a lot of threads created, idling
 * most of the time and wasting memory on low-end devices.
 * The mechanism to block the ScheduledExecutorService to run tasks concurrently is based on a chain of
 * {@link CompletableFuture}s.
 * Each instance has a reference to the last CompletableFuture and will call handleAsync to add a new task.
 * @author Jörg Sautter - Initial contribution
final class PoolBasedSequentialScheduledExecutorService implements ScheduledExecutorService {
    static class BasePoolExecutor extends WrappedScheduledExecutorService {
        protected final Logger logger = LoggerFactory.getLogger(BasePoolExecutor.class);
        private final String threadPoolName;
        private final AtomicInteger pending;
        private volatile int minimumPoolSize;
        public BasePoolExecutor(String threadPoolName, int corePoolSize, ThreadFactory threadFactory) {
            super(corePoolSize, threadFactory);
            this.threadPoolName = threadPoolName;
            // set to one does ensure at least one thread more than tasks running
            this.pending = new AtomicInteger(1);
        public synchronized void resizePool(int mandatoryPoolSize) {
            int corePoolSize = getCorePoolSize();
            if (minimumPoolSize > mandatoryPoolSize) {
                mandatoryPoolSize = minimumPoolSize;
            if (mandatoryPoolSize > corePoolSize) {
                // two more than needed, they will time out if there is no work for them im time
                setMaximumPoolSize(mandatoryPoolSize + 2);
                setCorePoolSize(mandatoryPoolSize);
            } else if (mandatoryPoolSize < corePoolSize) {
                // ensure we drop not needed threads, this is only needed under higher load when none of the
                // started threads have a chance to timeout
        public int getMinimumPoolSize() {
            return minimumPoolSize;
        public void setMinimumPoolSize(int minimumPoolSize) {
            this.minimumPoolSize = minimumPoolSize;
            resizePool(getCorePoolSize());
            logger.warn("shutdown() invoked on a shared thread pool '{}'. This is a bug, please submit a bug report",
                    threadPoolName, new IllegalStateException());
            logger.warn("shutdownNow() invoked on a shared thread pool '{}'. This is a bug, please submit a bug report",
    private final WorkQueueEntry empty;
    private final BasePoolExecutor pool;
    private final List<RunnableFuture<?>> scheduled;
    private final ScheduledFuture<?> cleaner;
    private @Nullable WorkQueueEntry tail;
    public PoolBasedSequentialScheduledExecutorService(BasePoolExecutor pool) {
        this.pool = pool;
        // prepare the WorkQueueEntry we are using when no tasks are pending
        RunnableCompletableFuture<?> future = new RunnableCompletableFuture<>();
        future.complete(null);
        empty = new WorkQueueEntry(null, null, future);
        // tracks scheduled tasks alive
        this.scheduled = new ArrayList<>();
        tail = empty;
        // clean up to ensure we do not keep references to old tasks
        cleaner = this.scheduleWithFixedDelay(() -> {
                scheduled.removeIf((sf) -> sf.isCancelled());
                if (tail == null) {
                    // the service is shutdown
                WorkQueueEntry entry = tail;
                while (entry.prev != null) {
                    if (entry.prev.future.isDone()) {
                        entry.prev = null;
                    entry = entry.prev;
                if (tail != empty && tail.future.isDone()) {
                    // replace the tail with empty to ensure we do not prevent GC
                // avoid cleaners of promptly created instances to run at the same time
                (System.nanoTime() % 13), 8, TimeUnit.SECONDS);
    public ScheduledFuture<?> schedule(@Nullable Runnable command, long delay, @Nullable TimeUnit unit) {
        return schedule((origin) -> pool.schedule(() -> {
            // we might block the thread here, in worst case new threads are spawned
            submitToWorkQueue(origin.join(), command, true).join();
        }, delay, unit));
    public <V> ScheduledFuture<V> schedule(@Nullable Callable<V> callable, long delay, @Nullable TimeUnit unit) {
            return submitToWorkQueue(origin.join(), callable, true).join();
    public ScheduledFuture<?> scheduleAtFixedRate(@Nullable Runnable command, long initialDelay, long period,
            @Nullable TimeUnit unit) {
        return schedule((origin) -> pool.scheduleAtFixedRate(() -> {
            CompletableFuture<?> submitted;
                submitted = submitToWorkQueue(origin.join(), command, true);
            } catch (RejectedExecutionException ex) {
                // the pool has been shutdown, scheduled tasks should cancel
            submitted.join();
        }, initialDelay, period, unit));
    public ScheduledFuture<?> scheduleWithFixedDelay(@Nullable Runnable command, long initialDelay, long delay,
        return schedule((origin) -> pool.scheduleWithFixedDelay(() -> {
        }, initialDelay, delay, unit));
    private <V> ScheduledFuture<V> schedule(
            Function<CompletableFuture<RunnableFuture<?>>, ScheduledFuture<V>> doSchedule) {
                throw new RejectedExecutionException("this scheduled executor has been shutdown before");
            CompletableFuture<RunnableFuture<?>> origin = new CompletableFuture<>();
            ScheduledFuture<V> future = doSchedule.apply(origin);
            scheduled.add((RunnableFuture<?>) future);
            origin.complete((RunnableFuture<?>) future);
            cleaner.cancel(false);
            scheduled.removeIf((sf) -> {
                sf.cancel(false);
            tail = null;
            // ensures we do not leak the internal cleaner as Runnable
            Set<@Nullable Runnable> runnables = Collections.newSetFromMap(new IdentityHashMap<>());
                if (sf.cancel(false)) {
                    runnables.add(sf);
            while (entry != null) {
                if (!entry.future.cancel(false)) {
                if (entry.origin != null) {
                    // entry has been submitted by a .schedule call
                    runnables.add(entry.origin);
                    // entry has been submitted by a .submit call
                    runnables.add(entry.future);
            return List.copyOf(runnables);
            return pool == null;
            return pool == null && tail.future.isDone();
    public boolean awaitTermination(long timeout, @Nullable TimeUnit unit) throws InterruptedException {
        long timeoutAt = System.currentTimeMillis() + unit.toMillis(timeout);
        while (!isTerminated()) {
            if (System.currentTimeMillis() > timeoutAt) {
            Thread.onSpinWait();
    public <T> Future<T> submit(@Nullable Callable<T> task) {
        return submitToWorkQueue(null, task, false);
    private CompletableFuture<?> submitToWorkQueue(RunnableFuture<?> origin, @Nullable Runnable task, boolean inPool) {
        Callable<?> callable = () -> {
            task.run();
        return submitToWorkQueue(origin, callable, inPool);
    private <T> CompletableFuture<T> submitToWorkQueue(@Nullable RunnableFuture<?> origin, @Nullable Callable<T> task,
            boolean inPool) {
        BiFunction<? super Object, Throwable, T> action = (result, error) -> {
            // ignore result & error, they are from the previous task
                return task.call();
            } catch (RuntimeException ex) {
                // a small hack to throw the Exception unchecked
                throw PoolBasedSequentialScheduledExecutorService.unchecked(ex);
                pool.pending.decrementAndGet();
        RunnableCompletableFuture<T> cf;
        boolean runNow;
            var mandatoryPoolSize = pool.pending.incrementAndGet();
            pool.resizePool(mandatoryPoolSize);
            // avoid waiting for one pool thread to finish inside a pool thread
            runNow = inPool && tail.future.isDone();
            if (runNow) {
                cf = new RunnableCompletableFuture<>(task);
                tail = new WorkQueueEntry(null, origin, cf);
                cf = tail.future.handleAsync(action, pool);
                cf.setCallable(task);
                tail = new WorkQueueEntry(tail, origin, cf);
            // ensure we do not wait for one pool thread to finish inside another pool thread
                cf.run();
        return cf;
    private static <E extends RuntimeException> E unchecked(Exception ex) throws E {
        throw (E) ex;
    public <T> Future<T> submit(@Nullable Runnable task, T result) {
        return submitToWorkQueue(null, () -> {
    public Future<?> submit(@Nullable Runnable task) {
        return submit(task, (Void) null);
    public <T> List<Future<T>> invokeAll(@Nullable Collection<? extends @Nullable Callable<T>> tasks)
        List<Future<T>> futures = new ArrayList<>();
        for (Callable<T> task : tasks) {
            futures.add(submit(task));
        // wait for all futures to complete
        for (Future<T> future : futures) {
                // ignore, we are just waiting here for the futures to complete
        return futures;
    public <T> List<Future<T>> invokeAll(@Nullable Collection<? extends @Nullable Callable<T>> tasks, long timeout,
            TimeUnit unit) throws InterruptedException {
            futures.add(submitToWorkQueue(null, task, false).orTimeout(timeout, unit));
    public <T> T invokeAny(@Nullable Collection<? extends @Nullable Callable<T>> tasks)
            throws InterruptedException, ExecutionException {
            return invokeAny(tasks, Long.MAX_VALUE);
        } catch (TimeoutException ex) {
            throw new AssertionError(ex);
    public <T> T invokeAny(@Nullable Collection<? extends @Nullable Callable<T>> tasks, long timeout,
            @Nullable TimeUnit unit) throws InterruptedException, ExecutionException, TimeoutException {
        return invokeAny(tasks, timeoutAt);
    private <T> T invokeAny(@Nullable Collection<? extends @Nullable Callable<T>> tasks, long timeoutAt)
        List<CompletableFuture<T>> futures = new ArrayList<>();
            futures.add(submitToWorkQueue(null, task, false));
        // wait for any future to complete
        while (timeoutAt >= System.currentTimeMillis()) {
            boolean allDone = true;
            for (CompletableFuture<T> future : futures) {
                if (future.isDone()) {
                    if (!future.isCompletedExceptionally()) {
                        // stop the others
                        for (CompletableFuture<T> tooLate : futures) {
                            if (tooLate != future) {
                                tooLate.cancel(true);
                        return future.join();
                    allDone = false;
            if (allDone) {
                ExecutionException exe = new ExecutionException("all tasks failed", null);
                        throw new AssertionError("all tasks should be failed");
                    } catch (ExecutionException ex) {
                        exe.addSuppressed(ex);
                throw exe;
        throw new TimeoutException("none of the tasks did complete in time");
        submit(command);
    static class WorkQueueEntry {
        private @Nullable WorkQueueEntry prev;
        private @Nullable RunnableFuture<?> origin;
        private final RunnableCompletableFuture<?> future;
        public WorkQueueEntry(@Nullable WorkQueueEntry prev, @Nullable RunnableFuture<?> origin,
                RunnableCompletableFuture<?> future) {
            this.prev = prev;
            this.origin = origin;
    static class RunnableCompletableFuture<V> extends CompletableFuture<V> implements RunnableFuture<V> {
        private @Nullable Callable<V> callable;
        public RunnableCompletableFuture() {
            this.callable = null;
        public RunnableCompletableFuture(@Nullable Callable<V> callable) {
            this.callable = callable;
        public void setCallable(@Nullable Callable<V> callable) {
        public <U> RunnableCompletableFuture<U> newIncompleteFuture() {
            return new RunnableCompletableFuture<>();
        public <U> RunnableCompletableFuture<U> handleAsync(BiFunction<? super V, Throwable, ? extends U> fn,
                Executor executor) {
            return (RunnableCompletableFuture<U>) super.handleAsync(fn, executor);
            if (this.isDone()) {
                // a FutureTask does also return here without exception
                this.complete(callable.call());
            } catch (Error | Exception t) {
                this.completeExceptionally(t);
