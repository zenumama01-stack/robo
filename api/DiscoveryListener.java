 * The {@link DiscoveryListener} interface for receiving discovery events.
 * A class that is interested in processing discovery events fired synchronously by a {@link DiscoveryService} has to
 * implement this interface.
 * @author Andre Fuechsel - Added removeOlderThings
 * @see DiscoveryService
public interface DiscoveryListener {
     * Invoked synchronously when a {@link DiscoveryResult} has been created
     * by the according {@link DiscoveryService}.
     * <i>Hint:</i> This method could even be invoked for {@link DiscoveryResult}s, whose existence has already been
     * informed about.
     * @param source the discovery service which is the source of this event (not null)
     * @param result the discovery result (not null)
    void thingDiscovered(DiscoveryService source, DiscoveryResult result);
     * Invoked synchronously when an already existing {@code Thing} has been
     * marked to be deleted by the according {@link DiscoveryService}.
     * <i>Hint:</i> This method could even be invoked for {@link DiscoveryResult}s, whose removal has already been
     * @param thingUID the Thing UID to be removed (not null)
    void thingRemoved(DiscoveryService source, ThingUID thingUID);
     * Removes all results belonging to one of the given types that are older
     * than the given timestamp.
     * @param source the discovery service which is the source of this event (not
     * @param timestamp timestamp, all <b>older</b> results will be removed
     * @return collection of thing UIDs of all removed things
    Collection<ThingUID> removeOlderResults(DiscoveryService source, Instant timestamp,
            @Nullable Collection<ThingTypeUID> thingTypeUIDs, @Nullable ThingUID bridgeUID);
