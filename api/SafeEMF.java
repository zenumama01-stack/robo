 * Service interface to execute EMF methods in a single based thread.
public interface SafeEMF {
     * Calls the given function.
     * @param <T> the return type of the calling function
     * @param func the function to call
     * @return the return value of the called function
    <T> T call(Supplier<T> func);
    void call(Runnable func);
