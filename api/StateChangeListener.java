 * This interface must be implemented by all classes that want to be notified about changes in the state of an item.
 * The {@link GenericItem} class provides the possibility to register such listeners.
public interface StateChangeListener {
     * This method is called, if a state has changed.
     * @param item the item whose state has changed
     * @param oldState the previous state
     * @param newState the new state
    void stateChanged(Item item, State oldState, State newState);
     * This method is called, if a state was updated, but has not changed
     * @param item the item whose state was updated
     * @param state the current state, same before and after the update
    void stateUpdated(Item item, State state);
