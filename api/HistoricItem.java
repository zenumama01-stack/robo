 * This interface is used by persistence services to represent an item
 * with a certain state at a given point in time.
 * Note that this interface does not extend {@link org.openhab.core.items.Item} as the persistence services could not
 * provide an implementation
 * that correctly implement getAcceptedXTypes() and getGroupNames().
public interface HistoricItem {
     * returns the timestamp of the persisted item
     * @return the timestamp of the item
    ZonedDateTime getTimestamp();
    default Instant getInstant() {
        return getTimestamp().toInstant();
     * returns the current state of the item
     * @return the current state
    State getState();
     * returns the name of the item
     * @return the name of the item
