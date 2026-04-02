 * {@link AnnotatedActions} for one action module with a configuration
@Component(configurationPid = "org.openhab.magicsingleaction", //
        property = Constants.SERVICE_PID + "=org.openhab.automation.action.magicSingleActionService")
@ConfigurableService(category = "RuleActions", label = "Magic Single Action Service", description_uri = "automationAction:magicSingleAction")
@ActionScope(name = "binding.magicService")
public class MagicSingleActionService implements AnnotatedActions {
    private final Logger logger = LoggerFactory.getLogger(MagicSingleActionService.class);
    @RuleAction(label = "Magic Single Service", description = "Just a simple Magic Single Service Action")
    public @ActionOutput(name = "output1", type = "java.lang.Integer") @ActionOutput(name = "output2", type = "java.lang.String") Map<String, Object> singleServiceAction(
            @ActionInput(name = "input1") String input1, @ActionInput(name = "input2") String input2, String input3,
            @ActionInput(name = "someNameForInput4") String input4) {
        // do some calculation stuff here and place the outputs into the result map
        result.put("output2", "myOutput2 String");
        String configParam = (String) config.get("confParam1");
                "Magic Magic Single Service method: executed map method with inputs: {}, {}, {}, {} and configuration parameter: {}",
                input1, input2, input3, input4, configParam);
