 * Builder to create a safe-call wrapper for another object.
public interface SafeCallerBuilder<@NonNull T> {
     * Creates a dynamic proxy with the according properties which guards the caller from hanging implementations in the
     * target object.
     * @return the dynamic proxy wrapping the target object
    T build();
     * Sets the timeout
     * @param timeout the timeout in milliseconds.
     * @return the SafeCallerBuilder itself
    SafeCallerBuilder<T> withTimeout(long timeout);
     * Specifies the identifier for the context in which only one thread may be occupied at the same time.
     * @param identifier the identifier much must have a proper hashcode()/equals() implementation in order to
     *            distinguish different contexts.
    SafeCallerBuilder<T> withIdentifier(Object identifier);
     * Specifies a callback in case of execution errors.
     * @param exceptionHandler
    SafeCallerBuilder<T> onException(Consumer<Throwable> exceptionHandler);
     * Specifies a callback in case of timeouts.
     * @param timeoutHandler
    SafeCallerBuilder<T> onTimeout(Runnable timeoutHandler);
     * Denotes that the calls should be executed asynchronously, i.e. that they should return immediately and not even
     * block until they reached the timeout.
     * By default, calls will be executed synchronously (i.e. blocking) until the timeout is reached.
    SafeCallerBuilder<T> withAsync();
