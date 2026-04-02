 * Common base interface for read and write tasks.
 * @param <R> request type
 * @param <C> callback type
public interface TaskWithEndpoint<R, C extends ModbusResultCallback, F extends ModbusFailureCallback<R>> {
     * Gets endpoint associated with this task
     * Gets request associated with this task
    R getRequest();
     * Gets the result callback associated with this task, will be called with response
    C getResultCallback();
     * Gets the failure callback associated with this task, will be called in case of an error
    F getFailureCallback();
    int getMaxTries();
