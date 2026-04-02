 * Synchronous invocation handler implementation.
public class InvocationHandlerSync<T> extends AbstractInvocationHandler<T> implements InvocationHandler {
    private static final String MSG_CONTEXT = "Already in a safe-call context, executing '{}' directly on '{}'.";
    private final Logger logger = LoggerFactory.getLogger(InvocationHandlerSync.class);
    public InvocationHandlerSync(SafeCallManager manager, T target, Object identifier, long timeout,
            Invocation invocation = new Invocation(this, method, args);
            Invocation activeInvocation = getManager().getActiveInvocation();
            if (activeInvocation != null) {
                    logger.debug(MSG_CONTEXT, toString(method), getTarget());
                    activeInvocation.getInvocationStack().push(invocation);
                    return invokeDirect(invocation);
                    activeInvocation.getInvocationStack().poll();
                Future<Object> future = getManager().getScheduler().submit(invocation);
                return future.get(getTimeout(), TimeUnit.MILLISECONDS);
                handleTimeout(method, invocation);
                handleExecutionException(method, e);
