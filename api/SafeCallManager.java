 * Interface to the safe call manager which handles queuing and tracking of safe-call executions.
public interface SafeCallManager {
     * Track that the call to the target method starts.
     * @param invocation the wrapper around the actual call
    void recordCallStart(Invocation invocation);
     * Track that the call to the target method finished.
    void recordCallEnd(Invocation invocation);
     * Queue the given invocation for asynchronous execution.
     * @param invocation the call to the proxy
    void enqueue(Invocation invocation);
     * Get the safe-caller's executor service instance
     * @return the safe-caller's executor service
    ExecutorService getScheduler();
     * Get the active invocation if the current thread already is a safe-call thread.
     * @return the active invocation or {@code null}
    Invocation getActiveInvocation();
