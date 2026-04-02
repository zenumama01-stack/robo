 * The {@link ConfigDescriptionParameterValidator} for the pattern attribute of a
final class PatternValidator implements ConfigDescriptionParameterValidator {
        String pattern = parameter.getPattern();
        if (value == null || parameter.getType() != Type.TEXT || pattern == null) {
        if (!((String) value).matches(pattern)) {
            MessageKey messageKey = MessageKey.PATTERN_VIOLATED;
            return new ConfigValidationMessage(parameter.getName(), messageKey.defaultMessage, messageKey.key,
                    String.valueOf(value), parameter.getPattern());
