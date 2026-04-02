 * The {@link ConfigDescriptionParameterValidator} for the options of a {@link ConfigDescriptionParameter}.
 * @author Jan N. Klug - Extend for decimal types
final class OptionsValidator implements ConfigDescriptionParameterValidator {
    public @Nullable ConfigValidationMessage validate(ConfigDescriptionParameter param, @Nullable Object value) {
        if (value == null || !param.getLimitToOptions() || param.getOptions().isEmpty()) {
        boolean invalid;
        if (param.getType() == ConfigDescriptionParameter.Type.DECIMAL
                || param.getType() == ConfigDescriptionParameter.Type.INTEGER) {
            BigDecimal bdValue = new BigDecimal(value.toString());
            invalid = param.getOptions().stream().map(o -> new BigDecimal(o.getValue()))
                    .noneMatch(v -> v.compareTo(bdValue) == 0);
            invalid = param.getOptions().stream().map(ParameterOption::getValue)
                    .noneMatch(v -> v.equals(value.toString()));
        if (invalid) {
            MessageKey messageKey = MessageKey.OPTIONS_VIOLATED;
            return new ConfigValidationMessage(param.getName(), messageKey.defaultMessage, messageKey.key,
                    String.valueOf(value), param.getOptions());
