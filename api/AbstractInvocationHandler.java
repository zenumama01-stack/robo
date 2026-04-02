package org.openhab.core.internal.common;
 * Common base class for synchronous and ansynchronous invocation handlers.
abstract class AbstractInvocationHandler<T> {
    private static final String MSG_TIMEOUT_R = "Timeout of {}ms exceeded while calling\n{}\nThread '{}' ({}) is in state '{}'\n{}";
    private static final String MSG_TIMEOUT_Q = "Timeout of {}ms exceeded while calling\n{}\nThe task was still queued.";
    private static final String MSG_DUPLICATE = "Thread occupied while calling method '{}' on '{}' because of another blocking call.\n\tThe other call was to '{}'.\n\tIt's thread '{}' ({}) is in state '{}'\n{}";
    private static final String MSG_ERROR = "An error occurred while calling method '{}' on '{}': {}";
    private final Logger logger = LoggerFactory.getLogger(AbstractInvocationHandler.class);
    private final SafeCallManager manager;
    private final T target;
    private final Object identifier;
    private final long timeout;
    private final @Nullable Consumer<Throwable> exceptionHandler;
    private final @Nullable Runnable timeoutHandler;
    AbstractInvocationHandler(SafeCallManager manager, T target, Object identifier, long timeout,
            @Nullable Consumer<Throwable> exceptionHandler, @Nullable Runnable timeoutHandler) {
        this.exceptionHandler = exceptionHandler;
        this.timeoutHandler = timeoutHandler;
    SafeCallManager getManager() {
    T getTarget() {
    Object getIdentifier() {
    long getTimeout() {
    Consumer<Throwable> getExceptionHandler() {
        return exceptionHandler;
    Runnable getTimeoutHandler() {
        return timeoutHandler;
    void handleExecutionException(Method method, ExecutionException e) {
        if (cause instanceof DuplicateExecutionException exception) {
            handleDuplicate(method, exception);
        } else if (cause instanceof InvocationTargetException exception) {
            handleException(method, exception);
    void handleException(Method method, InvocationTargetException e) {
        logger.error(MSG_ERROR, toString(method), target, cause == null ? "" : cause.getMessage(), e.getCause());
        Consumer<Throwable> localConsumer = exceptionHandler;
        if (localConsumer != null) {
            localConsumer.accept(cause == null ? e : cause);
    void handleDuplicate(Method method, DuplicateExecutionException e) {
        Thread thread = Objects.requireNonNull(e.getCallable().getThread());
        logger.debug(MSG_DUPLICATE, toString(method), target, toString(e.getCallable().getMethod()), thread.getName(),
                thread.threadId(), thread.getState().toString(), getStacktrace(thread));
    void handleTimeout(Method method, Invocation invocation) {
        final Thread thread = invocation.getThread();
        if (thread != null) {
            logger.debug(MSG_TIMEOUT_R, timeout, toString(invocation.getInvocationStack()), thread.getName(),
            logger.debug(MSG_TIMEOUT_Q, timeout, toString(invocation.getInvocationStack()));
        if (timeoutHandler instanceof Runnable handler) {
            handler.run();
    private String toString(Collection<Invocation> invocationStack) {
        return invocationStack.stream().map(invocation -> "\t'" + toString(invocation.getMethod()) + "' on '"
                + invocation.getInvocationHandler().getTarget() + "'").collect(Collectors.joining(" via\n"));
    private String getStacktrace(final Thread thread) {
        return Arrays.stream(elements).map(element -> "\tat " + element).collect(Collectors.joining("\n"));
    String toString(Method method) {
        return method.getDeclaringClass().getSimpleName() + "." + method.getName() + "()";
    Object invokeDirect(Invocation invocation) throws IllegalAccessException, IllegalArgumentException {
            manager.recordCallStart(invocation);
        } catch (DuplicateExecutionException e) {
            return invocation.getMethod().invoke(target, invocation.getArgs());
            handleException(invocation.getMethod(), e);
            manager.recordCallEnd(invocation);
