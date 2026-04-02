import org.openhab.core.events.system.StartlevelEvent;
 * This is a ModuleHandler implementation for Triggers which trigger the rule if a certain system event occurs.
public class SystemTriggerHandler extends BaseTriggerModuleHandler implements EventSubscriber {
    public static final String STARTLEVEL_MODULE_TYPE_ID = "core.SystemStartlevelTrigger";
    public static final String CFG_STARTLEVEL = "startlevel";
    public static final String OUT_STARTLEVEL = "startlevel";
    private final Logger logger = LoggerFactory.getLogger(SystemTriggerHandler.class);
    private final Integer startlevel;
    private boolean triggered = false;
    public SystemTriggerHandler(Trigger module, BundleContext bundleContext) {
        this.startlevel = ((BigDecimal) module.getConfiguration().get(CFG_STARTLEVEL)).intValue();
        if (STARTLEVEL_MODULE_TYPE_ID.equals(module.getTypeUID())) {
            this.types = Set.of(StartlevelEvent.TYPE);
            logger.warn("Module type '{}' is not (yet) handled by this class.", module.getTypeUID());
            throw new IllegalArgumentException(module.getTypeUID() + " is no valid module type.");
        if (triggered) {
            // this trigger only works once
        logger.trace("Received Event: Source: {} Topic: {} Type: {}  Payload: {}", event.getSource(), event.getTopic(),
                event.getType(), event.getPayload());
        if (event instanceof StartlevelEvent startlevelEvent && STARTLEVEL_MODULE_TYPE_ID.equals(module.getTypeUID())) {
            Integer sl = startlevelEvent.getStartlevel();
            if (startlevel <= sl && startlevel > StartLevelService.STARTLEVEL_RULEENGINE) {
                // only execute rules if their start level is higher than the rule engine activation level, since
                // otherwise the rule engine takes care of the execution already
                trigger(event);
    private void trigger(Event event) {
        final ModuleHandlerCallback callback = this.callback;
        if (!(callback instanceof TriggerHandlerCallback)) {
        TriggerHandlerCallback thCallback = (TriggerHandlerCallback) callback;
        Map<String, Object> values = Map.of(OUT_STARTLEVEL, startlevel, "event", event);
        thCallback.triggered(module, values);
        triggered = true;
