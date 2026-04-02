 * The {@link ConfigDescriptionParameterGroupConverter} creates a {@link ConfigDescriptionParameterGroup} instance from
 * an {@code option} XML node.
public class ConfigDescriptionParameterGroupConverter extends GenericUnmarshaller<ConfigDescriptionParameterGroup> {
    public ConfigDescriptionParameterGroupConverter() {
        super(ConfigDescriptionParameterGroup.class);
    public @Nullable Object unmarshal(HierarchicalStreamReader reader, UnmarshallingContext marshallingContext) {
        String name = reader.getAttribute("name");
        // Read values
        ConverterValueMap valueMap = new ConverterValueMap(reader, marshallingContext);
        String context = valueMap.getString("context");
        return ConfigDescriptionParameterGroupBuilder.create(name) //
                .withContext(context) //
                .withAdvanced(advanced) //
                .withLabel(label) //
                .withDescription(description) //
