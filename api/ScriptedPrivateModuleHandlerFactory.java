 * The {@link ScriptedPrivateModuleHandlerFactory} is used to provide types for "private" scripted Actions, Triggers and
 * Conditions. These Module Types are meant to be only used inside scripts.
@Component(immediate = true, service = { ScriptedPrivateModuleHandlerFactory.class, ModuleHandlerFactory.class })
public class ScriptedPrivateModuleHandlerFactory extends AbstractScriptedModuleHandlerFactory {
    private static final String PRIV_ID = "privId";
    private static final Collection<String> TYPES = Arrays.asList("jsr223.ScriptedAction", "jsr223.ScriptedCondition",
            "jsr223.ScriptedTrigger");
    private final Logger logger = LoggerFactory.getLogger(ScriptedPrivateModuleHandlerFactory.class);
    private final HashMap<String, ScriptedHandler> privateTypes = new HashMap<>();
    private int nextId = 0;
        ScriptedHandler scriptedHandler = null;
            scriptedHandler = privateTypes.get(module.getConfiguration().get(PRIV_ID));
            logger.warn("ScriptedHandler {} for ruleUID {} not found", module.getConfiguration().get(PRIV_ID), ruleUID);
            moduleHandler = getModuleHandler(module, scriptedHandler);
    public String addHandler(String privId, ScriptedHandler scriptedHandler) {
        privateTypes.put(privId, scriptedHandler);
        return privId;
    public String addHandler(ScriptedHandler scriptedHandler) {
        String privId = "i" + (nextId++);
    public void removeHandler(String privId) {
        privateTypes.remove(privId);
