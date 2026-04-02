import org.openhab.core.thing.events.ThingStatusInfoChangedEvent;
 * This is a ModuleHandler implementation for Triggers which trigger the rule if a thing status event occurs. The
 * eventType and status value can be set in the configuration.
public class ThingStatusTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    public static final String UPDATE_MODULE_TYPE_ID = "core.ThingStatusUpdateTrigger";
    public static final String CHANGE_MODULE_TYPE_ID = "core.ThingStatusChangeTrigger";
    public static final String CFG_PREVIOUS_STATUS = "previousStatus";
    public static final String OUT_STATUS = "status";
    public static final String OUT_NEW_STATUS = "newStatus";
    public static final String OUT_OLD_STATUS = "oldStatus";
    public static final String OUT_EVENT = "event";
    private final Logger logger = LoggerFactory.getLogger(ThingStatusTriggerHandler.class);
    private @Nullable final String status;
    private @Nullable final String previousStatus;
    private final TopicEventFilter eventTopicFilter;
    public ThingStatusTriggerHandler(Trigger module, BundleContext bundleContext) {
        String thingUID = (String) module.getConfiguration().get(CFG_THING_UID);
        this.status = (String) module.getConfiguration().get(CFG_STATUS);
        this.previousStatus = (String) module.getConfiguration().get(CFG_PREVIOUS_STATUS);
            this.types = Set.of(ThingStatusInfoEvent.TYPE);
            this.types = Set.of(ThingStatusInfoChangedEvent.TYPE);
        this.eventTopicFilter = new TopicEventFilter(
                "^openhab/things/" + thingUID.replace("?", ".?").replace("*", ".*?") + "/.*$");
        return eventTopicFilter;
        if (!(callback instanceof TriggerHandlerCallback thCallback)) {
        if (event instanceof ThingStatusInfoEvent infoEvent && UPDATE_MODULE_TYPE_ID.equals(module.getTypeUID())) {
            ThingStatus status = infoEvent.getStatusInfo().getStatus();
            if (statusMatches(this.status, status)) {
                values.put(OUT_STATUS, status);
        } else if (event instanceof ThingStatusInfoChangedEvent changedEvent
            ThingStatus newStatus = changedEvent.getStatusInfo().getStatus();
            ThingStatus oldStatus = changedEvent.getOldStatusInfo().getStatus();
            if (statusMatches(this.status, newStatus) && statusMatches(this.previousStatus, oldStatus)) {
                values.put(OUT_NEW_STATUS, newStatus);
                values.put(OUT_OLD_STATUS, oldStatus);
            values.put(OUT_EVENT, event);
    private boolean statusMatches(@Nullable String requiredStatus, ThingStatus status) {
        if (requiredStatus == null) {
        String reqStatus = requiredStatus.trim();
        return reqStatus.isEmpty() || reqStatus.equals(status.toString());
