 * An {@link ExecutionEvent} is only used to notify rules when a script or the REST API trigger the run.
public class ExecutionEvent extends AbstractEvent {
    public static final String TYPE = ExecutionEvent.class.getSimpleName();
     * Constructs a new rule execution event
    public ExecutionEvent(String topic, String payload, String source) {
        return "Execution triggered by " + getSource();
