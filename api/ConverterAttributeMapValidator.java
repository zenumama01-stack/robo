package org.openhab.core.config.core.xml.util;
 * The {@link ConverterAttributeMapValidator} class reads any attributes of a node, validates
 * if they appear or not, and returns the validated key-value pair map.
public class ConverterAttributeMapValidator {
    private @Nullable Map<String, Boolean> validationMaskTemplate;
     * Creates a new instance of this class with the specified parameter.
     * The validation mask template is a two-dimensional key-required ({@code String, [true|false]}) list, defining all
     * attributes the node could have and which are required and which not. The structure of the array is the following:
     * <code><pre>
     * String[] validationMaskTemplate = new String[][] {
     *         { "uri", "false" },
     *         { "attribute", "true" }};
     * </pre></code>
     * If the validation mask template is {@code null} the validation is skipped. If it's empty, no attributes are
     * allowed for this node.
     * @param validationMaskTemplate the two-dimensional key-required list (could be null or empty)
    public ConverterAttributeMapValidator(String @Nullable [][] validationMaskTemplate) {
        if (validationMaskTemplate != null) {
            Map<String, Boolean> template = new HashMap<>(validationMaskTemplate.length);
            for (String[] validationProperty : validationMaskTemplate) {
                template.put(validationProperty[0], Boolean.parseBoolean(validationProperty[1]));
            this.validationMaskTemplate = template;
     * The validation mask template is a key-required ({@code String, [true|false]}) map, defining all attributes the
     * node could have, and which are required and which not.
     * @param validationMaskTemplate the key-required map (could be null or empty)
    public ConverterAttributeMapValidator(@Nullable Map<String, Boolean> validationMaskTemplate) {
        this.validationMaskTemplate = validationMaskTemplate;
     * Reads, validates and returns all attributes of a node associated with the specified
     * reader as key-value map.
     * @param reader the reader to be used to read-in all attributes of the node (must not be null)
     * @return the key-value map (not null, could be empty)
     * @throws ConversionException if the validation check fails
    public Map<String, String> readValidatedAttributes(HierarchicalStreamReader reader) throws ConversionException {
        return readValidatedAttributes(reader, validationMaskTemplate);
    public static Map<String, String> readValidatedAttributes(HierarchicalStreamReader reader,
            @Nullable Map<String, Boolean> validationMaskTemplate) throws ConversionException {
        Map<String, String> attributeMap = new HashMap<>(reader.getAttributeCount());
        Map<String, Boolean> validationMask = null;
            // create a new one, because entries are removed during validation
            validationMask = new HashMap<>(validationMaskTemplate);
        for (int index = 0; index < reader.getAttributeCount(); index++) {
            String attributeName = reader.getAttributeName(index);
            if ((validationMask == null) || validationMask.containsKey(attributeName)) {
                attributeMap.put(attributeName, reader.getAttribute(index));
                if (validationMask != null) {
                    validationMask.remove(attributeName); // no duplicates are allowed
                throw new ConversionException("The attribute '" + attributeName + "' of the node '"
                        + reader.getNodeName() + "' is not supported or exists multiple times!");
        // there are still attributes in the validation mask left -> check if they are required
        if (validationMask != null && !validationMask.isEmpty()) {
            for (Map.Entry<String, Boolean> entry : validationMask.entrySet()) {
                String attributeName = entry.getKey();
                boolean attributeRequired = entry.getValue();
                if (attributeRequired) {
                            + reader.getNodeName() + "' is missing!");
        return attributeMap;
