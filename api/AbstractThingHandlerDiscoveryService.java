import org.openhab.core.thing.binding.ThingHandlerService;
 * The {@link AbstractThingHandlerDiscoveryService} extends the {@link AbstractDiscoveryService} for thing-based
 * discovery services.
 * It handles the injection of the {@link ThingHandler}
public abstract class AbstractThingHandlerDiscoveryService<T extends ThingHandler> extends AbstractDiscoveryService
        implements ThingHandlerService {
    private final Logger logger = LoggerFactory.getLogger(AbstractThingHandlerDiscoveryService.class);
    private final Class<T> thingClazz;
    private boolean backgroundDiscoveryEnabled = false;
    // this works around a bug in ecj: @NonNullByDefault({}) complains about the field not being
    // initialized when the type is generic, so we have to initialize it with "something"
    protected @NonNullByDefault({}) T thingHandler = (@NonNull T) null;
     * @param thingClazz the {@link ThingHandler} class.
     * @param supportedThingTypes the list of Thing types which are supported (can be {@code null}).
     *            parameter supported.
     *            input parameter supported.
     * @throws IllegalArgumentException if {@code timeout < 0}.
    protected AbstractThingHandlerDiscoveryService(Class<T> thingClazz, @Nullable Set<ThingTypeUID> supportedThingTypes,
            int timeout, boolean backgroundDiscoveryEnabledByDefault, @Nullable String scanInputLabel,
        super(supportedThingTypes, timeout, backgroundDiscoveryEnabledByDefault, scanInputLabel, scanInputDescription);
        this.thingClazz = thingClazz;
     * Creates a new instance of this class with the specified parameters and with {@code scanInputLabel} and
     * {@code scanInputDescription} set to {@code null}.
            int timeout, boolean backgroundDiscoveryEnabledByDefault) throws IllegalArgumentException {
        this(thingClazz, supportedThingTypes, timeout, backgroundDiscoveryEnabledByDefault, null, null);
     * {@code scanInputDescription} set to {@code null}, and {@code backgroundDiscoveryEnabledByDefault} enabled.
            int timeout) throws IllegalArgumentException {
        this(thingClazz, supportedThingTypes, timeout, true);
     * {@code scanInputDescription} set to {@code null}, without any {@code supportedThingTypes}, and
     * {@code backgroundDiscoveryEnabledByDefault} enabled.
    protected AbstractThingHandlerDiscoveryService(Class<T> thingClazz, int timeout) throws IllegalArgumentException {
        this(thingClazz, null, timeout);
    protected AbstractThingHandlerDiscoveryService(ScheduledExecutorService scheduler, Class<T> thingClazz,
        super(scheduler, supportedThingTypes, timeout, backgroundDiscoveryEnabledByDefault, scanInputLabel,
                scanInputDescription);
        if (thingClazz.isAssignableFrom(handler.getClass())) {
            this.thingHandler = (T) handler;
                    "Expected class is " + thingClazz + " but the parameter has class " + handler.getClass());
    public @Nullable T getThingHandler() {
        return thingHandler;
    public void activate(@Nullable Map<String, Object> config) {
        // do not call super.activate here, otherwise the scan might be background scan might be started before the
        // thing handler is set. This is correctly handled in initialize
                    config.get(DiscoveryService.CONFIG_PROPERTY_BACKGROUND_DISCOVERY), Boolean.class,
                logger.debug("Background discovery for discovery service '{}' disabled.", getClass().getName());
                logger.debug("Background discovery for discovery service '{}' enabled.", getClass().getName());
        // do not call super.deactivate here, background scan is already handled in dispose
    public void initialize() {
