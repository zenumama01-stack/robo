import org.openhab.core.events.AbstractEventFactory;
import org.openhab.core.events.EventFactory;
 * This is an {@link EventFactory} for creating add-on events. The following event types are supported by this
 * factory:
 * {@link AddonEvent#TYPE}
@Component(service = EventFactory.class, immediate = true)
public class AddonEventFactory extends AbstractEventFactory {
    static final String TOPIC_PREFIX = "openhab/addons/{id}";
    static final String ADDON_INSTALLED_EVENT_TOPIC_POSTFIX = "/installed";
    static final String ADDON_UNINSTALLED_EVENT_TOPIC_POSTFIX = "/uninstalled";
    static final String ADDON_FAILURE_EVENT_TOPIC_POSTFIX = "/failed";
    static final String ADDON_INSTALLED_EVENT_TOPIC = TOPIC_PREFIX + ADDON_INSTALLED_EVENT_TOPIC_POSTFIX;
    static final String ADDON_UNINSTALLED_EVENT_TOPIC = TOPIC_PREFIX + ADDON_UNINSTALLED_EVENT_TOPIC_POSTFIX;
    static final String ADDON_FAILURE_EVENT_TOPIC = TOPIC_PREFIX + ADDON_FAILURE_EVENT_TOPIC_POSTFIX;
     * Constructs a new AddonEventFactory.
    public AddonEventFactory() {
        super(Set.of(AddonEvent.TYPE));
    protected Event createEventByType(String eventType, String topic, String payload, @Nullable String source)
            throws Exception {
        if (topic.endsWith(ADDON_FAILURE_EVENT_TOPIC_POSTFIX)) {
            String[] properties = deserializePayload(payload, String[].class);
            return new AddonEvent(topic, payload, properties[0], properties[1]);
            String id = deserializePayload(payload, String.class);
            return new AddonEvent(topic, payload, id);
     * Creates an "add-on installed" event.
     * @param id the id of the installed add-on
     * @return the according event
    public static AddonEvent createAddonInstalledEvent(String id) {
        String topic = buildTopic(ADDON_INSTALLED_EVENT_TOPIC, id);
        String payload = serializePayload(id);
     * Creates an "add-on uninstalled" event.
     * @param id the id of the uninstalled add-on
    public static AddonEvent createAddonUninstalledEvent(String id) {
        String topic = buildTopic(ADDON_UNINSTALLED_EVENT_TOPIC, id);
     * Creates an "add-on failure" event.
     * @param id the id of the add-on that caused a failure
     * @param msg the message text of the failure
    public static AddonEvent createAddonFailureEvent(String id, @Nullable String msg) {
        String topic = buildTopic(ADDON_FAILURE_EVENT_TOPIC, id);
        String[] properties = new String[] { id, msg };
        String payload = serializePayload(properties);
        return new AddonEvent(topic, payload, id, msg);
    static String buildTopic(String topic, String id) {
        return topic.replace("{id}", id);
