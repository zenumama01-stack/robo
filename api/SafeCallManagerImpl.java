 * Manages the execution of safe-calls.
 * It therefore tracks the executions in order to detect parallel execution and offers some helper methods for the
 * invocation handlers.
public class SafeCallManagerImpl implements SafeCallManager {
    private final Logger logger = LoggerFactory.getLogger(SafeCallManagerImpl.class);
    private final Map<Object, Queue<Invocation>> queues = new HashMap<>();
    private final Map<Object, Invocation> activeIdentifiers = new HashMap<>();
    private final Map<Object, Invocation> activeAsyncInvocations = new HashMap<>();
    private final ScheduledExecutorService watcher;
    private final ExecutorService scheduler;
    private boolean enforceSingleThreadPerIdentifier;
    public SafeCallManagerImpl(ScheduledExecutorService watcher, ExecutorService scheduler,
            boolean enforceSingleThreadPerIdentifier) {
        this.watcher = watcher;
        this.enforceSingleThreadPerIdentifier = enforceSingleThreadPerIdentifier;
    public void recordCallStart(Invocation invocation) {
        synchronized (activeIdentifiers) {
            Invocation otherInvocation = activeIdentifiers.get(invocation.getIdentifier());
            if (enforceSingleThreadPerIdentifier && otherInvocation != null) {
                // another call to the same identifier is (still) running,
                // therefore queue it instead for async execution later on.
                // Inform the caller about the timeout by means of the exception.
                enqueue(invocation);
                throw new DuplicateExecutionException(otherInvocation);
            activeIdentifiers.put(invocation.getIdentifier(), invocation);
        if (invocation.getInvocationHandler() instanceof InvocationHandlerAsync) {
            watch(invocation);
    public void recordCallEnd(Invocation invocation) {
            activeIdentifiers.remove(invocation.getIdentifier());
        synchronized (activeAsyncInvocations) {
            activeAsyncInvocations.remove(invocation.getIdentifier());
        logger.trace("Finished {}", invocation);
        trigger(invocation.getIdentifier());
    public void enqueue(Invocation invocation) {
        synchronized (queues) {
            Queue<Invocation> queue = Objects
                    .requireNonNull(queues.computeIfAbsent(invocation.getIdentifier(), k -> new LinkedList<>()));
            queue.add(invocation);
    private void trigger(Object identifier) {
        logger.trace("Triggering submissions for '{}'", identifier);
            if (enforceSingleThreadPerIdentifier && activeIdentifiers.containsKey(identifier)) {
                logger.trace("Identifier '{}' is already running", identifier);
            if (activeAsyncInvocations.containsKey(identifier)) {
                logger.trace("Identifier '{}' is already scheduled for asynchronous execution", identifier);
            Invocation next = dequeue(identifier);
                logger.trace("Scheduling {} for asynchronous execution", next);
                activeAsyncInvocations.put(identifier, next);
                getScheduler().submit(next);
                logger.trace("Submitted {} for asynchronous execution", next);
    private void handlePotentialTimeout(Invocation invocation) {
        Object identifier = invocation.getIdentifier();
        Invocation activeAsyncInvocation = activeAsyncInvocations.get(identifier);
        if (activeAsyncInvocation == invocation) {
            Invocation activeInvocation = activeIdentifiers.get(identifier);
                invocation.getInvocationHandler().handleTimeout(invocation.getMethod(), activeInvocation);
    public @Nullable Invocation dequeue(Object identifier) {
            Queue<Invocation> queue = queues.get(identifier);
            if (queue != null) {
                return queue.poll();
    public @Nullable Invocation getActiveInvocation() {
            for (Invocation invocation : activeIdentifiers.values()) {
                if (invocation.getThread() == Thread.currentThread()) {
                    return invocation;
    public ExecutorService getScheduler() {
    private void watch(Invocation invocation) {
        watcher.schedule(() -> {
            handlePotentialTimeout(invocation);
        }, invocation.getTimeout(), TimeUnit.MILLISECONDS);
        logger.trace("Scheduling timeout watcher in {}ms", invocation.getTimeout());
    public void setEnforceSingleThreadPerIdentifier(boolean enforceSingleThreadPerIdentifier) {
