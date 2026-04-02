package org.openhab.core.config.core.internal.validation;
import org.openhab.core.config.core.validation.ConfigValidationMessage;
 * The {@link ConfigDescriptionParameterValidator} can be implemented to provide a specific validation of a
 * {@link ConfigDescriptionParameter} and its value to be set.
 * @author Thomas Höfer - Initial contribution
public interface ConfigDescriptionParameterValidator {
     * Validates the given value against the given {@link ConfigDescriptionParameter}.
     * @param parameter the configuration description parameter
     * @param value the value to be set for the config description parameter
     * @return a {@link ConfigValidationMessage} if value does not meet the declaration of the parameter,
     *         otherwise null
    ConfigValidationMessage validate(ConfigDescriptionParameter parameter, @Nullable Object value);
