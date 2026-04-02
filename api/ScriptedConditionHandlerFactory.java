import org.openhab.core.automation.handler.ConditionHandler;
public interface ScriptedConditionHandlerFactory extends ScriptedHandler {
    ConditionHandler get(Condition module);
