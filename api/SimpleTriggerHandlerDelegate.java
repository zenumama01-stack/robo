import org.openhab.core.automation.ModuleHandlerCallback;
import org.openhab.core.automation.handler.BaseTriggerModuleHandler;
 * The {@link SimpleTriggerHandlerDelegate} allows to define triggers in a script language in different ways.
public class SimpleTriggerHandlerDelegate extends BaseTriggerModuleHandler {
    private final org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleTriggerHandler triggerHandler;
    public SimpleTriggerHandlerDelegate(Trigger module,
            org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleTriggerHandler triggerHandler) {
        this.triggerHandler = triggerHandler;
    public void setCallback(ModuleHandlerCallback callback) {
        triggerHandler.setRuleEngineCallback(this.module,
                new SimpleTriggerHandlerCallbackDelegate(this.module, (TriggerHandlerCallback) callback));
