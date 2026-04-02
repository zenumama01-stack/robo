 * {@link ProfileContext} implementation.
 * @author Jan N. Klug - Add accepted type methods
public class ProfileContextImpl implements ProfileContext {
    private static final String THREAD_POOL_NAME = "profiles";
    private final Configuration configuration;
    public ProfileContextImpl(Configuration configuration) {
        this(configuration, List.of(), List.of(), List.of());
    public ProfileContextImpl(Configuration configuration, List<Class<? extends State>> acceptedDataTypes,
            List<Class<? extends Command>> acceptedCommandTypes,
    public <T> T getConfigurationAs(Class<T> configurationClass) {
        return getConfiguration().as(configurationClass);
    public ScheduledExecutorService getExecutorService() {
        return ThreadPoolManager.getScheduledPool(THREAD_POOL_NAME);
        return acceptedDataTypes;
        return acceptedCommandTypes;
    public List<Class<? extends Command>> getHandlerAcceptedCommandTypes() {
        return handlerAcceptedCommandTypes;
