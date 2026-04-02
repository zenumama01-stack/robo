 * The {@link ConfigDescriptionParameterValidatorFactory} creates the corresponding
 * {@link ConfigDescriptionParameterValidator}s used by ConfigDescriptionValidator.
public final class ConfigDescriptionParameterValidatorFactory {
    private ConfigDescriptionParameterValidatorFactory() {
     * Returns a new validator for the required attribute of a config description parameter.
     * @return a new validator for the required attribute of a config description parameter
    public static ConfigDescriptionParameterValidator createRequiredValidator() {
        return new RequiredValidator();
     * Returns a new validator for the data type validation of a config description parameter.
     * @return a new validator for the data type validation of a config description parameter
    public static ConfigDescriptionParameterValidator createTypeValidator() {
        return new TypeValidator();
     * Returns a new validator for the min and max attribute of a config description parameter.
     * @return a new validator for the min and max attribute of a config description parameter
    public static ConfigDescriptionParameterValidator createMinMaxValidator() {
        return new MinMaxValidator();
     * Returns a new validator for the pattern attribute of a config description parameter.
     * @return a new validator for the pattern attribute of a config description parameter
    public static ConfigDescriptionParameterValidator createPatternValidator() {
        return new PatternValidator();
     * Returns a new validator for the parameter options of a config description parameter.
     * @return a new validator for the parameter options of a config description parameter
    public static ConfigDescriptionParameterValidator createOptionsValidator() {
        return new OptionsValidator();
