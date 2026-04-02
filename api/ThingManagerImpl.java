import org.openhab.core.thing.ThingTypeMigrationService;
import org.openhab.core.thing.internal.update.ThingUpdateInstruction;
import org.openhab.core.thing.internal.update.ThingUpdateInstructionReader;
import org.openhab.core.thing.internal.update.ThingUpdateInstructionReader.UpdateInstructionKey;
import org.openhab.core.thing.type.AbstractDescriptionType;
 * {@link ThingManagerImpl} tracks all things in the {@link ThingRegistry} and mediates the communication between the
 * {@link Thing} and the {@link ThingHandler} from the binding. It tracks {@link ThingHandlerFactory}s and
 * calls {@link ThingHandlerFactory#registerHandler(Thing)} for each thing, that was added to the {@link ThingRegistry}.
 * In addition, the {@link ThingManagerImpl} acts as an {@link org.openhab.core.internal.events.EventHandler}
 * and subscribes to update and command events.
 * Finally, the {@link ThingManagerImpl} implements the {@link ThingTypeMigrationService} to offer a way to change the
 * thing-type of a {@link Thing}.
 * @author Stefan Bußweiler - Added new thing status handling, migration to new event mechanism,
 *         refactorings due to thing/bridge life cycle
 * @author Simon Kaufmann - Added remove handling, type conversion
 * @author Kai Kreuzer - Removed usage of itemRegistry and thingLinkRegistry, fixed vetoing mechanism
 * @author Andre Fuechsel - Added the {@link ThingTypeMigrationService}
 * @author Thomas Höfer - Added localization of thing status info
 * @author Henning Sudbrock - Consider thing type properties when migrating to new thing type
 * @author Yordan Zhelev - Added thing disabling mechanism
 * @author Björn Lange - Ignore illegal thing status transitions instead of throwing IllegalArgumentException
 * @author Jan N. Klug - Add thing update mechanism
@Component(immediate = true, service = { ThingTypeMigrationService.class, ThingManager.class })
public class ThingManagerImpl implements ReadyTracker, ThingManager, ThingTracker, ThingTypeMigrationService {
    public static final String PROPERTY_THING_TYPE_VERSION = "thingTypeVersion";
    // interval to check if thing prerequisites are met (in s)
    private static final int CHECK_INTERVAL = 2;
    // time after we try to initialize a thing even if the thing-type is not registered (in s)
    private static final int MAX_CHECK_PREREQUISITE_TIME = 120;
    private static final int MAX_BRIDGE_NESTING = 50;
    private static final ReadyMarker READY_MARKER_THINGS_LOADED = new ReadyMarker("things", "handler");
    private static final String THING_STATUS_STORAGE_NAME = "thing_status_storage";
    private static final String FORCE_REMOVE_THREAD_POOL_NAME = "forceRemove";
    private static final String THING_MANAGER_THREAD_POOL_NAME = "thingManager";
    private final Logger logger = LoggerFactory.getLogger(ThingManagerImpl.class);
            .getScheduledPool(THING_MANAGER_THREAD_POOL_NAME);
    private final Map<ThingUID, ThingHandler> thingHandlers = new ConcurrentHashMap<>();
    private final Map<ThingHandlerFactory, Set<ThingHandler>> thingHandlersByFactory = new ConcurrentHashMap<>();
    private final Map<UpdateInstructionKey, List<ThingUpdateInstruction>> updateInstructions = new ConcurrentHashMap<>();
    private final Map<ThingUID, ThingPrerequisites> missingPrerequisites = new ConcurrentHashMap<>();
    private final Map<ThingUID, Thing> things = new ConcurrentHashMap<>();
    private final Map<ThingUID, Lock> thingLocks = new ConcurrentHashMap<>();
    private final Set<ThingUID> thingUpdatedLock = ConcurrentHashMap.newKeySet();
    protected final ChannelGroupTypeRegistry channelGroupTypeRegistry;
    protected final CommunicationManager communicationManager;
    protected final ConfigDescriptionRegistry configDescriptionRegistry;
    protected final ConfigDescriptionValidator configDescriptionValidator;
    protected final ItemChannelLinkRegistry itemChannelLinkRegistry;
    private final Storage<String> disabledStorage;
    protected final ThingRegistryImpl thingRegistry;
    private final ThingUpdateInstructionReader thingUpdateInstructionReader;
    private @Nullable ScheduledFuture<?> startLevelSetterJob = null;
    private @Nullable ScheduledFuture<?> prerequisiteCheckerJob = null;
    private final ThingHandlerCallback thingHandlerCallback = new ThingHandlerCallbackImpl(this);
    public ThingManagerImpl( //
            final @Reference CommunicationManager communicationManager,
            final @Reference ConfigDescriptionValidator configDescriptionValidator,
            final @Reference ReadyService readyService, //
            final @Reference StorageService storageService, //
            final @Reference ThingUpdateInstructionReader thingUpdateInstructionReader,
            final @Reference BundleResolver bundleResolver, final @Reference TranslationProvider translationProvider,
        this.communicationManager = communicationManager;
        this.configDescriptionValidator = configDescriptionValidator;
        this.thingRegistry = (ThingRegistryImpl) thingRegistry;
        this.thingUpdateInstructionReader = thingUpdateInstructionReader;
        this.disabledStorage = storageService.getStorage(THING_STATUS_STORAGE_NAME, this.getClass().getClassLoader());
        this.thingRegistry.addThingTracker(this);
                .withIdentifier(Integer.toString(StartLevelService.STARTLEVEL_STATES)));
        thingRegistry.removeThingTracker(this);
        for (ThingHandlerFactory factory : thingHandlerFactories) {
            removeThingHandlerFactory(factory);
        ScheduledFuture<?> startLevelSetterJob = this.startLevelSetterJob;
        if (startLevelSetterJob != null) {
            startLevelSetterJob.cancel(true);
            this.startLevelSetterJob = null;
        ScheduledFuture<?> prerequisiteCheckerJob = this.prerequisiteCheckerJob;
        if (prerequisiteCheckerJob != null) {
            prerequisiteCheckerJob.cancel(true);
            this.prerequisiteCheckerJob = null;
    protected void thingUpdated(final Thing thing) {
        thingUpdatedLock.add(thing.getUID());
        final Thing oldThing = thingRegistry.get(thing.getUID());
            throw new IllegalArgumentException(MessageFormat.format(
                    "Cannot update thing {0} because it is not known to the registry", thing.getUID().getAsString()));
        final Provider<Thing> provider = thingRegistry.getProvider(oldThing);
                    "Provider for thing {0} cannot be determined because it is not known to the registry",
                    thing.getUID().getAsString()));
        if (provider instanceof ManagedProvider managedProvider) {
            managedProvider.update(thing);
            logger.debug("Only updating thing {} in the registry because provider {} is not managed.",
                    thing.getUID().getAsString(), provider);
            thingRegistry.updated(provider, oldThing, thing);
        thingUpdatedLock.remove(thing.getUID());
            final @Nullable Configuration configuration) {
                    MessageFormat.format("No thing type {0} registered, cannot change thing type for thing {1}",
                            thingTypeUID.getAsString(), thing.getUID().getAsString()));
        scheduler.schedule(new Runnable() {
                Lock lock = getLockForThing(thingUID);
                    // Remove the ThingHandler, if any
                    final ThingHandlerFactory oldThingHandlerFactory = findThingHandlerFactory(thing.getThingTypeUID());
                    if (oldThingHandlerFactory != null) {
                            unregisterAndDisposeHandler(oldThingHandlerFactory, thing, thingHandler);
                            waitUntilHandlerUnregistered(thing, 60 * 1000);
                            logger.debug("No ThingHandler to dispose for {}", thing.getUID());
                        logger.debug("No ThingHandlerFactory available that can handle {}", thing.getThingTypeUID());
                    // Set the new channels
                    List<Channel> channels = ThingFactoryHelper.createChannels(thingType, thingUID,
                    ((ThingImpl) thing).setChannels(channels);
                    // Set the given configuration
                    Configuration editConfiguration = configuration != null ? configuration : new Configuration();
                    ThingFactoryHelper.applyDefaultConfiguration(editConfiguration, thingType,
                    ((ThingImpl) thing).setConfiguration(editConfiguration);
                    // Set the new properties (keeping old properties, unless they have the same name as a new property)
                    for (Entry<String, String> entry : thingType.getProperties().entrySet()) {
                    // set the new ThingTypeUID
                    ((ThingImpl) thing).setThingTypeUID(thingTypeUID);
                    // update the thing and register a new handler
                    thingUpdated(thing);
                    final ThingHandlerFactory newThingHandlerFactory = findThingHandlerFactory(thing.getThingTypeUID());
                    if (newThingHandlerFactory != null) {
                        registerAndInitializeHandler(thing, newThingHandlerFactory);
                    logger.debug("Changed ThingType of Thing {} to {}. New ThingHandler is {}.", thingUID,
                            thing.getThingTypeUID(), handler == null ? "NO HANDLER" : handler);
            private void waitUntilHandlerUnregistered(final Thing thing, int timeout) {
                for (int i = 0; i < timeout / 100; i++) {
                    if (thing.getHandler() == null && thingHandlers.get(thing.getUID()) == null) {
                        logger.debug("Waiting for handler deregistration to complete for thing {}. Took already {}ms.",
                                thing.getUID().getAsString(), (i + 1) * 100);
                String message = MessageFormat.format(
                        "Thing type migration failed for {0}. The handler deregistration did not complete within {1}ms.",
                        thing.getUID().getAsString(), timeout);
                throw new IllegalStateException(message);
        }, 0, TimeUnit.MILLISECONDS);
    public void thingAdded(Thing thing, ThingTrackerEvent thingTrackerEvent) {
            logger.error("A thing with UID '{}' is already tracked by ThingManager. This is a bug.", thing.getUID());
        ThingPrerequisites thingPrerequisites = new ThingPrerequisites(thing);
        if (!thingPrerequisites.isReady()) {
            missingPrerequisites.put(thing.getUID(), thingPrerequisites);
        eventPublisher.post(ThingEventFactory.createStatusInfoEvent(thing.getUID(), thing.getStatusInfo()));
        logger.debug("Thing '{}' is tracked by ThingManager.", thing.getUID());
        if (!isHandlerRegistered(thing)) {
            registerAndInitializeHandler(thing, getThingHandlerFactory(thing));
            logger.debug("Handler of tracked thing '{}' already registered.", thing.getUID());
    public void thingRemoving(Thing thing, ThingTrackerEvent thingTrackerEvent) {
        setThingStatus(thing, buildStatusInfo(ThingStatus.REMOVING, ThingStatusDetail.NONE));
        notifyThingHandlerAboutRemoval(thing);
    public void thingRemoved(final Thing thing, ThingTrackerEvent thingTrackerEvent) {
        logger.debug("Thing '{}' is no longer tracked by ThingManager.", thing.getUID());
        ThingHandler thingHandler = thingHandlers.get(thing.getUID());
            final ThingHandlerFactory thingHandlerFactory = findThingHandlerFactory(thing.getThingTypeUID());
            if (thingHandlerFactory != null) {
                unregisterAndDisposeHandler(thingHandlerFactory, thing, thingHandler);
                if (thingTrackerEvent == ThingTrackerEvent.THING_REMOVED) {
                    safeCaller.create(thingHandlerFactory, ThingHandlerFactory.class).build()
                            .removeThing(thing.getUID());
                logger.warn("Cannot unregister handler. No handler factory for thing '{}' found.", thing.getUID());
        if (!things.containsKey(thing.getUID())) {
            logger.error("Trying to remove thing '{}', but is not tracked by ThingManager. This is a bug.",
            if (!thing.equals(things.remove(thing.getUID()))) {
                        "Trying to remove thing '{}', but it is different from the thing with the same UID tracked by ThingManager. This is a bug.",
        missingPrerequisites.remove(thing.getUID());
    public void thingUpdated(Thing oldThing, Thing newThing, ThingTrackerEvent thingTrackerEvent) {
        ThingUID thingUID = newThing.getUID();
            normalizeThingConfiguration(oldThing);
            logger.debug("Failed to normalize configuration for old thing during update '{}': {}", oldThing.getUID(),
                    e.getValidationMessages(null));
            normalizeThingConfiguration(newThing);
            logger.warn("Failed to normalize configuration for new thing during update '{}': {}", newThing.getUID(),
        if (thingUpdatedLock.contains(thingUID)) {
            // called from the thing handler itself or during thing structure update, therefore it exists
            // and either is initializing/initialized and must not be informed (in order to prevent infinite loops)
            // or will be initialized after the update is done by the thing type update method itself
            replaceThing(oldThing, newThing);
            Lock lock = getLockForThing(newThing.getUID());
                ThingHandler thingHandler = replaceThing(oldThing, newThing);
                    if (ThingHandlerHelper.isHandlerInitialized(newThing)
                            || newThing.getStatus() == ThingStatus.INITIALIZING) {
                        oldThing.setHandler(null);
                        newThing.setHandler(thingHandler);
                            validate(newThing, thingTypeRegistry.getThingType(newThing.getThingTypeUID()));
                            safeCaller.create(thingHandler, ThingHandler.class).build().thingUpdated(newThing);
                            final ThingHandlerFactory thingHandlerFactory = findThingHandlerFactory(
                                    newThing.getThingTypeUID());
                                if (newThing instanceof Bridge bridge) {
                                    unregisterAndDisposeChildHandlers(bridge, thingHandlerFactory);
                                disposeHandler(newThing, thingHandler);
                                setThingStatus(newThing,
                                        buildStatusInfo(ThingStatus.UNINITIALIZED,
                                                ThingStatusDetail.HANDLER_CONFIGURATION_PENDING,
                                                e.getValidationMessages(null).toString()));
                                "Cannot notify handler about updated thing '{}', because handler is not initialized (thing must be in status UNKNOWN, ONLINE or OFFLINE).",
                        if (thingHandler.getThing() == newThing) {
                            logger.debug("Initializing handler of thing '{}'", newThing.getThingTypeUID());
                            initializeHandler(newThing);
                            logger.debug("Replacing uninitialized handler for updated thing '{}'",
                            ThingHandlerFactory thingHandlerFactory = getThingHandlerFactory(newThing);
                                unregisterHandler(thingHandler.getThing(), thingHandlerFactory);
                                logger.debug("No ThingHandlerFactory available that can handle {}",
                            registerAndInitializeHandler(newThing, thingHandlerFactory);
                    registerAndInitializeHandler(newThing, getThingHandlerFactory(newThing));
    private @Nullable ThingHandler replaceThing(Thing oldThing, Thing newThing) {
        final ThingHandler thingHandler = thingHandlers.get(newThing.getUID());
        if (oldThing != newThing) {
            if (!oldThing.equals(things.remove(oldThing.getUID()))) {
                logger.error("Thing '{}' is different from thing tracked by ThingManager. This is a bug.",
                        oldThing.getUID());
            things.put(newThing.getUID(), newThing);
    private @Nullable ThingHandlerFactory findThingHandlerFactory(ThingTypeUID thingTypeUID) {
        return thingHandlerFactories.stream().filter(factory -> factory.supportsThingType(thingTypeUID)).findFirst()
    private void registerHandler(Thing thing, ThingHandlerFactory thingHandlerFactory) {
        Lock lock = getLockForThing(thing.getUID());
            if (isHandlerRegistered(thing)) {
                logger.debug("Attempt to register a handler twice for thing {} at the same time will be ignored.",
            if (thing.getBridgeUID() == null) {
                doRegisterHandler(thing, thingHandlerFactory);
                Bridge bridge = getBridge(thing.getBridgeUID());
                if (bridge == null || !ThingHandlerHelper.isHandlerInitialized(bridge)) {
                    setThingStatus(thing,
                            buildStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.BRIDGE_UNINITIALIZED));
    private void doRegisterHandler(final Thing thing, final ThingHandlerFactory thingHandlerFactory) {
        logger.debug("Calling '{}.registerHandler()' for thing '{}'.", thingHandlerFactory.getClass().getSimpleName(),
            ThingHandler thingHandler = thingHandlerFactory.registerHandler(thing);
            thingHandler.setCallback(thingHandlerCallback);
            thing.setHandler(thingHandler);
            thingHandlers.put(thing.getUID(), thingHandler);
            thingHandlersByFactory.computeIfAbsent(thingHandlerFactory, unused -> new HashSet<>()).add(thingHandler);
            ThingStatusInfo statusInfo = buildStatusInfo(ThingStatus.UNINITIALIZED,
                    ThingStatusDetail.HANDLER_REGISTERING_ERROR,
                    ex.getCause() != null ? ex.getCause().getMessage() : ex.getMessage());
            setThingStatus(thing, statusInfo);
            logger.error("Exception occurred while calling thing handler factory '{}': {}", thingHandlerFactory,
                    ex.getMessage(), ex);
    protected void registerChildHandlers(final Bridge bridge) {
        for (final Thing child : bridge.getThings()) {
            logger.debug("Register and initialize child '{}' of bridge '{}'.", child.getUID(), bridge.getUID());
                    registerAndInitializeHandler(child, getThingHandlerFactory(child));
                    logger.error("Registration resp. initialization of child '{}' of bridge '{}' has been failed: {}",
                            child.getUID(), bridge.getUID(), ex.getMessage(), ex);
    protected void initializeHandler(Thing thing) {
        if (disabledStorage.containsKey(thingUID.getAsString())) {
            setThingStatus(thing, buildStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.DISABLED));
            logger.debug("Thing '{}' will not be initialized. It is marked as disabled.", thing.getUID());
            if (ThingHandlerHelper.isHandlerInitialized(thing)) {
                logger.debug("Attempt to initialize the already initialized thing '{}' will be ignored.",
            if (thing.getStatus() == ThingStatus.INITIALIZING) {
                logger.debug("Attempt to initialize a handler twice for thing '{}' at the same time will be ignored.",
                throw new IllegalStateException("Handler should not be null here");
            if (handler.getThing() != thing) {
                logger.warn("The model of {} is inconsistent [thing.getHandler().getThing() != thing]", thing.getUID());
                ThingFactoryHelper.applyDefaultConfiguration(thing.getConfiguration(), thingType,
                validate(thing, thingType);
                if (ThingStatus.REMOVING.equals(thing.getStatus())) {
                    // preserve REMOVING state so the callback can later decide to remove the thing after it has been
                    // initialized
                    logger.debug("Not setting status to INITIALIZING because thing '{}' is in REMOVING status.",
                    setThingStatus(thing, buildStatusInfo(ThingStatus.INITIALIZING, ThingStatusDetail.NONE));
                doInitializeHandler(handler);
                setThingStatus(thing, buildStatusInfo(ThingStatus.UNINITIALIZED,
                        ThingStatusDetail.HANDLER_CONFIGURATION_PENDING, e.getValidationMessages(null).toString()));
    private void validate(Thing thing, @Nullable ThingType thingType) throws ConfigValidationException {
        validate(thingType, thing.getUID(), thing.getConfiguration());
        // validate a bridge is set when it is mandatory
        if (thingType != null && thing.getBridgeUID() == null && !thingType.getSupportedBridgeTypeUIDs().isEmpty()) {
            ConfigValidationMessage message = new ConfigValidationMessage("bridge",
                    "Configuring a bridge is mandatory.", "bridge_not_configured");
            throw new ConfigValidationException(bundleContext.getBundle(), translationProvider, List.of(message));
            validate(channelType, channel.getUID(), channel.getConfiguration());
     * Determines if all 'required' configuration parameters are available in the configuration
     * @param prototype the "prototype", i.e. thing type or channel type
     * @param targetUID the UID of the thing or channel entity
     * @param configuration the current configuration
     * @throws ConfigValidationException if validation failed
    private void validate(@Nullable AbstractDescriptionType prototype, UID targetUID, Configuration configuration)
            throws ConfigValidationException {
        if (prototype == null) {
            logger.debug("Prototype for '{}' is not known, assuming it can be initialized", targetUID);
        URI configDescriptionURI = prototype.getConfigDescriptionURI();
            logger.debug("Config description URI for '{}' not found, assuming '{}' can be initialized",
                    prototype.getUID(), targetUID);
        configDescriptionValidator.validate(configuration.getProperties(), configDescriptionURI);
    private void normalizeThingConfiguration(Thing thing) throws ConfigValidationException {
            logger.warn("Could not normalize configuration for '{}' because the thing type was not found in registry.",
        normalizeConfiguration(thingType, thing.getThingTypeUID(), thing.getUID(), thing.getConfiguration());
                normalizeConfiguration(channelType, channelTypeUID, channel.getUID(), channel.getConfiguration());
    private void normalizeConfiguration(@Nullable AbstractDescriptionType prototype, UID prototypeUID, UID targetUID,
            Configuration configuration) throws ConfigValidationException {
            ConfigValidationMessage message = new ConfigValidationMessage("thing/channel",
                    "Type description {0} for {1} not found, although we checked the presence before.",
                    "type_description_missing", prototypeUID.toString(), targetUID.toString());
            logger.debug("Config description URI for '{}' not found, assuming '{}' is normalized", prototype.getUID(),
                    targetUID);
                    "Config description {0} for {1} not found, although we checked the presence before.",
                    "config_description_missing", configDescriptionURI.toString(), targetUID.toString());
        Objects.requireNonNull(ConfigUtil.normalizeTypes(configuration.getProperties(), List.of(configDescription)))
                .forEach(configuration::put);
    private void doInitializeHandler(final ThingHandler thingHandler) {
        logger.debug("Calling initialize handler for thing '{}' at '{}'.", thingHandler.getThing().getUID(),
                thingHandler);
        safeCaller.create(thingHandler, ThingHandler.class)
                .onTimeout(() -> logger.warn("Initializing handler for thing '{}' takes more than {}ms.",
                        thingHandler.getThing().getUID(), SafeCaller.DEFAULT_TIMEOUT))
                .onException(e -> {
                    setThingStatus(thingHandler.getThing(), buildStatusInfo(ThingStatus.UNINITIALIZED,
                            ThingStatusDetail.HANDLER_INITIALIZING_ERROR, e.getMessage()));
                    logger.error("Exception occurred while initializing handler of thing '{}': {}",
                            thingHandler.getThing().getUID(), e.getMessage(), e);
                }).build().initialize();
    private boolean isHandlerRegistered(Thing thing) {
        ThingHandler handler = thingHandlers.get(thing.getUID());
        return handler != null && handler == thing.getHandler();
    private @Nullable Bridge getBridge(@Nullable ThingUID bridgeUID) {
        if (bridgeUID == null) {
        Thing bridge = thingRegistry.get(bridgeUID);
        return bridge instanceof Bridge b ? b : null;
    private void unregisterHandler(Thing thing, ThingHandlerFactory thingHandlerFactory) {
                safeCaller.create(() -> doUnregisterHandler(thing, thingHandlerFactory), Runnable.class).build().run();
    private void doUnregisterHandler(final Thing thing, final ThingHandlerFactory thingHandlerFactory) {
        logger.debug("Calling unregisterHandler handler for thing '{}' at '{}'.", thing.getUID(), thingHandlerFactory);
        thingHandlerFactory.unregisterHandler(thing);
            thingHandler.setCallback(null);
        thing.setHandler(null);
        boolean enabled = !disabledStorage.containsKey(thingUID.getAsString());
        ThingStatusDetail detail = enabled ? ThingStatusDetail.HANDLER_MISSING_ERROR : ThingStatusDetail.DISABLED;
        setThingStatus(thing, buildStatusInfo(ThingStatus.UNINITIALIZED, detail));
        thingHandlers.remove(thing.getUID());
        synchronized (thingHandlersByFactory) {
            final Set<ThingHandler> thingHandlers = thingHandlersByFactory.get(thingHandlerFactory);
            if (thingHandlers != null) {
                thingHandlers.remove(thingHandler);
    private void disposeHandler(Thing thing, ThingHandler thingHandler) {
            doDisposeHandler(thingHandler);
                notifyBridgeAboutChildHandlerDisposal(thing, thingHandler);
    private void doDisposeHandler(final ThingHandler thingHandler) {
        logger.debug("Calling dispose handler for thing '{}' at '{}'.", thingHandler.getThing().getUID(), thingHandler);
        setThingStatus(thingHandler.getThing(), buildStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.NONE));
        safeCaller.create(thingHandler, ThingHandler.class) //
                .onTimeout(() -> logger.warn("Disposing handler for thing '{}' takes more than {}ms.",
                        thingHandler.getThing().getUID(), SafeCaller.DEFAULT_TIMEOUT)) //
                .onException(e -> logger.error("Exception occurred while disposing handler of thing '{}': {}",
                        thingHandler.getThing().getUID(), e.getMessage(), e)) //
                .build().dispose();
    private void unregisterAndDisposeChildHandlers(Bridge bridge, ThingHandlerFactory thingHandlerFactory) {
        ThingUID bridgeUID = bridge.getUID();
        thingRegistry.stream().filter(thing -> bridgeUID.equals(thing.getBridgeUID())).forEach(child -> {
            ThingHandler handler = child.getHandler();
                logger.debug("Unregister and dispose child '{}' of bridge '{}'.", child.getUID(), bridge.getUID());
                unregisterAndDisposeHandler(thingHandlerFactory, child, handler);
    private void unregisterAndDisposeHandler(ThingHandlerFactory thingHandlerFactory, Thing thing,
            ThingHandler handler) {
        disposeHandler(thing, handler);
        unregisterHandler(thing, thingHandlerFactory);
    protected void notifyThingsAboutBridgeStatusChange(final Bridge bridge, final ThingStatusInfo bridgeStatus) {
        if (ThingHandlerHelper.isHandlerInitialized(bridge)) {
                        if (handler != null && ThingHandlerHelper.isHandlerInitialized(child)) {
                            handler.bridgeStatusChanged(bridgeStatus);
                                "Exception occurred during notification about bridge status change on thing '{}': {}",
                                child.getUID(), e.getMessage(), e);
    protected void notifyBridgeAboutChildHandlerInitialization(final Thing thing) {
        final Bridge bridge = getBridge(thing.getBridgeUID());
        if (bridge != null) {
                    BridgeHandler bridgeHandler = bridge.getHandler();
                    if (bridgeHandler != null) {
                            bridgeHandler.childHandlerInitialized(thingHandler, thing);
                            "Exception occurred during bridge handler ('{}') notification about handler initialization of child '{}': {}",
                            bridge.getUID(), thing.getUID(), e.getMessage(), e);
    private void notifyBridgeAboutChildHandlerDisposal(final Thing thing, final ThingHandler thingHandler) {
            ThreadPoolManager.getPool(THING_MANAGER_THREAD_POOL_NAME).execute(() -> {
                        bridgeHandler.childHandlerDisposed(thingHandler, thing);
                            "Exception occurred during bridge handler ('{}') notification about handler disposal of child '{}': {}",
                            bridge.getUID(), thing.getUID(), ex.getMessage(), ex);
    protected void notifyThingHandlerAboutRemoval(final Thing thing) {
        logger.trace("Asking handler of thing '{}' to handle its removal.", thing.getUID());
                    handler.handleRemoval();
                    logger.trace("Handler of thing '{}' returned from handling its removal.", thing.getUID());
                    logger.trace("No handler of thing '{}' available, so deferring the removal call.", thing.getUID());
                logger.error("The ThingHandler caused an exception while handling the removal of its thing", ex);
    protected void notifyRegistryAboutForceRemove(final Thing thing) {
        logger.debug("Removal handling of thing '{}' completed. Going to remove it now.", thing.getUID());
        // call asynchronous to avoid deadlocks in thing handler
        ThreadPoolManager.getPool(FORCE_REMOVE_THREAD_POOL_NAME).execute(() -> {
                thingRegistry.forceRemove(thing.getUID());
            } catch (IllegalStateException ex) {
                logger.debug("Could not remove thing {}. Most likely because it is not managed.", thing.getUID(), ex);
                        "Could not remove thing {}, because an unknown Exception occurred. Most likely because it is not managed.",
                        thing.getUID(), ex);
    private void registerAndInitializeHandler(final Thing thing,
            final @Nullable ThingHandlerFactory thingHandlerFactory) {
            logger.debug("Not registering a handler at this point. Thing is disabled.");
                if (!missingPrerequisites.containsKey(thing.getUID())) {
                    if (thingRegistry.getProvider(thing) instanceof ManagedProvider
                            && checkAndPerformUpdate(thing, thingHandlerFactory)) {
                        normalizeThingConfiguration(thing);
                        logger.warn("Failed to normalize configuration for thing '{}': {}", thing.getUID(),
                    registerHandler(thing, thingHandlerFactory);
                    initializeHandler(thing);
                    setThingStatus(thing, buildStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.NOT_YET_READY));
                            "Not registering a handler at this point. The thing type '{}' is not fully loaded yet.",
                            thing.getThingTypeUID());
                        ThingStatusDetail.HANDLER_MISSING_ERROR, "Handler factory not found"));
                logger.debug("Not registering a handler at this point. No handler factory for thing '{}' found.",
    private @Nullable ThingHandlerFactory getThingHandlerFactory(Thing thing) {
        ThingHandlerFactory thingHandlerFactory = findThingHandlerFactory(thing.getThingTypeUID());
            return thingHandlerFactory;
        logger.debug("Not registering a handler at this point since no handler factory for thing '{}' found.",
    private Lock getLockForThing(ThingUID thingUID) {
        return Objects.requireNonNull(thingLocks.computeIfAbsent(thingUID, k -> new ReentrantLock()));
    private ThingStatusInfo buildStatusInfo(ThingStatus thingStatus, ThingStatusDetail thingStatusDetail,
        ThingStatusInfoBuilder statusInfoBuilder = ThingStatusInfoBuilder.create(thingStatus, thingStatusDetail);
        statusInfoBuilder.withDescription(description);
        return statusInfoBuilder.build();
    private ThingStatusInfo buildStatusInfo(ThingStatus thingStatus, ThingStatusDetail thingStatusDetail) {
        return buildStatusInfo(thingStatus, thingStatusDetail, null);
    protected void setThingStatus(Thing thing, ThingStatusInfo thingStatusInfo) {
        ThingStatusInfo oldStatusInfo = thingStatusInfoI18nLocalizationService.getLocalizedThingStatusInfo(thing, null);
        thing.setStatusInfo(thingStatusInfo);
        ThingStatusInfo newStatusInfo = thingStatusInfoI18nLocalizationService.getLocalizedThingStatusInfo(thing, null);
            eventPublisher.post(ThingEventFactory.createStatusInfoEvent(thing.getUID(), newStatusInfo));
            if (!oldStatusInfo.equals(newStatusInfo)) {
                eventPublisher.post(
                        ThingEventFactory.createStatusInfoChangedEvent(thing.getUID(), newStatusInfo, oldStatusInfo));
            logger.error("Could not post 'ThingStatusInfoEvent' event: {}", ex.getMessage(), ex);
    public void setEnabled(ThingUID thingUID, boolean enabled) {
        Thing thing = things.get(thingUID);
        persistThingEnableStatus(thingUID, enabled);
            logger.debug("Thing with the UID {} is unknown, cannot set its enabled status.", thingUID);
            // Enable a thing
            if (thing.isEnabled()) {
                logger.debug("Thing {} is already enabled.", thingUID);
            logger.debug("Thing {} will be enabled.", thingUID);
                // A handler is already registered for that thing. Try to initialize it.
                // No handler registered. Try to register handler and initialize the thing.
                registerAndInitializeHandler(thing, findThingHandlerFactory(thing.getThingTypeUID()));
                // Check if registration was successful
                            buildStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.HANDLER_MISSING_ERROR));
            if (!thing.isEnabled()) {
                logger.debug("Thing {} is already disabled.", thingUID);
            logger.debug("Thing {} will be disabled.", thingUID);
            boolean disposed = false;
                // Dispose handler if registered.
                if (thingHandler != null && thingHandlerFactory != null) {
            if (!disposed) {
                // Only set the correct status to the thing. There is no handler to be disposed
                updateChildThingStatusForDisabledBridges(bridge);
    private void updateChildThingStatusForDisabledBridges(Bridge bridge) {
        for (Thing childThing : bridge.getThings()) {
            ThingStatusDetail statusDetail = childThing.getStatusInfo().getStatusDetail();
            if (childThing.getStatus() == ThingStatus.UNINITIALIZED && statusDetail != ThingStatusDetail.DISABLED) {
                setThingStatus(childThing,
    private void persistThingEnableStatus(ThingUID thingUID, boolean enabled) {
        logger.debug("Thing with UID {} will be persisted as {}.", thingUID, enabled ? "enabled." : "disabled.");
            // Clear the disabled thing storage. Otherwise, the handler will NOT be initialized later.
            disabledStorage.remove(thingUID.getAsString());
            // Mark the thing as disabled in the storage.
            disabledStorage.put(thingUID.getAsString(), "");
    public boolean isEnabled(ThingUID thingUID) {
            return thing.isEnabled();
        logger.debug("Thing with UID {} is unknown. Will try to get the enabled status from the persistent storage.",
        return !disabledStorage.containsKey(thingUID.getAsString());
    private boolean checkAndPerformUpdate(Thing thing, ThingHandlerFactory factory) {
        final int currentThingTypeVersion = Integer
                .parseInt(thing.getProperties().getOrDefault(PROPERTY_THING_TYPE_VERSION, "0"));
        UpdateInstructionKey thingKey = new UpdateInstructionKey(factory, thing.getThingTypeUID());
        List<ThingUpdateInstruction> instructions = updateInstructions.getOrDefault(thingKey, List.of()).stream()
                .filter(ThingUpdateInstruction.applies(currentThingTypeVersion)).toList();
        if (instructions.isEmpty()) {
        // create a thing builder and apply the update instructions
        ThingBuilder thingBuilder = thing instanceof Bridge bridge ? BridgeBuilder.create(bridge)
                : ThingBuilder.create(thing);
        instructions.forEach(instruction -> instruction.perform(thing, thingBuilder));
        int newThingTypeVersion = instructions.getLast().getThingTypeVersion();
        thingBuilder.withProperty(PROPERTY_THING_TYPE_VERSION, String.valueOf(newThingTypeVersion));
        logger.info("Updating '{}' from version {} to {}", thing.getUID(), currentThingTypeVersion,
                newThingTypeVersion);
        Thing newThing = thingBuilder.build();
        thingUpdated(newThing);
        ThingPrerequisites thingPrerequisites = new ThingPrerequisites(newThing);
            missingPrerequisites.put(newThing.getUID(), thingPrerequisites);
    private void checkMissingPrerequisites() {
        Iterator<ThingPrerequisites> it = missingPrerequisites.values().iterator();
            ThingPrerequisites prerequisites = it.next();
                if (prerequisites.isReady()) {
                    Thing thing = things.get(prerequisites.thingUID);
                                    "Handler of thing '{}' already registered even though not all prerequisites were met.",
                        logger.warn("Found missing thing while checking prerequisites of thing '{}'",
                                prerequisites.thingUID);
                logger.error("Checking/initializing thing '{}' failed unexpectedly.", prerequisites.thingUID, e);
    protected synchronized void addThingHandlerFactory(ThingHandlerFactory thingHandlerFactory) {
        logger.debug("Thing handler factory '{}' added", thingHandlerFactory.getClass().getSimpleName());
        updateInstructions.putAll(thingUpdateInstructionReader.readForFactory(thingHandlerFactory));
        things.values().stream().filter(thing -> thingHandlerFactory.supportsThingType(thing.getThingTypeUID()))
                .forEach(thing -> {
                        registerAndInitializeHandler(thing, thingHandlerFactory);
                        logger.debug("Thing handler for thing '{}' already registered", thing.getUID());
    protected synchronized void removeThingHandlerFactory(ThingHandlerFactory thingHandlerFactory) {
        logger.debug("Thing handler factory '{}' removed", thingHandlerFactory.getClass().getSimpleName());
        final Set<ThingHandler> handlers = thingHandlersByFactory.remove(thingHandlerFactory);
        if (handlers != null) {
            for (ThingHandler thingHandler : handlers) {
                final Thing thing = thingHandler.getThing();
        updateInstructions.keySet().removeIf(key -> thingHandlerFactory.equals(key.factory()));
    private boolean allEnabledThingsAreInitialized() {
        for (Thing thing : things.values()) {
            if (!thing.isEnabled() || ThingHandlerHelper.isHandlerInitialized(thing)) {
            int bridgeNestingLevel = 0;
            boolean bridgeDisabled = false;
            while (bridge != null) {
                if (!bridge.isEnabled()) {
                    bridgeDisabled = true;
                bridge = getBridge(bridge.getBridgeUID());
                if (bridgeNestingLevel++ > MAX_BRIDGE_NESTING) {
                    logger.warn("Bridge nesting is too deep for thing '{}'", thing.getUID());
            if (bridgeDisabled) {
                logger.debug("Thing '{}' is not ready because its bridge is disabled.", thing.getUID());
        startLevelSetterJob = scheduler.scheduleWithFixedDelay(() -> {
            if (allEnabledThingsAreInitialized()) {
                readyService.markReady(READY_MARKER_THINGS_LOADED);
                    startLevelSetterJob.cancel(false);
        }, CHECK_INTERVAL, CHECK_INTERVAL, TimeUnit.SECONDS);
        prerequisiteCheckerJob = scheduler.scheduleWithFixedDelay(this::checkMissingPrerequisites, CHECK_INTERVAL,
                CHECK_INTERVAL, TimeUnit.SECONDS);
     * The {@link ThingPrerequisites} class is used to gather and check the pre-requisites of a given thing (i.e.
     * availability of the {@link ThingType} and all needed {@link ChannelType}s and {@link ConfigDescription}s).
    private class ThingPrerequisites {
        private final Set<ChannelTypeUID> channelTypeUIDs = new HashSet<>();
        private final Set<URI> configDescriptionUris = new HashSet<>();
        private int timesChecked = 0;
        public ThingPrerequisites(Thing thing) {
            thingUID = thing.getUID();
            thingTypeUID = thing.getThingTypeUID();
            thing.getChannels().stream().map(Channel::getChannelTypeUID).filter(Objects::nonNull)
                    .map(Objects::requireNonNull).distinct().forEach(channelTypeUIDs::add);
         * Check if all necessary information is present in the registries.
         * If a {@link ThingHandlerFactory} reports that it supports {@link ThingTypeUID} but the {@link ThingType}
         * can't be found in the {@link ThingTypeRegistry} this method also returns <code>true</code> after
         * {@link #MAX_CHECK_PREREQUISITE_TIME} s.
         * @return <code>true</code> if all pre-requisites are present, <code>false</code> otherwise
        public synchronized boolean isReady() {
            ThingTypeUID thingTypeUID = this.thingTypeUID;
            // thing-type
            if (thingTypeUID != null) {
                    this.thingTypeUID = null;
                        configDescriptionUris.add(configDescriptionUri);
                } else if (thingHandlerFactories.stream().anyMatch(f -> f.supportsThingType(thingTypeUID))) {
                    timesChecked++;
                    if (timesChecked > MAX_CHECK_PREREQUISITE_TIME / CHECK_INTERVAL) {
                                "A thing handler factory claims to support '{}' for thing '{}' for more than {}s, but the thing type can't be found in the registry. This should be fixed in the binding.",
                                thingTypeUID, thingUID, MAX_CHECK_PREREQUISITE_TIME);
            // channel types
            Iterator<ChannelTypeUID> it = channelTypeUIDs.iterator();
                ChannelType channelType = channelTypeRegistry.getChannelType(it.next());
            // config descriptions
            configDescriptionUris.removeIf(uri -> configDescriptionRegistry.getConfigDescription(uri) != null);
            if (thingTypeUID == null && (!channelTypeUIDs.isEmpty() || !configDescriptionUris.isEmpty())) {
                // the thing type is present, so most likely the bundle is fully initialized
                // if channel types or config descriptions are missing increase the timeout counter
                            "Channel types or config descriptions for thing '{}' are missing in the respective registry for more than {}s. In case it does not happen immediately after an upgrade, it should be fixed in the binding.",
                            thingUID, MAX_CHECK_PREREQUISITE_TIME);
                    channelTypeUIDs.clear();
                    configDescriptionUris.clear();
            boolean isReady = this.thingTypeUID == null && channelTypeUIDs.isEmpty() && configDescriptionUris.isEmpty();
                logger.debug("Check result is 'not ready': {}", this);
            return "ThingPrerequisites{thingUID=" + thingUID + ", thingTypeUID=" + thingTypeUID + ", channelTypeUIDs="
                    + channelTypeUIDs + ", configDescriptionUris=" + configDescriptionUris + "}";
