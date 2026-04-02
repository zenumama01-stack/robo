 * if an event occurs. The eventType, eventSource and topic can be set with the
 * configuration. It is a generic approach which makes it easier to specify
 * more concrete event based triggers with the composite module approach of the
 * automation component. Each GenericTriggerHandler instance registers as
 * EventSubscriber, so the dispose method must be called for unregistering the
 * service.
 * @author Cody Cutrer - refactored to match configuration and semantics of GenericConditionTriggerHandler
public class GenericEventTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber, EventFilter {
    public static final String MODULE_TYPE_ID = "core.GenericEventTrigger";
    private final Logger logger = LoggerFactory.getLogger(GenericEventTriggerHandler.class);
    public GenericEventTriggerHandler(Trigger module, BundleContext bundleContext) {
            types = Set.of(((String) module.getConfiguration().get(CFG_TYPES)).split(","));
            types = Set.of();
        logger.trace("Registered EventSubscriber: Topic: {} Type: {} Source: {} Payload: {}", topic, types, source,
                payload);
            logger.trace("Received Event: Topic: {} Type: {} Source: {} Payload: {}", event.getTopic(), event.getType(),
                    event.getSource(), event.getPayload());
            Map<String, Object> values = new HashMap<>();
            values.put("event", event);
            ((TriggerHandlerCallback) callback).triggered(this.module, values);
     * do the cleanup: unregistering eventSubscriber...
            eventSubscriberRegistration = null;
        logger.trace("->FILTER: {}: {}", event.getTopic(), source);
        if (localTopicFilter != null && !topicFilter.apply(event)) {
