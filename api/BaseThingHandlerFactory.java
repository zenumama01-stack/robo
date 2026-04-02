import org.openhab.core.config.core.status.ConfigStatusProvider;
import org.osgi.framework.ServiceObjects;
import org.osgi.service.component.annotations.ServiceScope;
 * The {@link BaseThingHandlerFactory} provides a base implementation for the {@link ThingHandlerFactory} interface.
 * @author Benedikt Niehues - fix for Bug https://bugs.eclipse.org/bugs/show_bug.cgi?id=445137 considering
 *         default values
 * @author Thomas Höfer - added config status provider and firmware update handler service registration
 * @author Stefan Bußweiler - API changes due to bridge/thing life cycle refactoring, removed OSGi service registration
 *         for thing handlers
 * @author Connor Petty - added osgi service registration for thing handler services.
public abstract class BaseThingHandlerFactory implements ThingHandlerFactory {
    private static final String THING_HANDLER_SERVICE_CANONICAL_NAME = ThingHandlerService.class.getCanonicalName();
    private final Logger logger = LoggerFactory.getLogger(BaseThingHandlerFactory.class);
    private final Map<String, ServiceRegistration<ConfigStatusProvider>> configStatusProviders = new ConcurrentHashMap<>();
    private final Map<String, ServiceRegistration<FirmwareUpdateHandler>> firmwareUpdateHandlers = new ConcurrentHashMap<>();
    private final Map<ThingUID, Set<RegisteredThingHandlerService<?>>> thingHandlerServices = new ConcurrentHashMap<>();
    private @NonNullByDefault({}) ServiceTracker<ThingTypeRegistry, ThingTypeRegistry> thingTypeRegistryServiceTracker;
    private @NonNullByDefault({}) ServiceTracker<ConfigDescriptionRegistry, ConfigDescriptionRegistry> configDescriptionRegistryServiceTracker;
     * Initializes the {@link BaseThingHandlerFactory}. If this method is overridden by a sub class, the implementing
     * method must call <code>super.activate(componentContext)</code> first.
     * @param componentContext component context (must not be null)
        thingTypeRegistryServiceTracker = new ServiceTracker<>(bundleContext, ThingTypeRegistry.class.getName(), null);
        thingTypeRegistryServiceTracker.open();
        configDescriptionRegistryServiceTracker = new ServiceTracker<>(bundleContext,
                ConfigDescriptionRegistry.class.getName(), null);
        configDescriptionRegistryServiceTracker.open();
     * Disposes the {@link BaseThingHandlerFactory}. If this method is overridden by a sub class, the implementing
     * method must call <code>super.deactivate(componentContext)</code> first.
        for (ServiceRegistration<ConfigStatusProvider> serviceRegistration : configStatusProviders.values()) {
            if (serviceRegistration != null) {
        for (ServiceRegistration<FirmwareUpdateHandler> serviceRegistration : firmwareUpdateHandlers.values()) {
        thingTypeRegistryServiceTracker.close();
        configDescriptionRegistryServiceTracker.close();
        configStatusProviders.clear();
        firmwareUpdateHandlers.clear();
        thingHandlerServices.clear();
     * Get the bundle context.
     * @throws IllegalArgumentException if the bundle thing handler is not active
        final BundleContext bundleContext = this.bundleContext;
        if (bundleContext != null) {
                    "The bundle context is missing (it seems your thing handler factory is used but not active).");
        ThingHandler thingHandler = createHandler(thing);
            throw new IllegalStateException(this.getClass().getSimpleName()
                    + " could not create a handler for the thing '" + thing.getUID() + "'.");
        if ((thing instanceof Bridge) && !(thingHandler instanceof BridgeHandler)) {
                    "Created handler of bridge '" + thing.getUID() + "' must implement the BridgeHandler interface.");
        registerConfigStatusProvider(thing, thingHandler);
        registerFirmwareUpdateHandler(thing, thingHandler);
        registerServices(thing, thingHandler);
    private void registerServices(Thing thing, ThingHandler thingHandler) {
        for (Class<? extends ThingHandlerService> c : thingHandler.getServices()) {
            if (!ThingHandlerService.class.isAssignableFrom(c)) {
                        "Should register service={} for thingUID={}, but it does not implement the interface ThingHandlerService.",
                        c.getCanonicalName(), thingUID);
            registerThingHandlerService(thingUID, thingHandler, c);
     * Registers a dynamic service for the given thing handler. The service must implement ThingHandlerService.
     * @param thingHandler The thing handler requesting a service to be registered.
     * @param klass The service class to register.
    public void registerService(ThingHandler thingHandler, Class<? extends ThingHandlerService> klass) {
        ThingUID thingUID = thingHandler.getThing().getUID();
        if (!ThingHandlerService.class.isAssignableFrom(klass)) {
                    klass.getCanonicalName(), thingUID);
        registerThingHandlerService(thingUID, thingHandler, klass);
    private <T extends ThingHandlerService> void registerThingHandlerService(ThingUID thingUID,
            ThingHandler thingHandler, Class<T> c) {
        RegisteredThingHandlerService<T> registeredService;
        Component component = c.getAnnotation(Component.class);
        if (component != null && component.enabled()) {
            if (component.scope() != ServiceScope.PROTOTYPE) {
                // then we cannot use it.
                logger.warn("Could not register service for class={}. Service must have a prototype scope",
                        c.getCanonicalName());
            if (component.service().length != 1 || component.service()[0] != c) {
                        "Could not register service for class={}. ThingHandlerService with @Component must only label itself as a service.",
        ServiceReference<T> serviceRef = bundleContext.getServiceReference(c);
        if (serviceRef != null) {
            ServiceObjects<T> serviceObjs = bundleContext.getServiceObjects(serviceRef);
            registeredService = new RegisteredThingHandlerService<>(serviceObjs);
                T serviceInstance = c.getConstructor().newInstance();
                registeredService = new RegisteredThingHandlerService<>(serviceInstance);
            } catch (NoSuchMethodException | SecurityException | InstantiationException | IllegalAccessException
                logger.warn("Could not register service for class={}", c.getCanonicalName(), e);
        String[] serviceNames = getAllInterfaces(c).stream()//
                .map(Class::getCanonicalName)
                // we only add specific ThingHandlerServices, i.e. those that derive from the
                // ThingHandlerService
                // interface, NOT the ThingHandlerService itself. We do this to register them as specific OSGi
                // services later, rather than as a generic ThingHandlerService.
                .filter(className -> className != null && !className.equals(THING_HANDLER_SERVICE_CANONICAL_NAME))
                .toArray(String[]::new);
        registeredService.initializeService(thingHandler, serviceNames);
        Objects.requireNonNull(thingHandlerServices.computeIfAbsent(thingUID, uid -> new HashSet<>()))
                .add(registeredService);
    private void unregisterServices(Thing thing) {
        Set<RegisteredThingHandlerService<?>> serviceRegs = thingHandlerServices.remove(thingUID);
        if (serviceRegs != null) {
            serviceRegs.forEach(RegisteredThingHandlerService::disposeService);
     * Returns all interfaces of the given class as well as all super classes.
     * @return A {@link List} of interfaces
    private static Set<Class<?>> getAllInterfaces(Class<?> clazz) {
        Set<Class<?>> interfaces = new HashSet<>();
            interfaces.addAll(Arrays.asList(superclazz.getInterfaces()));
        return interfaces;
     * Creates a {@link ThingHandler} for the given thing.
     * @return thing the created handler
    protected abstract @Nullable ThingHandler createHandler(Thing thing);
    private void registerConfigStatusProvider(Thing thing, ThingHandler thingHandler) {
        if (thingHandler instanceof ConfigStatusProvider) {
            ServiceRegistration<ConfigStatusProvider> serviceRegistration = registerAsService(thingHandler,
                    ConfigStatusProvider.class);
            configStatusProviders.put(thing.getUID().getAsString(), serviceRegistration);
    private void registerFirmwareUpdateHandler(Thing thing, ThingHandler thingHandler) {
        if (thingHandler instanceof FirmwareUpdateHandler) {
            ServiceRegistration<FirmwareUpdateHandler> serviceRegistration = registerAsService(thingHandler,
                    FirmwareUpdateHandler.class);
            firmwareUpdateHandlers.put(thing.getUID().getAsString(), serviceRegistration);
    private <T> ServiceRegistration<T> registerAsService(ThingHandler thingHandler, Class<T> type) {
        ServiceRegistration<T> serviceRegistration = (ServiceRegistration<T>) bundleContext
                .registerService(type.getName(), thingHandler, null);
        if (thingHandler != null) {
            removeHandler(thingHandler);
        unregisterConfigStatusProvider(thing);
        unregisterFirmwareUpdateHandler(thing);
        unregisterServices(thing);
     * This method is called when a thing handler should be removed. The
     * implementing caller can override this method to release specific
     * @param thingHandler thing handler to be removed
    protected void removeHandler(ThingHandler thingHandler) {
    private void unregisterConfigStatusProvider(Thing thing) {
        ServiceRegistration<ConfigStatusProvider> serviceRegistration = configStatusProviders
                .remove(thing.getUID().getAsString());
    private void unregisterFirmwareUpdateHandler(Thing thing) {
        ServiceRegistration<FirmwareUpdateHandler> serviceRegistration = firmwareUpdateHandlers
     * Returns the {@link ThingType} which is represented by the given {@link ThingTypeUID}.
     * @param thingTypeUID the unique id of the thing type
     * @return the thing type represented by the given unique id
    protected @Nullable ThingType getThingTypeByUID(ThingTypeUID thingTypeUID) {
        if (thingTypeRegistryServiceTracker == null) {
                    "Base thing handler factory has not been properly initialized. Did you forget to call super.activate()?");
        ThingTypeRegistry thingTypeRegistry = thingTypeRegistryServiceTracker.getService();
        if (thingTypeRegistry != null) {
            return thingTypeRegistry.getThingType(thingTypeUID);
     * Creates a thing based on given thing type uid.
     * @param thingTypeUID thing type uid (can not be null)
     * @param configuration (can not be null)
     * @param thingUID thingUID (can not be null)
     * @return thing (can be null, if thing type is unknown)
    protected @Nullable Thing createThing(ThingTypeUID thingTypeUID, Configuration configuration, ThingUID thingUID) {
        return createThing(thingTypeUID, configuration, thingUID, null);
     * @param thingTypeUID thing type uid (must not be null)
     * @param thingUID thingUID (can be null)
     * @param configuration (must not be null)
     * @param bridgeUID (can be null)
        ThingType thingType = getThingTypeByUID(thingTypeUID);
            return ThingFactory.createThing(thingType, effectiveUID, configuration, bridgeUID,
                    getConfigDescriptionRegistry());
    protected @Nullable ConfigDescriptionRegistry getConfigDescriptionRegistry() {
        if (configDescriptionRegistryServiceTracker == null) {
                    "Config Description Registry has not been properly initialized. Did you forget to call super.activate()?");
        return configDescriptionRegistryServiceTracker.getService();
    private class RegisteredThingHandlerService<T extends ThingHandlerService> {
        private final T serviceInstance;
        private @Nullable ServiceObjects<T> serviceObjects;
        private @Nullable ServiceRegistration<?> serviceRegistration;
        public RegisteredThingHandlerService(T serviceInstance) {
            this.serviceInstance = serviceInstance;
        public RegisteredThingHandlerService(ServiceObjects<T> serviceObjs) {
            this.serviceInstance = serviceObjs.getService();
            this.serviceObjects = serviceObjs;
        public void initializeService(ThingHandler handler, String[] serviceNames) {
            serviceInstance.setThingHandler(handler);
            if (serviceNames.length > 0) {
                ServiceRegistration<?> serviceReg = bundleContext.registerService(serviceNames, serviceInstance, null);
                if (serviceReg != null) {
                    serviceRegistration = serviceReg;
            serviceInstance.initialize();
        public void disposeService() {
            serviceInstance.dispose();
            ServiceRegistration<?> serviceReg = this.serviceRegistration;
                serviceReg.unregister();
            ServiceObjects<T> serviceObjs = this.serviceObjects;
            if (serviceObjs != null) {
                serviceObjs.ungetService(serviceInstance);
