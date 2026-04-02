 * The {@link ConfigDescriptionParameterBuilder} class provides a builder for the {@link ConfigDescriptionParameter}
 * class.
 * @author Chris Jackson - Initial contribution
public class ConfigDescriptionParameterBuilder {
    private Type type;
    private String groupName;
    private BigDecimal min;
    private BigDecimal max;
    private BigDecimal step;
    private String pattern;
    private Boolean required;
    private Boolean readOnly;
    private Boolean multiple;
    private Integer multipleLimit;
    private String unit;
    private String unitLabel;
    private String context;
    private Boolean limitToOptions;
    private Boolean advanced;
    private Boolean verify;
    private ConfigDescriptionParameterBuilder(String name, Type type) {
     * Creates a parameter builder
     * @param name configuration parameter name
     * @param type configuration parameter type
     * @return parameter builder
    public static ConfigDescriptionParameterBuilder create(String name, Type type) {
        return new ConfigDescriptionParameterBuilder(name, type);
     * Set the minimum value of the configuration parameter
     * @param min the min value of the {@link ConfigDescriptionParameter}
    public ConfigDescriptionParameterBuilder withMinimum(BigDecimal min) {
        this.min = min;
     * Set the maximum value of the configuration parameter
     * @param max the max value of the {@link ConfigDescriptionParameter}
    public ConfigDescriptionParameterBuilder withMaximum(BigDecimal max) {
        this.max = max;
     * Set the step size of the configuration parameter
     * @param step the step of the {@link ConfigDescriptionParameter}
    public ConfigDescriptionParameterBuilder withStepSize(BigDecimal step) {
        this.step = step;
     * Set the pattern of the configuration parameter
     * @param pattern the pattern for the {@link ConfigDescriptionParameter}
    public ConfigDescriptionParameterBuilder withPattern(String pattern) {
     * Set the configuration parameter as read only
     * @param readOnly <code>true</code> to make the parameter read only
    public ConfigDescriptionParameterBuilder withReadOnly(Boolean readOnly) {
     * Set the configuration parameter to allow multiple selection
     * @param multiple <code>true</code> for multiple selection
    public ConfigDescriptionParameterBuilder withMultiple(Boolean multiple) {
     * @param multipleLimit the parameters limit
    public ConfigDescriptionParameterBuilder withMultipleLimit(Integer multipleLimit) {
     * Set the context of the configuration parameter
     * @param context the context for this parameter
    public ConfigDescriptionParameterBuilder withContext(String context) {
     * Set the configuration parameter to be required
     * @param required <code>true</code> if the parameter is required
    public ConfigDescriptionParameterBuilder withRequired(Boolean required) {
     * Set the default value of the configuration parameter
    public ConfigDescriptionParameterBuilder withDefault(String defaultValue) {
     * Set the label of the configuration parameter
     * @param label a short user friendly description
    public ConfigDescriptionParameterBuilder withLabel(String label) {
     * Set the description of the configuration parameter
     * @param description a detailed user friendly description
    public ConfigDescriptionParameterBuilder withDescription(String description) {
     * Set the options of the configuration parameter
     * @param options the options for this parameter
    public ConfigDescriptionParameterBuilder withOptions(List<ParameterOption> options) {
     * Set the configuration parameter as an advanced parameter
     * @param advanced <code>true</code> to make the parameter advanced
    public ConfigDescriptionParameterBuilder withAdvanced(Boolean advanced) {
     * Set the configuration parameter as a verifyable parameter
     * @param verify flag
    public ConfigDescriptionParameterBuilder withVerify(Boolean verify) {
     * Set the configuration parameter to be limited to the values in the options list
     * @param limitToOptions <code>true</code> if only the declared options are acceptable
    public ConfigDescriptionParameterBuilder withLimitToOptions(Boolean limitToOptions) {
     * @param groupName the group name of this config description parameter
    public ConfigDescriptionParameterBuilder withGroupName(String groupName) {
     * Set the filter criteria of the configuration parameter
     * @param filterCriteria the filter criteria
    public ConfigDescriptionParameterBuilder withFilterCriteria(List<FilterCriteria> filterCriteria) {
        this.filterCriteria = filterCriteria;
     * Sets the unit of the configuration parameter.
     * @param unit the unit to be set
    public ConfigDescriptionParameterBuilder withUnit(String unit) {
     * Sets the unit label of the configuration parameter.
     * @param unitLabel the unit label to be set
    public ConfigDescriptionParameterBuilder withUnitLabel(String unitLabel) {
     * Builds a result with the settings of this builder.
    public ConfigDescriptionParameter build() throws IllegalArgumentException {
        return new ConfigDescriptionParameter(name, type, min, max, step, pattern, required, readOnly, multiple,
                context, defaultValue, label, description, options, filterCriteria, groupName, advanced, limitToOptions,
                multipleLimit, unit, unitLabel, verify);
