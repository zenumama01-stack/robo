 * An {@link TimerEvent} is only used to notify rules when timer triggers fire.
public class TimerEvent extends AbstractEvent {
    public static final String TYPE = TimerEvent.class.getSimpleName();
     * Constructs a new timer event
     * @param payload the payload of the event (contains trigger configuration)
    public TimerEvent(String topic, String payload, @Nullable String source) {
        return "Timer " + getSource() + " triggered.";
