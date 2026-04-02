 * {@link AnnotatedActions} with multiple action modules that can have different instances/configurations
 * The {@link MagicMultiActionMarker} holds the configuration URI and shows the system that THIS service can be
 * instantiated multiple times
@Component(immediate = true, configurationPolicy = ConfigurationPolicy.REQUIRE, configurationPid = "org.openhab.magicmultiaction")
@ActionScope(name = "binding.magic")
public class MagicMultiServiceMultiActions implements AnnotatedActions {
    private final Logger logger = LoggerFactory.getLogger(MagicMultiServiceMultiActions.class);
    protected Map<String, Object> config = Map.of();
    @RuleAction(label = "@text/module.binding.magic.testMethod.label", description = "Just a text that prints out inputs and config parameters")
    public @ActionOutput(name = "output1", type = "java.lang.Integer") @ActionOutput(name = "output2", type = "java.lang.String") Map<String, Object> testMethod(
            @ActionInput(name = "input1") String input1, @ActionInput(name = "input2") String input2) {
        boolean boolParam = (Boolean) config.get("boolParam");
        String textParam = (String) config.get("textParam");
                "Executed multi action testMethod with inputs: {}, {} and configParams: boolParam={}, textParam={}",
                input1, input2, boolParam, textParam);
    @RuleAction(label = "Magic Multi Action boolean", description = "Action method that returns a plain boolean")
    public @ActionOutput(name = "out1", type = "java.lang.Boolean") boolean booleanMethod(
            @ActionInput(name = "in1") String input1, @ActionInput(name = "in2") String input2) {
        result.put("output1", 42);
        result.put("output2", "foobar");
        logger.debug("executed boolean method with: {}, {}", input1, input2);
                "Executed multi action booleanMethod with inputs: {}, {} and configParams: boolParam={}, textParam={}",
    @RuleAction(label = "Magic Multi Action void", description = "Action method with type void, so no outputs")
    public void voidMethod(@ActionInput(name = "inv1") String input1, @ActionInput(name = "inv2") String input2) {
                "Executed multi action voidMethod with inputs: {}, {} and configParams: boolParam={}, textParam={}",
