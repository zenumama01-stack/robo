package org.openhab.core.automation.module.script.internal.provider;
import java.util.TreeMap;
import org.openhab.core.automation.module.script.internal.handler.AbstractScriptModuleHandler;
import org.openhab.core.automation.type.Output;
 * This class dynamically provides ScriptActionType and ScriptConditionType {@link ModuleType}s. This class is necessary
 * because there is no other way to provide dynamic {@link ParameterOption}s for {@link ModuleType}s.
public class ScriptModuleTypeProvider extends AbstractProvider<ModuleType> implements ModuleTypeProvider {
    private final Logger logger = LoggerFactory.getLogger(ScriptModuleTypeProvider.class);
    private final Map<String, String> parameterOptions = new TreeMap<>();
        listeners.clear();
        parameterOptions.clear();
        if (parameterOptions.isEmpty()) {
        } else if (ScriptActionHandler.TYPE_ID.equals(uid)) {
            return getScriptActionType(locale);
        } else if (ScriptConditionHandler.TYPE_ID.equals(uid)) {
            return getScriptConditionType(locale);
    private ModuleType getScriptActionType(@Nullable Locale locale) {
        List<Output> outputs = new ArrayList<>();
        Output result = new Output("result", "java.lang.Object", "result", "the script result", null, null, null);
        outputs.add(result);
        return new ActionType(ScriptActionHandler.TYPE_ID, getConfigDescriptions(locale), "Execute an inline script",
                "Executes a user-defined script", null, Visibility.VISIBLE, null, outputs);
    private ModuleType getScriptConditionType(@Nullable Locale locale) {
        return new ConditionType(ScriptConditionHandler.TYPE_ID, getConfigDescriptions(locale),
                "An inline script evaluates to true", "Allows the definition of a condition through a script", null,
                Visibility.VISIBLE, null);
    private List<ModuleType> getModuleTypesUnconditionally(@Nullable Locale locale) {
        return List.of(getScriptActionType(locale), getScriptConditionType(locale));
     * This method creates the {@link ConfigDescriptionParameter}s used by the generated ScriptActionType and
     * ScriptConditionType. {@link AbstractScriptModuleHandler} requires that the names of these be 'type' and 'script'.
     * @return a list of {#link ConfigurationDescriptionParameter}s
    private List<ConfigDescriptionParameter> getConfigDescriptions(@Nullable Locale locale) {
        List<ParameterOption> parameterOptionsList = new ArrayList<>();
        for (Map.Entry<String, String> entry : parameterOptions.entrySet()) {
            parameterOptionsList.add(new ParameterOption(entry.getKey(), entry.getValue()));
        final ConfigDescriptionParameter scriptType = ConfigDescriptionParameterBuilder.create("type", Type.TEXT)
                .withRequired(true).withMultiple(false).withLabel("Script Type")
                .withDescription("The scripting language used").withOptions(parameterOptionsList)
        final ConfigDescriptionParameter script = ConfigDescriptionParameterBuilder.create("script", Type.TEXT)
                .withRequired(true).withReadOnly(false).withMultiple(false).withLabel("Script").withContext("script")
                .withDescription("The script to execute").build();
        return List.of(scriptType, script);
        return parameterOptions.isEmpty() ? List.of() : getModuleTypesUnconditionally(locale);
    private void notifyModuleTypesAdded() {
        for (ModuleType moduleType : getModuleTypesUnconditionally(null)) {
            notifyListenersAboutAddedElement(moduleType);
    private void notifyModuleTypesRemoved() {
            notifyListenersAboutRemovedElement(moduleType);
     * As {@link ScriptEngineFactory}s are added/removed, this method will create the {@link ParameterOption}s
     * that are available when selecting a script type in a ScriptActionType or ScriptConditionType.
        Map.Entry<String, String> parameterOption = ScriptEngineFactoryHelper.getParameterOption(engineFactory);
        if (parameterOption != null) {
            boolean notifyListeners = parameterOptions.isEmpty();
            parameterOptions.put(parameterOption.getKey(), parameterOption.getValue());
            logger.trace("ParameterOptions: {}", parameterOptions);
            if (notifyListeners) {
                notifyModuleTypesAdded();
                parameterOptions.remove(ScriptEngineFactoryHelper.getPreferredMimeType(engineFactory));
                    notifyModuleTypesRemoved();
                logger.trace("unsetScriptEngineFactory: engine was null");
            logger.trace("unsetScriptEngineFactory: scriptTypes was empty");
