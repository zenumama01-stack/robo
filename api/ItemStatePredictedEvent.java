 * This event announces potential item state outcomes when a command was received.
 * Thereby it denotes that the item state is most likely going to change to the given predicted value.
 * If {@code isConfirmation == true}, then it basically only confirms the previous item state because a received command
 * will not be successfully executed and therefore presumably will not result in a state change (e.g. because no handler
 * currently is capable of delivering such an event to its device).
public class ItemStatePredictedEvent extends ItemEvent {
     * The item state predicted event type.
    public static final String TYPE = ItemStatePredictedEvent.class.getSimpleName();
    protected final State predictedState;
    protected final boolean isConfirmation;
     * Constructs a new item state predicted event.
     * @param predictedState the predicted item state
     * @param isConfirmation the confirmation of previous item state
    public ItemStatePredictedEvent(String topic, String payload, String itemName, State predictedState,
        super(topic, payload, itemName, null);
        this.predictedState = predictedState;
     * Gets the predicted item state.
     * @return the predicted item state
    public State getPredictedState() {
        return predictedState;
     * Gets the confirmation of previous item state.
     * @return true, if previous item state is confirmed
        return String.format("Item '%s' predicted to become %s", itemName, predictedState);
