 * The {@link StateDescriptionConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface
 * used to convert a state description within an XML document
 * into a {@link StateDescription} object.
public class StateDescriptionConverter extends GenericUnmarshaller<StateDescription> {
    public StateDescriptionConverter() {
        super(StateDescription.class);
        this.attributeMapValidator = new ConverterAttributeMapValidator(new String[][] { { "min", "false" },
                { "max", "false" }, { "step", "false" }, { "pattern", "false" }, { "readOnly", "false" } });
    private @Nullable BigDecimal toBigDecimal(Map<String, String> attributes, String attribute,
            @Nullable BigDecimal defaultValue) throws ConversionException {
        String attrValueText = attributes.get(attribute);
        if (attrValueText != null) {
                return new BigDecimal(attrValueText);
                        "The attribute '" + attribute + "' has not a valid decimal number format!", nfe);
    private boolean toBoolean(Map<String, String> attributes, String attribute, boolean defaultValue) {
        return attrValueText == null ? defaultValue : Boolean.parseBoolean(attrValueText);
    private List<StateOption> toListOfChannelState(NodeList nodeList) throws ConversionException {
            List<StateOption> stateOptions = new ArrayList<>();
                stateOptions.add(toChannelStateOption((NodeValue) nodeObject));
            return stateOptions;
    private StateOption toChannelStateOption(NodeValue nodeValue) throws ConversionException {
            final Map<String, String> attributes = nodeValue.getAttributes();
            final String value = Optional.ofNullable(attributes).map(entry -> entry.get("value"))
                    .orElseThrow(() -> new ConversionException("The node 'option' requires the attribute 'value'!"));
            return new StateOption(value, nodeValue.getValue().toString());
        boolean readOnly = toBoolean(attributes, "readOnly", false);
        StateDescriptionFragmentBuilder builder = StateDescriptionFragmentBuilder.create().withReadOnly(readOnly);
        BigDecimal minimum = toBigDecimal(attributes, "min", null);
        if (minimum != null) {
            builder.withMinimum(minimum);
        BigDecimal maximum = toBigDecimal(attributes, "max", null);
        if (maximum != null) {
            builder.withMaximum(maximum);
        BigDecimal step = toBigDecimal(attributes, "step", null);
            builder.withStep(step);
        String pattern = attributes.get("pattern");
            builder.withPattern(pattern);
            builder.withOptions(toListOfChannelState(optionNodes));
