@Component(immediate = true, service = { ModelParser.class, RuleRuntimeActivator.class })
public class RuleRuntimeActivator implements ModelParser {
    private final Logger logger = LoggerFactory.getLogger(RuleRuntimeActivator.class);
    public RuleRuntimeActivator(final @Reference ScriptEngine scriptEngine,
            final @Reference ScriptServiceUtil scriptServiceUtil) {
    public void activate(BundleContext bc) throws Exception {
        RulesStandaloneSetup.doSetup(scriptServiceUtil, scriptEngine);
        logger.debug("Registered 'rule' configuration parser");
        RulesStandaloneSetup.unregister();
        return "rules";
