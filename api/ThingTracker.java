 * A {@link ThingTracker} can be used to track added and removed things. In
 * contrast to the {@link ThingRegistryChangeListener} the method
 * {@link ThingTracker#thingAdded(Thing, ThingTrackerEvent)} is called for every
 * thing, although it was added before the tracker was registered.
 * @author Simon Kaufmann - Added THING_REMOVING state
public interface ThingTracker {
    enum ThingTrackerEvent {
        THING_ADDED,
        THING_REMOVING,
        THING_REMOVED,
        THING_UPDATED,
        TRACKER_ADDED,
        TRACKER_REMOVED
     * This method is called for every thing that exists in the {@link ThingRegistryImpl} and for every added thing.
     * @param thing the thing which was added
     * @param thingTrackerEvent the event that occurred
    void thingAdded(Thing thing, ThingTrackerEvent thingTrackerEvent);
     * This method is called for every thing that is going to be removed from the {@link ThingRegistryImpl}. Moreover
     * the method is
     * called for every thing,
     * that exists in the {@link ThingRegistryImpl}, when the tracker is
     * unregistered.
     * @param thing the thing which was removed
    void thingRemoving(Thing thing, ThingTrackerEvent thingTrackerEvent);
     * This method is called for every thing that was removed from the {@link ThingRegistryImpl}. Moreover the method is
    void thingRemoved(Thing thing, ThingTrackerEvent thingTrackerEvent);
     * This method is called for every thing that was updated within the {@link ThingRegistryImpl}.
     * @param oldThing the old think
     * @param newThing the thing which was updated
    void thingUpdated(Thing oldThing, Thing newThing, ThingTrackerEvent thingTrackerEvent);
