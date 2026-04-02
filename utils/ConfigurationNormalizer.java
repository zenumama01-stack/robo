import org.openhab.core.automation.internal.RuleImpl;
 * This class provides utility methods used by {@link RuleRegistry} to resolve and normalize the {@link RuleImpl}s
 * configuration values.
public class ConfigurationNormalizer {
     * Normalizes the configurations of the provided {@link ModuleImpl}s.
     * @param modules a list of {@link ModuleImpl}s to normalize.
     * @param mtRegistry the {@link ModuleTypeRegistry} that provides the meta-data needed for the normalization.
     * @see ConfigurationNormalizer#normalizeConfiguration(Configuration, Map)
    public static <T extends Module> void normalizeModuleConfigurations(List<T> modules,
            ModuleTypeRegistry mtRegistry) {
            ModuleType mt = mtRegistry.get(module.getTypeUID());
                Map<String, ConfigDescriptionParameter> mapConfigDescriptions = getConfigDescriptionMap(
                        mt.getConfigurationDescriptions());
                normalizeConfiguration(module.getConfiguration(), mapConfigDescriptions);
     * Converts a list of {@link ConfigDescriptionParameter}s to a map with the parameter's names as keys.
     * @param configDesc the list to convert.
     * @return a map that maps parameter names to {@link ConfigDescriptionParameter} instances.
    public static Map<String, ConfigDescriptionParameter> getConfigDescriptionMap(
            List<ConfigDescriptionParameter> configDesc) {
        Map<String, ConfigDescriptionParameter> mapConfigDescs = new HashMap<>();
        for (ConfigDescriptionParameter configDescriptionParameter : configDesc) {
            mapConfigDescs.put(configDescriptionParameter.getName(), configDescriptionParameter);
        return mapConfigDescs;
     * Normalizes the types of the configuration's parameters to the allowed ones. References are ignored. Null values
     * are replaced with the defaults and then normalized.
     * @param configuration the configuration to normalize.
     * @param configDescriptionMap the meta-data of the configuration.
    public static void normalizeConfiguration(Configuration configuration,
            Map<String, ConfigDescriptionParameter> configDescriptionMap) {
        for (Entry<String, ConfigDescriptionParameter> entry : configDescriptionMap.entrySet()) {
            ConfigDescriptionParameter parameter = entry.getValue();
            if (parameter != null) {
                String parameterName = entry.getKey();
                final Object value = configuration.get(parameterName);
                if (value instanceof String string && string.contains("${")) {
                    continue; // It is a reference
                    final Object defaultValue = parameter.getDefault();
                    if (defaultValue == null) {
                        configuration.remove(parameterName);
                        configuration.put(parameterName, ConfigUtil.normalizeType(defaultValue, parameter));
                    configuration.put(parameterName, ConfigUtil.normalizeType(value, parameter));
