 * This interface is used by persistence services to represent the full persisted state of an item, including the
 * previous state, and last update and change timestamps.
 * It can be used in restoring the full state of an item.
public interface PersistedItem extends HistoricItem {
     * returns the timestamp of the last state change of the persisted item
     * @return the timestamp of the last state change of the item
    ZonedDateTime getLastStateChange();
    default Instant getLastStateChangeInstant() {
        ZonedDateTime lastStateChange = getLastStateChange();
        return lastStateChange != null ? lastStateChange.toInstant() : null;
     * returns the last state of the item
     * @return the last state
    State getLastState();
