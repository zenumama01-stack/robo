package org.openhab.core.automation.module.script.rulesupport.internal.delegates;
 * The SimpleActionHandlerDelegate allows the registration of {@link SimpleActionHandler}s to the RuleManager.
public class SimpleActionHandlerDelegate extends BaseActionModuleHandler {
    private org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleActionHandler actionHandler;
    public SimpleActionHandlerDelegate(Action module,
            org.openhab.core.automation.module.script.rulesupport.shared.simple.SimpleActionHandler actionHandler) {
        this.actionHandler = actionHandler;
    public void dispose() {
    public @Nullable Map<String, @Nullable Object> execute(Map<String, @Nullable Object> inputs) {
        Set<String> keys = new HashSet<>(inputs.keySet());
        Map<String, @Nullable Object> extendedInputs = new HashMap<>(inputs);
        for (String key : keys) {
            Object value = extendedInputs.get(key);
            int dotIndex = key.indexOf('.');
            if (dotIndex != -1) {
                String moduleName = key.substring(0, dotIndex);
                extendedInputs.put("module", moduleName);
                String newKey = key.substring(dotIndex + 1);
                extendedInputs.put(newKey, value);
        Object result = actionHandler.execute(module, extendedInputs);
        Map<String, @Nullable Object> resultMap = new HashMap<>();
        resultMap.put("result", result);
