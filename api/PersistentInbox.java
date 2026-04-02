import static org.openhab.core.config.discovery.inbox.InboxPredicates.forThingUID;
import org.openhab.core.config.discovery.inbox.events.InboxEventFactory;
import org.openhab.core.thing.Bridge;
import org.openhab.core.thing.ManagedThingProvider;
import org.openhab.core.thing.ThingRegistryChangeListener;
import org.openhab.core.thing.binding.ThingFactory;
import org.openhab.core.thing.binding.ThingHandlerFactory;
 * The {@link PersistentInbox} class is a concrete implementation of the {@link Inbox}.
 * This implementation uses the {@link DiscoveryServiceRegistry} to register itself as {@link DiscoveryListener} to
 * receive {@link DiscoveryResult} objects automatically from {@link DiscoveryService}s.
 * @author Dennis Nobel - Added automated removing of entries
 * @author Michael Grammling - Added dynamic configuration updates
 * @author Dennis Nobel - Added persistence support
 * @author Christoph Knauf - Added removeThingsForBridge and getPropsAndConfigParams
@Component(immediate = true, service = Inbox.class)
public final class PersistentInbox implements Inbox, DiscoveryListener, ThingRegistryChangeListener {
    // Internal enumeration to identify the correct type of the event to be fired.
    private enum EventType {
        ADDED,
        REMOVED,
        UPDATED
    private class TimeToLiveCheckingThread implements Runnable {
        private final PersistentInbox inbox;
        public TimeToLiveCheckingThread(PersistentInbox inbox) {
            Instant now = Instant.now();
                if (isResultExpired(result, now)) {
                    logger.debug("Inbox entry for thing '{}' is expired and will be removed.", result.getThingUID());
                    remove(result.getThingUID());
        private boolean isResultExpired(DiscoveryResult result, Instant now) {
            long ttl = result.getTimeToLive();
            if (ttl == DiscoveryResult.TTL_UNLIMITED) {
            return result.getCreationTime().plusSeconds(ttl).isBefore(now);
    private final Logger logger = LoggerFactory.getLogger(PersistentInbox.class);
    private final Set<InboxListener> listeners = new CopyOnWriteArraySet<>();
    private final DiscoveryServiceRegistry discoveryServiceRegistry;
    private final ManagedThingProvider managedThingProvider;
    private final Storage<DiscoveryResult> discoveryResultStorage;
    private final Map<DiscoveryResult, Class<?>> resultDiscovererMap = new ConcurrentHashMap<>();
    private @NonNullByDefault({}) ScheduledFuture<?> timeToLiveChecker;
    private @NonNullByDefault({}) ScheduledFuture<?> delayedDiscoveryResultProcessor;
    private final List<ThingHandlerFactory> thingHandlerFactories = new CopyOnWriteArrayList<>();
    public PersistentInbox(final @Reference StorageService storageService,
            final @Reference DiscoveryServiceRegistry discoveryServiceRegistry,
            final @Reference ThingRegistry thingRegistry, final @Reference ManagedThingProvider thingProvider,
            final @Reference ThingTypeRegistry thingTypeRegistry,
            final @Reference ConfigDescriptionRegistry configDescriptionRegistry) {
        // First set all member variables to ensure the object itself is initialized (as most as possible).
        this.discoveryResultStorage = storageService.getStorage(DiscoveryResult.class.getName(),
        this.discoveryServiceRegistry = discoveryServiceRegistry;
        this.managedThingProvider = thingProvider;
        this.configDescRegistry = configDescriptionRegistry;
        discoveryServiceRegistry.addDiscoveryListener(this);
        ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool("discovery");
        timeToLiveChecker = scheduler.scheduleWithFixedDelay(new TimeToLiveCheckingThread(this), 0, 30,
        delayedDiscoveryResultProcessor = scheduler.scheduleWithFixedDelay(
                () -> Set.copyOf(delayedDiscoveryResults.values()).forEach(this::internalAdd), 0, 15, TimeUnit.SECONDS);
        discoveryServiceRegistry.removeDiscoveryListener(this);
        timeToLiveChecker.cancel(true);
        delayedDiscoveryResultProcessor.cancel(true);
        delayedDiscoveryResults.values().forEach(dr -> dr.future.complete(false));
    public @Nullable Thing approve(ThingUID thingUID, @Nullable String label, @Nullable String newThingId) {
        List<DiscoveryResult> results = stream().filter(forThingUID(thingUID)).toList();
        if (results.isEmpty()) {
            throw new IllegalArgumentException("No Thing with UID " + thingUID.getAsString() + " in inbox");
        if (newThingId != null && newThingId.contains(AbstractUID.SEPARATOR)) {
            throw new IllegalArgumentException("New Thing ID " + newThingId + " must not contain multiple segments");
        DiscoveryResult result = results.getFirst();
        final Map<String, String> properties = new HashMap<>();
        final Map<String, @Nullable Object> configParams = new HashMap<>();
        getPropsAndConfigParams(result, properties, configParams);
        final Configuration config = new Configuration(configParams);
        ThingTypeUID thingTypeUID = result.getThingTypeUID();
        ThingUID newThingUID = thingUID;
        if (newThingId != null) {
            String newUID = thingUID.getAsString().substring(0,
                    thingUID.getAsString().lastIndexOf(AbstractUID.SEPARATOR) + 1) + newThingId;
                newThingUID = new ThingUID(newUID);
                throw new IllegalArgumentException("Invalid thing UID " + newUID, e);
        Thing newThing = ThingFactory.createThing(newThingUID, config, properties, result.getBridgeUID(), thingTypeUID,
                thingHandlerFactories);
        if (newThing == null) {
            logger.warn("Cannot create thing. No binding found that supports creating a thing of type {}.",
                    thingTypeUID);
        if (label != null && !label.isEmpty()) {
            newThing.setLabel(label);
            newThing.setLabel(result.getLabel());
        addThingSafely(newThing);
        return newThing;
    private Map<ThingUID, DiscoveryResultWrapper> delayedDiscoveryResults = new ConcurrentHashMap<>();
    public synchronized CompletableFuture<Boolean> add(final @Nullable DiscoveryResult discoveryResult)
        if (discoveryResult == null) {
            return CompletableFuture.completedFuture(false);
        CompletableFuture<Boolean> future = new CompletableFuture<>();
        internalAdd(new DiscoveryResultWrapper(discoveryResult, future));
        return future;
    private void internalAdd(DiscoveryResultWrapper discoveryResultWrapper) {
        DiscoveryResult discoveryResult = discoveryResultWrapper.discoveryResult;
        // if we already have a result for the same ThingUID that is not added yet, delete it from the delayed map
        delayedDiscoveryResults.remove(discoveryResult.getThingUID());
        ThingType thingType = thingTypeRegistry.getThingType(discoveryResult.getThingTypeUID());
        if (thingType == null) {
            discoveryResultWrapper.retryCount++;
            if (discoveryResultWrapper.retryCount >= 20) {
                        "ThingTypeUID {} for discovery result with ThingUID {} not found, retried 20 times, aborting",
                        discoveryResult.getThingTypeUID(), discoveryResult.getThingUID());
                discoveryResultWrapper.future.complete(false);
                logger.trace(
                        "ThingTypeUID {} for discovery result with ThingUID {} not found, delaying add, retry {}/20",
                        discoveryResult.getThingTypeUID(), discoveryResult.getThingUID(),
                        discoveryResultWrapper.retryCount);
                delayedDiscoveryResults.put(discoveryResult.getThingUID(), discoveryResultWrapper);
        List<String> configurationParameters = getConfigDescParams(thingType).stream()
                .map(ConfigDescriptionParameter::getName).toList();
        discoveryResult.normalizePropertiesOnConfigDescription(configurationParameters);
        Thing thing = thingRegistry.get(thingUID);
            DiscoveryResult inboxResult = get(thingUID);
            if (inboxResult == null) {
                discoveryResultStorage.put(discoveryResult.getThingUID().toString(), discoveryResult);
                notifyListeners(discoveryResult, EventType.ADDED);
                logger.info("Added new thing '{}' to inbox.", thingUID);
                discoveryResultWrapper.future.complete(true);
                if (inboxResult instanceof DiscoveryResultImpl resultImpl) {
                    resultImpl.synchronize(discoveryResult);
                    discoveryResultStorage.put(discoveryResult.getThingUID().toString(), resultImpl);
                    notifyListeners(resultImpl, EventType.UPDATED);
                    logger.debug("Updated discovery result for '{}'.", thingUID);
                    logger.warn("Cannot synchronize result with implementation class '{}'.",
                            inboxResult.getClass().getName());
        } else if (managedThingProvider.get(thingUID) != null) {
            // only try to update properties if thing is managed
                    "Discovery result with thing '{}' not added as inbox entry. It is already present as thing in the ThingRegistry.",
                    thingUID);
            boolean updated = synchronizeConfiguration(discoveryResult.getThingTypeUID(),
                    discoveryResult.getProperties(), thing.getConfiguration());
            if (updated) {
                logger.debug("The configuration for thing '{}' is updated...", thingUID);
                managedThingProvider.update(thing);
    private boolean synchronizeConfiguration(ThingTypeUID thingTypeUID, Map<String, Object> properties,
            Configuration config) {
        boolean configUpdated = false;
        final Set<Map.Entry<String, Object>> propertySet = properties.entrySet();
        final ThingType thingType = thingTypeRegistry.getThingType(thingTypeUID);
        final List<ConfigDescriptionParameter> configDescParams = getConfigDescParams(thingType);
        for (Map.Entry<String, Object> propertyEntry : propertySet) {
            final String propertyKey = propertyEntry.getKey();
            final Object propertyValue = propertyEntry.getValue();
            // Check if the key is present in the configuration.
            if (!config.containsKey(propertyKey)) {
            // Normalize first
            ConfigDescriptionParameter configDescParam = getConfigDescriptionParam(configDescParams, propertyKey);
            Object normalizedValue = ConfigUtil.normalizeType(propertyValue, configDescParam);
            // If the value is equal to the one of the configuration, there is nothing to do.
            if (Objects.equals(normalizedValue, config.get(propertyKey))) {
            // - the given key is part of the configuration
            // - the values differ
            // update value
            config.put(propertyKey, normalizedValue);
            configUpdated = true;
        return configUpdated;
    private @Nullable ConfigDescriptionParameter getConfigDescriptionParam(
            List<ConfigDescriptionParameter> configDescParams, String paramName) {
        for (ConfigDescriptionParameter configDescriptionParameter : configDescParams) {
            if (configDescriptionParameter.getName().equals(paramName)) {
                return configDescriptionParameter;
    public void addInboxListener(@Nullable InboxListener listener) throws IllegalStateException {
    public List<DiscoveryResult> getAll() {
        return stream().toList();
    public Stream<DiscoveryResult> stream() {
        return (Stream<DiscoveryResult>) discoveryResultStorage.getValues().stream().filter(Objects::nonNull);
    public synchronized boolean remove(@Nullable ThingUID thingUID) throws IllegalStateException {
            DiscoveryResult discoveryResult = get(thingUID);
                if (!isInRegistry(thingUID)) {
                    removeResultsForBridge(thingUID);
                resultDiscovererMap.remove(discoveryResult);
                discoveryResultStorage.remove(thingUID.toString());
                notifyListeners(discoveryResult, EventType.REMOVED);
    public void removeInboxListener(@Nullable InboxListener listener) throws IllegalStateException {
    public void thingDiscovered(DiscoveryService source, DiscoveryResult result) {
        add(result).thenAccept(success -> {
                resultDiscovererMap.put(result, source.getClass());
    public void thingRemoved(DiscoveryService source, ThingUID thingUID) {
        remove(thingUID);
    public @Nullable Collection<ThingUID> removeOlderResults(DiscoveryService source, Instant timestamp,
            @Nullable Collection<ThingTypeUID> thingTypeUIDs, @Nullable ThingUID bridgeUID) {
        for (DiscoveryResult discoveryResult : getAll()) {
            Class<?> discoverer = resultDiscovererMap.get(discoveryResult);
            if (thingTypeUIDs != null && thingTypeUIDs.contains(discoveryResult.getThingTypeUID())
                    && discoveryResult.getCreationTime().isBefore(timestamp)
                    && (discoverer == null || source.getClass() == discoverer)) {
                if (bridgeUID == null || bridgeUID.equals(discoveryResult.getBridgeUID())) {
                    removedThings.add(thingUID);
                    logger.debug("Removed thing '{}' from inbox because it was older than {}.", thingUID, timestamp);
        return removedThings;
    public void added(Thing thing) {
        if (remove(thing.getUID())) {
                    "Discovery result for thing '{}' removed from inbox, because it was added as a Thing to the ThingRegistry.",
                    thing.getUID());
    public void removed(Thing thing) {
        if (thing instanceof Bridge) {
            removeResultsForBridge(thing.getUID());
    public void updated(Thing oldThing, Thing thing) {
        // Attention: Do NOT fire an event back to the ThingRegistry otherwise circular
        // events are fired! This event was triggered by the 'add(DiscoveryResult)'
        // method within this class. -> NOTHING TO DO HERE
    public void setFlag(ThingUID thingUID, @Nullable DiscoveryResultFlag flag) {
        DiscoveryResult result = get(thingUID);
        if (result instanceof DiscoveryResultImpl resultImpl) {
            resultImpl.setFlag((flag == null) ? DiscoveryResultFlag.NEW : flag);
            discoveryResultStorage.put(resultImpl.getThingUID().toString(), resultImpl);
        } else if (result == null) {
            logger.warn("Cannot set flag for result '{}' because it can't be found in storage", thingUID);
            logger.warn("Cannot set flag for result of instance type '{}'", result.getClass().getName());
     * Returns the {@link DiscoveryResult} in this {@link Inbox} associated with
     * the specified {@code Thing} ID, or {@code null}, if no {@link DiscoveryResult} could be found.
     * @param thingUID the Thing UID to which the discovery result should be returned
     * @return the discovery result associated with the specified Thing ID, or
     *         null, if no discovery result could be found
    private @Nullable DiscoveryResult get(ThingUID thingUID) {
        return discoveryResultStorage.get(thingUID.toString());
    private void notifyListeners(DiscoveryResult result, EventType type) {
        for (InboxListener listener : listeners) {
                    case ADDED:
                        listener.thingAdded(this, result);
                    case REMOVED:
                        listener.thingRemoved(this, result);
                    case UPDATED:
                        listener.thingUpdated(this, result);
                logger.error("Cannot notify the InboxListener '{}' about a Thing {} event!",
                        listener.getClass().getName(), type.name(), ex);
        // in case of EventType added/updated the listeners might have modified the result in the discoveryResultStorage
        final DiscoveryResult resultForEvent;
        if (type == EventType.REMOVED) {
            resultForEvent = result;
            resultForEvent = get(result.getThingUID());
            if (resultForEvent == null) {
        postEvent(resultForEvent, type);
    private void postEvent(DiscoveryResult result, EventType eventType) {
        EventPublisher eventPublisher = this.eventPublisher;
                switch (eventType) {
                        eventPublisher.post(InboxEventFactory.createAddedEvent(result));
                        eventPublisher.post(InboxEventFactory.createRemovedEvent(result));
                        eventPublisher.post(InboxEventFactory.createUpdatedEvent(result));
                logger.error("Could not post event of type '{}'.", eventType.name(), ex);
    private boolean isInRegistry(ThingUID thingUID) {
        return thingRegistry.get(thingUID) != null;
    private void removeResultsForBridge(ThingUID bridgeUID) {
        for (ThingUID thingUID : getResultsForBridge(bridgeUID)) {
    private List<ThingUID> getResultsForBridge(ThingUID bridgeUID) {
        List<ThingUID> thingsForBridge = new ArrayList<>();
        for (DiscoveryResult result : discoveryResultStorage.getValues()) {
            if (result != null && bridgeUID.equals(result.getBridgeUID())) {
                thingsForBridge.add(result.getThingUID());
        return thingsForBridge;
     * Get the properties and configuration parameters for the thing with the given {@link DiscoveryResult}.
     * @param discoveryResult the DiscoveryResult
     * @param props the location the properties should be stored to.
     * @param configParams the location the configuration parameters should be stored to.
    private void getPropsAndConfigParams(final DiscoveryResult discoveryResult, final Map<String, String> props,
            final Map<String, @Nullable Object> configParams) {
        final List<ConfigDescriptionParameter> configDescParams = getConfigDescParams(discoveryResult);
        final Set<String> paramNames = getConfigDescParamNames(configDescParams);
        final Map<String, Object> resultProps = discoveryResult.getProperties();
        for (Entry<String, Object> resultEntry : resultProps.entrySet()) {
            String resultKey = resultEntry.getKey();
            Object resultValue = resultEntry.getValue();
            if (paramNames.contains(resultKey)) {
                ConfigDescriptionParameter param = getConfigDescriptionParam(configDescParams, resultKey);
                Object normalizedValue = ConfigUtil.normalizeType(resultValue, param);
                configParams.put(resultKey, normalizedValue);
                props.put(resultKey, String.valueOf(resultValue));
    private Set<String> getConfigDescParamNames(List<ConfigDescriptionParameter> configDescParams) {
        Set<String> paramNames = new HashSet<>();
        for (ConfigDescriptionParameter param : configDescParams) {
            paramNames.add(param.getName());
        return paramNames;
    private List<ConfigDescriptionParameter> getConfigDescParams(DiscoveryResult discoveryResult) {
        return getConfigDescParams(thingType);
    private List<ConfigDescriptionParameter> getConfigDescParams(@Nullable ThingType thingType) {
        if (thingType != null && thingType.getConfigDescriptionURI() != null) {
            URI descURI = thingType.getConfigDescriptionURI();
            if (descURI != null) {
                ConfigDescription desc = configDescRegistry.getConfigDescription(descURI);
                if (desc != null) {
                    return desc.getParameters();
    private void addThingSafely(Thing thing) {
        ThingUID thingUID = thing.getUID();
        if (thingRegistry.get(thingUID) != null) {
            thingRegistry.remove(thingUID);
        thingRegistry.add(thing);
    void setTimeToLiveCheckingInterval(int interval) {
        timeToLiveChecker = ThreadPoolManager.getScheduledPool("discovery")
                .scheduleWithFixedDelay(new TimeToLiveCheckingThread(this), 0, interval, TimeUnit.SECONDS);
    void setDiscoveryResultAddRetryInterval(int interval) {
        delayedDiscoveryResultProcessor = ThreadPoolManager.getScheduledPool("discovery").scheduleWithFixedDelay(
                () -> Set.copyOf(delayedDiscoveryResults.values()).forEach(this::internalAdd), 0, interval,
    protected void addThingHandlerFactory(ThingHandlerFactory thingHandlerFactory) {
        this.thingHandlerFactories.add(thingHandlerFactory);
    protected void removeThingHandlerFactory(ThingHandlerFactory thingHandlerFactory) {
        this.thingHandlerFactories.remove(thingHandlerFactory);
    private static class DiscoveryResultWrapper {
        public final CompletableFuture<Boolean> future;
        public final DiscoveryResult discoveryResult;
        public int retryCount = 0;
        public DiscoveryResultWrapper(DiscoveryResult discoveryResult, CompletableFuture<Boolean> future) {
            this.future = future;
