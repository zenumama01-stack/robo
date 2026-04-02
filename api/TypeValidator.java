 * The {@link TypeValidator} validates if the given value can be assigned to the config description parameter according
 * to its type definition.
final class TypeValidator implements ConfigDescriptionParameterValidator {
        if (!typeIntrospection.isAssignable(value)) {
            return new ConfigValidationMessage(parameter.getName(), MessageKey.DATA_TYPE_VIOLATED.defaultMessage,
                    MessageKey.DATA_TYPE_VIOLATED.key, value.getClass(), parameter.getType());
