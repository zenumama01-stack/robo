 * The {@link ConfigDescriptionParameterValidator} for the required attribute of a {@link ConfigDescriptionParameter}.
final class RequiredValidator implements ConfigDescriptionParameterValidator {
        if (param.isRequired() && value == null) {
            MessageKey messageKey = MessageKey.PARAMETER_REQUIRED;
            return new ConfigValidationMessage(param.getName(), messageKey.defaultMessage, messageKey.key);
