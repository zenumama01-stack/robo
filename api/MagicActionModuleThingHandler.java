package org.openhab.core.magic.binding.handler;
import org.openhab.core.magic.binding.internal.automation.modules.MagicThingActionsService;
import org.openhab.core.thing.binding.BaseThingHandler;
 * ThingHandler which provides annotated actions that will become Action modules for the automation engine
public class MagicActionModuleThingHandler extends BaseThingHandler {
    private final Logger logger = LoggerFactory.getLogger(MagicActionModuleThingHandler.class);
    public MagicActionModuleThingHandler(Thing thing) {
        super(thing);
        updateStatus(ThingStatus.ONLINE);
        // doing nothing here
    public void communicateActionToDevice(String doSomething) {
        String text = (String) thing.getConfiguration().get("textParam");
        logger.debug("Handler with textParam={} pushes action {}  to device.", text, doSomething);
    public Collection<Class<? extends ThingHandlerService>> getServices() {
        return List.of(MagicThingActionsService.class);
