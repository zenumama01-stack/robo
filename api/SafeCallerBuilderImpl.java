import java.lang.reflect.Proxy;
 * Builder implementation to create safe-call wrapper objects.
public class SafeCallerBuilderImpl<@NonNull T> implements SafeCallerBuilder<T> {
    private final Class<?>[] interfaceTypes;
    private long timeout;
    private Object identifier;
    private @Nullable Consumer<Throwable> exceptionHandler;
    private @Nullable Runnable timeoutHandler;
    private boolean async;
    public SafeCallerBuilderImpl(T target, Class<?>[] classes, SafeCallManager manager) {
        this.interfaceTypes = classes;
        this.timeout = SafeCaller.DEFAULT_TIMEOUT;
        this.identifier = target;
        this.async = false;
        InvocationHandler handler;
        if (async) {
            handler = new InvocationHandlerAsync<>(manager, target, identifier, timeout, exceptionHandler,
                    timeoutHandler);
            handler = new InvocationHandlerSync<>(manager, target, identifier, timeout, exceptionHandler,
        if (classLoader == null) {
                    "Cannot create proxy because '" + getClass().getName() + "' class loader is null");
        return (T) Proxy.newProxyInstance(
                CombinedClassLoader.fromClasses(classLoader,
                        Stream.concat(Stream.of(target.getClass()), Arrays.stream(interfaceTypes))),
                interfaceTypes, handler);
    public SafeCallerBuilder<T> withTimeout(long timeout) {
    public SafeCallerBuilder<T> withIdentifier(Object identifier) {
    public SafeCallerBuilder<T> onException(Consumer<Throwable> exceptionHandler) {
    public SafeCallerBuilder<T> onTimeout(Runnable timeoutHandler) {
    public SafeCallerBuilder<T> withAsync() {
        this.async = true;
