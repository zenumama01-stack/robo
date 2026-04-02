import org.openhab.core.thing.internal.ThingTracker.ThingTrackerEvent;
 * Default implementation of {@link ThingRegistry}.
 * @author Simon Kaufmann - Added forceRemove
 * @author Chris Jackson - ensure thing added event is sent before linked events
public class ThingRegistryImpl extends AbstractRegistry<Thing, ThingUID, ThingProvider> implements ThingRegistry {
    private final Logger logger = LoggerFactory.getLogger(ThingRegistryImpl.class.getName());
    private final List<ThingTracker> thingTrackers = new CopyOnWriteArrayList<>();
    public ThingRegistryImpl() {
        super(ThingProvider.class);
     * Adds a thing tracker.
     * @param thingTracker
     *            the thing tracker
    public void addThingTracker(ThingTracker thingTracker) {
        notifyTrackerAboutAllThingsAdded(thingTracker);
        thingTrackers.add(thingTracker);
        Thing thing = get(thingUID);
                thingHandler.handleConfigurationUpdate(configurationParameters);
                throw new IllegalStateException("Thing with UID " + thingUID + " has no handler attached.");
            throw new IllegalArgumentException("Thing with UID " + thingUID + " does not exist.");
        return super.remove(thingUID);
            notifyTrackers(thing, null, ThingTrackerEvent.THING_REMOVING);
     * Removes a thing tracker.
    public void removeThingTracker(ThingTracker thingTracker) {
        notifyTrackerAboutAllThingsRemoved(thingTracker);
        thingTrackers.remove(thingTracker);
    protected void notifyListenersAboutAddedElement(Thing element) {
        postEvent(ThingEventFactory.createAddedEvent(element));
        notifyTrackers(null, element, ThingTrackerEvent.THING_ADDED);
    protected void notifyListenersAboutRemovedElement(Thing element) {
        notifyTrackers(element, null, ThingTrackerEvent.THING_REMOVED);
        postEvent(ThingEventFactory.createRemovedEvent(element));
    protected void notifyListenersAboutUpdatedElement(Thing oldElement, Thing element) {
        notifyTrackers(oldElement, element, ThingTrackerEvent.THING_UPDATED);
        postEvent(ThingEventFactory.createUpdateEvent(element, oldElement));
    protected void onAddElement(Thing thing) throws IllegalArgumentException {
        addThingToBridge(thing);
            addThingsToBridge(bridge);
    protected void onRemoveElement(Thing thing) {
        // needed because the removed element was taken from the storage and lost its dynamic state
        preserveDynamicState(thing);
        if (bridgeUID != null) {
            Thing bridge = this.get(bridgeUID);
            if (bridge instanceof BridgeImpl impl) {
                impl.removeThing(thing);
    protected void onUpdateElement(Thing oldThing, Thing thing) {
        // better call it explicitly here, even if it is called in onRemoveElement
        onRemoveElement(thing);
        onAddElement(thing);
    private void preserveDynamicState(Thing thing) {
        final Thing existingThing = get(thing.getUID());
        if (existingThing != null) {
            thing.setHandler(existingThing.getHandler());
            thing.setStatusInfo(existingThing.getStatusInfo());
    private void addThingsToBridge(Bridge bridge) {
        forEach(thing -> {
            if (bridgeUID != null && bridgeUID.equals(bridge.getUID())) {
                if (bridge instanceof BridgeImpl impl && !bridge.getThings().contains(thing)) {
                    impl.addThing(thing);
    private void addThingToBridge(Thing thing) {
            if (bridge instanceof BridgeImpl impl && !impl.getThings().contains(thing)) {
    private void notifyTrackers(@Nullable Thing oldThing, @Nullable Thing newThing, ThingTrackerEvent event) {
        for (ThingTracker thingTracker : thingTrackers) {
                switch (event) {
                    case THING_ADDED:
                            thingTracker.thingAdded(newThing, ThingTrackerEvent.THING_ADDED);
                            throw new IllegalArgumentException("newThing must not be null when thing is added");
                    case THING_REMOVING:
                            thingTracker.thingRemoving(oldThing, ThingTrackerEvent.THING_REMOVING);
                            throw new IllegalArgumentException("oldThing must not be null when thing is removed");
                    case THING_REMOVED:
                            thingTracker.thingRemoved(oldThing, ThingTrackerEvent.THING_REMOVED);
                    case THING_UPDATED:
                        if (oldThing != null && newThing != null) {
                            thingTracker.thingUpdated(oldThing, newThing, ThingTrackerEvent.THING_UPDATED);
                                    "oldThing and newThing must not be null when thing is removed");
                logger.error("Could not inform the ThingTracker '{}' about the '{}' event!", thingTracker, event.name(),
    private void notifyTrackerAboutAllThingsAdded(ThingTracker thingTracker) {
        for (Thing thing : getAll()) {
            thingTracker.thingAdded(thing, ThingTrackerEvent.TRACKER_ADDED);
    private void notifyTrackerAboutAllThingsRemoved(ThingTracker thingTracker) {
            thingTracker.thingRemoved(thing, ThingTrackerEvent.TRACKER_REMOVED);
        logger.debug("Creating thing for type '{}'.", thingTypeUID);
                            "Cannot create thing of type '{}'. Binding '{}' says it supports it, but it could not be created.",
                            thingTypeUID, thingHandlerFactory.getClass().getName());
        logger.warn("Cannot create thing. No binding found that supports creating a thing of type '{}'.", thingTypeUID);
    @Reference(cardinality = ReferenceCardinality.OPTIONAL, policy = ReferencePolicy.DYNAMIC, name = "ManagedThingProvider")
    protected void setManagedProvider(ManagedThingProvider provider) {
    protected void unsetManagedProvider(ManagedThingProvider managedProvider) {
