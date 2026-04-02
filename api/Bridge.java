package org.openhab.core.thing;
import org.openhab.core.thing.binding.BridgeHandler;
 * A {@link Bridge} is a {@link Thing} that connects other {@link Thing}s.
 * @author Christoph Weitkamp - Added method {@code getThing(ThingUID)}
public interface Bridge extends Thing {
     * Gets the thing for the given UID or null if no thing with the UID exists.
     * @param thingUID thing UID
     * @return the thing for the given UID or null if no thing with the UID exists
    Thing getThing(ThingUID thingUID);
     * Returns the children of the bridge.
     * @return children
    List<Thing> getThings();
     * Gets the bridge handler.
     * @return the handler which can be null for a Thing that is not initialized. Note that a Bridge is
     *         guaranteed to be initialized before its children. It is therefore safe to call getBridge().getHandler()
     *         for a subordinate Thing
    BridgeHandler getHandler();
