import org.openhab.core.events.EventFilter;
import org.openhab.core.thing.events.ChannelTriggeredEvent;
 * This is a ModuleHandler implementation for trigger channels with specific events
public class ChannelEventTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber, EventFilter {
    public static final String MODULE_TYPE_ID = "core.ChannelEventTrigger";
    public static final String CFG_CHANNEL_EVENT = "event";
    public static final String CFG_CHANNEL = "channelUID";
    public static final String TOPIC = "openhab/channels/*/triggered";
    private final Logger logger = LoggerFactory.getLogger(ChannelEventTriggerHandler.class);
    private @Nullable final String eventOnChannel;
    private final ChannelUID channelUID;
    private final Set<String> types;
    private final ServiceRegistration<?> eventSubscriberRegistration;
    public ChannelEventTriggerHandler(Trigger module, BundleContext bundleContext) {
        this.eventOnChannel = (String) module.getConfiguration().get(CFG_CHANNEL_EVENT);
        this.channelUID = new ChannelUID((String) module.getConfiguration().get(CFG_CHANNEL));
        this.types = Set.of("ChannelTriggeredEvent");
        eventSubscriberRegistration = this.bundleContext.registerService(EventSubscriber.class.getName(), this, null);
        ModuleHandlerCallback localCallback = callback;
        if (localCallback != null) {
            logger.trace("Received Event: Source: {} Topic: {} Type: {}  Payload: {}", event.getSource(),
                    event.getTopic(), event.getType(), event.getPayload());
            ((TriggerHandlerCallback) localCallback).triggered(module, Map.of("event", event));
    public boolean apply(Event event) {
        boolean eventMatches = false;
        if (event instanceof ChannelTriggeredEvent cte) {
            if (channelUID.equals(cte.getChannel())) {
                String eventOnChannel = this.eventOnChannel;
                logger.trace("->FILTER: {}:{}", cte.getEvent(), eventOnChannel);
                eventMatches = eventOnChannel == null || eventOnChannel.isBlank()
                        || eventOnChannel.equals(cte.getEvent());
        return eventMatches;
    public @Nullable EventFilter getEventFilter() {
        eventSubscriberRegistration.unregister();
