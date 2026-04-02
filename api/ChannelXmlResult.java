 * The {@link ChannelXmlResult} is an intermediate XML conversion result object.
public class ChannelXmlResult {
    private final String typeId;
    private final @Nullable List<NodeValue> properties;
     * Constructs a new {@link ChannelXmlResult}
     * @param id the channel id
     * @param typeId the channel type id
     * @param label the channel label
     * @param description the channel description
     * @param properties a {@link List} of channel properties
    public ChannelXmlResult(String id, String typeId, @Nullable String label, @Nullable String description,
            @Nullable List<NodeValue> properties, @Nullable AutoUpdatePolicy autoUpdatePolicy) {
        this.typeId = typeId;
     * Retrieves the ID for this channel
     * @return channel id
     * Retrieves the type ID for this channel
     * @return type ID
     * Retrieves the properties for this channel
     * @return properties list
    public List<NodeValue> getProperties() {
        return Objects.requireNonNullElse(properties, List.of());
     * Get the label for this channel
     * @return the channel label
     * Get the description for this channel
     * @return the channel description
     * Get the auto update policy for this channel.
        return "ChannelXmlResult [id=" + id + ", typeId=" + typeId + ", properties=" + properties + "]";
    protected ChannelDefinition toChannelDefinition(String bindingId) throws ConversionException {
        String typeUID = getTypeUID(bindingId, typeId);
        // Convert the channel properties into a map
        Map<String, String> propertiesMap = new HashMap<>();
        for (NodeValue property : getProperties()) {
            Map<String, String> attributes = property.getAttributes();
                String value = (String) property.getValue();
                if (name != null && value != null) {
                    propertiesMap.put(name, value);
        return new ChannelDefinitionBuilder(id, new ChannelTypeUID(typeUID)).withProperties(propertiesMap)
                .withLabel(getLabel()).withDescription(getDescription()).withAutoUpdatePolicy(getAutoUpdatePolicy())
    private String getTypeUID(String bindingId, String typeId) {
        if (typeId.startsWith(XmlHelper.SYSTEM_NAMESPACE_PREFIX)) {
            return XmlHelper.getSystemUID(typeId);
