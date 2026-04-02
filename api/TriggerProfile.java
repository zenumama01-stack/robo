 * A {@link TriggerProfile} specifies the communication between the framework and the handler for trigger channels.
 * Although trigger channels by their nature do not have a state, it becomes possible to link such trigger channels to
 * items using such a profile.
 * The main purpose of a {@link TriggerProfile} is to listen to triggered events and use them to calculate a meaningful
 * state.
public interface TriggerProfile extends Profile {
     * Will be called whenever the binding intends to issue a trigger event.
     * @param event the event payload
    void onTriggerFromHandler(String event);
