 * OSGi service to obtain a {@link SafeCallerBuilder}.
 * Safe-calls are used within the framework in order to protect it from hanging/blocking binding code and log meaningful
 * messages to detect and identify such hanging code.
public interface SafeCaller {
     * Default timeout for actions in milliseconds.
    long DEFAULT_TIMEOUT = TimeUnit.SECONDS.toMillis(5);
     * Create a safe call builder for the given object.
     * @param target the object on which calls should be protected by the safe caller
     * @param interfaceType the interface which defines the relevant methods
     * @return a safe call builder instance.
    <T> SafeCallerBuilder<@NonNull T> create(T target, Class<T> interfaceType);
