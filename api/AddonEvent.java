import org.openhab.core.events.AbstractEvent;
 * This is an {@link org.openhab.core.events.Event} that is sent on add-on operations, such as installing and
 * uninstalling.
public class AddonEvent extends AbstractEvent {
     * The add-on event type.
    public static final String TYPE = AddonEvent.class.getSimpleName();
    private @Nullable String msg;
     * Constructs a new add-on event object.
     * @param payload the payload
     * @param id the id of the add-on
     * @param msg the message text
    public AddonEvent(String topic, String payload, String id, @Nullable String msg) {
        super(topic, payload, null);
        this.msg = msg;
    public AddonEvent(String topic, String payload, String id) {
        this(topic, payload, id, null);
        return TYPE;
        if (getTopic().equals(AddonEventFactory.buildTopic(AddonEventFactory.ADDON_INSTALLED_EVENT_TOPIC, id))) {
            return "Add-on '" + id + "' has been installed.";
        } else if (getTopic()
                .equals(AddonEventFactory.buildTopic(AddonEventFactory.ADDON_UNINSTALLED_EVENT_TOPIC, id))) {
            return "Add-on '" + id + "' has been uninstalled.";
            return id + ": " + (msg != null ? msg : "Operation failed.");
