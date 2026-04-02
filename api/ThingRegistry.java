import org.openhab.core.thing.internal.ThingTracker;
 * {@link ThingRegistry} tracks all {@link Thing}s from different {@link ThingProvider}s and provides access to them.
 * The {@link ThingRegistry} supports adding of listeners (see {@link ThingRegistryChangeListener}) and trackers
 * (see {@link ThingTracker}).
 * @author Oliver Libutzki - Extracted ManagedThingProvider
 * @auther Thomas Höfer - Added config description validation exception to updateConfiguration operation
public interface ThingRegistry extends Registry<Thing, ThingUID> {
     * Returns a thing for a given UID or null if no thing was found.
     * @param uid thing UID
     * @return thing for a given UID or null if no thing was found
    Thing get(ThingUID uid);
     * Returns a channel for the given channel UID or null if no channel was found
     * @return channel for the given channel UID or null of no channel was found
     * Updates the configuration of a thing for the given UID.
     * @param configurationParameters configuration parameters
     * @throws ConfigValidationException if one or more of the given configuration parameters do not match
     *             their declarations in the configuration description
     * @throws IllegalArgumentException if no thing with the given UID exists
     * @throws IllegalStateException if no handler is attached to the thing
    void updateConfiguration(ThingUID thingUID, Map<String, Object> configurationParameters);
     * Initiates the removal process for the {@link Thing} specified by the given {@link ThingUID}.
     * Unlike in other {@link Registry}s, {@link Thing}s don't get removed immediately.
     * Instead, the corresponding {@link ThingHandler} is given the chance to perform
     * any required removal handling before it actually gets removed.
     * If for any reasons the {@link Thing} should be removed immediately without any prior processing, use
     * {@link #forceRemove(ThingUID)} instead.
     * @param thingUID Identificator of the {@link Thing} to be removed
     * @return the {@link Thing} that was removed, or null if no {@link Thing} with the given {@link ThingUID} exists
    Thing remove(ThingUID thingUID);
     * Removes the {@link Thing} specified by the given {@link ThingUID}.
     * If the corresponding {@link ThingHandler} should be given the chance to perform
     * any removal operations, use {@link #remove(ThingUID)} instead.
    Thing forceRemove(ThingUID thingUID);
     * Creates a thing based on the given configuration properties
     * @param thingTypeUID thing type unique id
     * @param thingUID thing unique id which should be created. This id might be
     *            null.
     * @param bridgeUID the thing's bridge. Null if there is no bridge or if the thing
     *            is a bridge by itself.
     * @param configuration the configuration
     * @return the created thing
    Thing createThingOfType(ThingTypeUID thingTypeUID, @Nullable ThingUID thingUID, @Nullable ThingUID bridgeUID,
            @Nullable String label, Configuration configuration);
