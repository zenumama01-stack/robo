import java.lang.reflect.InvocationHandler;
 * Asynchronuous {@link InvocationHandler} implementation.
 * Instead of directly invoking the called method it rather queues it in the {@link SafeCallManager} for asynchronous
 * execution.
class InvocationHandlerAsync<T> extends AbstractInvocationHandler<T> implements InvocationHandler {
    InvocationHandlerAsync(SafeCallManager manager, T target, Object identifier, long timeout,
        super(manager, target, identifier, timeout, exceptionHandler, timeoutHandler);
    public @Nullable Object invoke(Object proxy, @Nullable Method method, @Nullable Object @Nullable [] args)
            throws Throwable {
        if (method != null) {
                getManager().enqueue(new Invocation(this, method, args));
                handleDuplicate(method, e);
