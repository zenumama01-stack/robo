package org.openhab.core.thing.xml.internal;
 * The {@link AbstractDescriptionTypeConverter} is an abstract implementation of the {@code XStream} {@link Converter}
 * interface used as helper class to convert type
 * definition information within an XML document into a concrete type object.
 * This class should be used for each type definition which inherits from the {@link AbstractDescriptionType} class.
public abstract class AbstractDescriptionTypeConverter<T> extends GenericUnmarshaller<T> {
    protected ConverterAttributeMapValidator attributeMapValidator;
     * @param type the name of the type (e.g. "thing-type", "channel-type")
    protected AbstractDescriptionTypeConverter(Class<T> clazz, String type) {
        super(clazz);
        this.attributeMapValidator = new ConverterAttributeMapValidator(new String[][] { { "id", "true" } });
     * Returns the {@code id} attribute of the specific XML type definition.
     * @param attributes the attributes where to extract the ID information
     * @return the ID of the type definition (not empty)
    protected String getID(Map<String, String> attributes) {
        return requireNonEmpty(attributes.get("id"), "The 'id' attribute cannot be null or empty!");
     * Returns the full extracted UID of the specific XML type definition.
     * @param context the context where to extract the binding ID information
     * @return the UID of the type definition (not empty)
    protected String getUID(Map<String, String> attributes, UnmarshallingContext context) {
        String bindingId = (String) context.get("thing-descriptions.bindingId");
        String typeId = getID(attributes);
        return String.format("%s:%s", bindingId, typeId);
     * Returns the value of the {@code label} tag from the specific XML type definition.
     * @param nodeIterator the iterator to be used to extract the information
     * @return the value of the label (not empty)
     * @throws ConversionException if the label could not be read
    protected String readLabel(NodeIterator nodeIterator) throws ConversionException {
        return requireNonEmpty((String) nodeIterator.nextValue("label", true),
                "The 'label' node cannot be null or empty!");
     * Returns the value of the {@code description} tag from the specific XML type definition.
     * @return the value of the description (could be empty)
    protected @Nullable String readDescription(NodeIterator nodeIterator) {
        return (String) nodeIterator.nextValue("description", false);
    private @Nullable URI readConfigDescriptionURI(NodeIterator nodeIterator) throws ConversionException {
        String uriText = nodeIterator.nextAttribute("config-description-ref", "uri", false);
        if (uriText != null) {
                return new URI(uriText);
                throw new ConversionException("The URI '" + uriText + "' in node 'config-description-ref' is invalid!",
            if (nextNode instanceof ConfigDescription description) {
     * Returns the value of the {@code config-description-ref} and the {@code config-description} tags from the specific
     * XML type definition.
     * @param nodeIterator the iterator to be used to extract the information (must not be null)
     * @return the URI and configuration object
     *         (contains two elements: URI - could be null, ConfigDescription - could be null)
    protected Object[] getConfigDescriptionObjects(NodeIterator nodeIterator) {
        URI configDescriptionURI = readConfigDescriptionURI(nodeIterator);
                configDescriptionURI = configDescription.getUID();
        return new Object[] { configDescriptionURI, configDescription };
     * The abstract unmarshal method which must be implemented by the according type converter.
     * @param reader the reader to be used to read XML information from a stream (not null)
     * @param context the context to be used for the XML document conversion (not null)
     * @param attributes the attributes map containing attributes of the type - only UID -
     *            (not null, could be empty)
     * @param nodeIterator the iterator to be used to simply extract information in the right
     *            order and appearance from the XML stream
     * @return the concrete type definition object (could be null)
     * @throws ConversionException if any conversion error occurs
    protected abstract @Nullable T unmarshalType(HierarchicalStreamReader reader, UnmarshallingContext context,
            Map<String, String> attributes, NodeIterator nodeIterator) throws ConversionException;
        context.put("config-description.uri", type + ":" + getUID(attributes, context));
        Object object = unmarshalType(reader, context, attributes, nodeIterator);
