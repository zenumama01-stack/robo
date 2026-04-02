import org.openhab.core.config.core.internal.validation.TypeIntrospections.TypeIntrospection;
 * The {@link ConfigDescriptionParameterValidator} for the minimum and maximum attribute of a
 * @author Chris Jackson - Allow options to be outside of min/max value
final class MinMaxValidator implements ConfigDescriptionParameterValidator {
    public @Nullable ConfigValidationMessage validate(ConfigDescriptionParameter parameter, @Nullable Object value) {
        if (value == null || parameter.getType() == Type.BOOLEAN) {
        String normalizedValueString = ConfigUtil.normalizeType(value, parameter).toString();
        // Allow specified options to be outside the min/max value
        // Option values are a string, so we can do a simple compare
        if (parameter.getOptions().stream().map(ParameterOption::getValue)
                .anyMatch(v -> v.equals(normalizedValueString))) {
        TypeIntrospection typeIntrospection = TypeIntrospections.get(parameter.getType());
        BigDecimal min = parameter.getMinimum();
        if (min != null && typeIntrospection.isMinViolated(value, min)) {
            return createMinMaxViolationMessage(parameter.getName(), typeIntrospection.getMinViolationMessageKey(),
                    min);
        BigDecimal max = parameter.getMaximum();
        if (max != null && typeIntrospection.isMaxViolated(value, max)) {
            return createMinMaxViolationMessage(parameter.getName(), typeIntrospection.getMaxViolationMessageKey(),
                    max);
    private static ConfigValidationMessage createMinMaxViolationMessage(String parameterName, MessageKey messageKey,
            BigDecimal minMax) {
        return new ConfigValidationMessage(parameterName, messageKey.defaultMessage, messageKey.key,
                String.valueOf(minMax));
