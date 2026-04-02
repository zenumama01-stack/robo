 * Implementation of the {@link SafeCaller} API.
@Component(configurationPid = "org.openhab.safecaller", immediate = true)
public class SafeCallerImpl implements SafeCaller {
    private static final String SAFE_CALL_POOL_NAME = "safeCall";
    private final SafeCallManagerImpl manager;
    public SafeCallerImpl(@Nullable Map<String, Object> properties) {
        watcher = Executors.newSingleThreadScheduledExecutor();
        manager = new SafeCallManagerImpl(watcher, getScheduler(), false);
    public void modified(@Nullable Map<String, Object> properties) {
            String enabled = (String) properties.get("singleThread");
            manager.setEnforceSingleThreadPerIdentifier("true".equalsIgnoreCase(enabled));
        watcher.shutdownNow();
    public <T> SafeCallerBuilder<@NonNull T> create(T target, Class<T> interfaceType) {
        return new SafeCallerBuilderImpl<>(target, new Class<?>[] { interfaceType }, manager);
    protected ExecutorService getScheduler() {
        return ThreadPoolManager.getPool(SAFE_CALL_POOL_NAME);
