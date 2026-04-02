 * Some automation actions to be used with a {@link MagicActionModuleThingHandler}
@ActionScope(name = "magic")
public class MagicThingActionsService implements ThingActions {
    private final Logger logger = LoggerFactory.getLogger(MagicThingActionsService.class);
    private @Nullable MagicActionModuleThingHandler handler;
    @RuleAction(label = "Magic thingHandlerAction", description = "Action that calls some logic in a thing handler")
    public @ActionOutput(name = "output1", type = "java.lang.String") @ActionOutput(name = "output2", type = "java.lang.String") Map<String, Object> thingHandlerAction(
        logger.debug("thingHandlerAction called with inputs: {} {}", input1, input2);
        // one can pass any data to the handler of the selected thing, here we are passing the first input parameter
        // passed into this module via the automation engine
        handler.communicateActionToDevice(input1);
        // hint: one could also put handler results into the output map for further processing within the automation
        // engine
        result.put("output2", "myThing");
        if (handler instanceof MagicActionModuleThingHandler thingHandler) {
            this.handler = thingHandler;
        return this.handler;
