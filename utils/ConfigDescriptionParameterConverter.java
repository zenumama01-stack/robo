import org.openhab.core.config.core.xml.util.ConverterValueMap;
 * The {@link ConfigDescriptionParameterConverter} is a concrete implementation
 * of the {@code XStream} {@link Converter} interface used to convert config
 * description parameters information within an XML document into a {@link ConfigDescriptionParameter} object.
 * This converter converts {@code parameter} XML tags.
 * @author Chris Jackson - Modified to use config parameter builder. Added
 *         parameters.
public class ConfigDescriptionParameterConverter extends GenericUnmarshaller<ConfigDescriptionParameter> {
    public ConfigDescriptionParameterConverter() {
        super(ConfigDescriptionParameter.class);
        this.attributeMapValidator = new ConverterAttributeMapValidator(
                new String[][] { { "name", "true" }, { "type", "true" }, { "min", "false" }, { "max", "false" },
                        { "step", "false" }, { "pattern", "false" }, { "required", "false" }, { "readOnly", "false" },
                        { "multiple", "false" }, { "groupName", "false" }, { "unit", "false" } });
    private @Nullable Type toType(@Nullable String xmlType) {
        if (xmlType != null) {
            return Type.valueOf(xmlType.toUpperCase());
    private @Nullable BigDecimal toNumber(@Nullable String value) {
                return new BigDecimal(value);
            throw new ConversionException("The value '" + value + "' could not be converted to a decimal number.", e);
    private @Nullable Boolean toBoolean(@Nullable String val) {
        if (val == null) {
        return Boolean.valueOf(val);
    private Boolean falseIfNull(@Nullable Boolean b) {
        return b != null && b;
        ConfigDescriptionParameter configDescriptionParam;
        String name = attributes.get("name");
        Type type = toType(attributes.get("type"));
        BigDecimal min = toNumber(attributes.get("min"));
        BigDecimal max = toNumber(attributes.get("max"));
        BigDecimal step = toNumber(attributes.get("step"));
        String patternString = attributes.get("pattern");
        Boolean required = toBoolean(attributes.get("required"));
        Boolean readOnly = falseIfNull(toBoolean(attributes.get("readOnly")));
        Boolean multiple = falseIfNull(toBoolean(attributes.get("multiple")));
        String groupName = attributes.get("groupName");
        String unit = attributes.get("unit");
        ConverterValueMap valueMap = new ConverterValueMap(reader, context);
        String parameterContext = valueMap.getString("context");
        if (required == null) {
            // fallback to deprecated "required" element
            required = valueMap.getBoolean("required", false);
        String defaultValue = valueMap.getString("default");
        String label = valueMap.getString("label");
        String description = valueMap.getString("description");
        Boolean advanced = valueMap.getBoolean("advanced", false);
        Boolean verify = valueMap.getBoolean("verify", false);
        Boolean limitToOptions = valueMap.getBoolean("limitToOptions", true);
        Integer multipleLimit = valueMap.getInteger("multipleLimit");
        String unitLabel = null;
            unitLabel = valueMap.getString("unitLabel");
        // read options and filter criteria
        List<ParameterOption> options = readParameterOptions(valueMap.getObject("options"));
        List<FilterCriteria> filterCriteria = (List<FilterCriteria>) valueMap.getObject("filter");
        configDescriptionParam = ConfigDescriptionParameterBuilder.create(name, type).withMinimum(min).withMaximum(max)
                .withStepSize(step).withPattern(patternString).withRequired(required).withReadOnly(readOnly)
                .withMultiple(multiple).withContext(parameterContext).withDefault(defaultValue).withLabel(label)
                .withDescription(description).withOptions(options).withFilterCriteria(filterCriteria)
                .withGroupName(groupName).withAdvanced(advanced).withVerify(verify).withLimitToOptions(limitToOptions)
                .withMultipleLimit(multipleLimit).withUnit(unit).withUnitLabel(unitLabel).build();
        return configDescriptionParam;
    private @Nullable List<ParameterOption> readParameterOptions(@Nullable Object rawNodeValueList) {
        if (rawNodeValueList instanceof List<?> list) {
            List<ParameterOption> result = new ArrayList<>();
            for (Object object : list) {
                if (object instanceof NodeValue nodeValue) {
                    String value = nodeValue.getAttributes().get("value");
                    String label = nodeValue.getValue().toString();
                    result.add(new ParameterOption(value, label));
