import org.openhab.core.automation.handler.TriggerHandler;
public interface ScriptedTriggerHandlerFactory extends ScriptedHandler {
    TriggerHandler get(Trigger module);
