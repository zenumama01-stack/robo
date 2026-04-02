import org.openhab.core.automation.module.script.action.ScriptExecution;
 * This is a scope provider for script actions that were available in openHAB 1 DSL rules
public class ScriptActionScriptScopeProvider implements ScriptExtensionProvider {
    private static final String PRESET_ACTIONS = "ScriptAction";
    private final Map<String, Object> elements;
    public ScriptActionScriptScopeProvider(final @Reference BusEvent busEvent,
            final @Reference ScriptExecution scriptExecution) {
        elements = Map.of("busEvent", busEvent, "scriptExecution", scriptExecution);
        return Set.of(PRESET_ACTIONS);
        if (PRESET_ACTIONS.equals(preset)) {
