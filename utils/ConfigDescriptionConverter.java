import com.thoughtworks.xstream.converters.Converter;
 * The {@link ConfigDescriptionConverter} is a concrete implementation of the {@code XStream} {@link Converter}
 * interface used to convert config
 * description information within an XML document into a {@link ConfigDescription} object.
 * This converter converts {@code config-description} XML tags.
 * @author Chris Jackson - Added configuration groups
public class ConfigDescriptionConverter extends GenericUnmarshaller<ConfigDescription> {
    private ConverterAttributeMapValidator attributeMapValidator;
    public ConfigDescriptionConverter() {
        super(ConfigDescription.class);
        this.attributeMapValidator = new ConverterAttributeMapValidator(new String[][] { { "uri", "false" } });
        Map<String, String> attributes = this.attributeMapValidator.readValidatedAttributes(reader);
        String uriText = attributes.get("uri");
        if (uriText == null) {
            // the URI could be overridden by a context field if it could be
            // automatically extracted
            uriText = (String) context.get("config-description.uri");
        URI uri;
            throw new ConversionException("No URI provided");
            uri = new URI(uriText);
        } catch (URISyntaxException ex) {
            throw new ConversionException(
                    "The URI '" + uriText + "' in node '" + reader.getNodeName() + "' is invalid!", ex);
        // create the lists to hold parameters and groups
        List<ConfigDescriptionParameter> configDescriptionParams = new ArrayList<>();
        List<ConfigDescriptionParameterGroup> configDescriptionGroups = new ArrayList<>();
        // iterate through the nodes, putting the different types into their
        // respective arrays
        while (nodeIterator.hasNext()) {
            Object node = nodeIterator.next();
            if (node instanceof ConfigDescriptionParameter parameter) {
                configDescriptionParams.add(parameter);
            if (node instanceof ConfigDescriptionParameterGroup group) {
                configDescriptionGroups.add(group);
        if (reader.hasMoreChildren()) {
            throw new ConversionException("The document is invalid, it contains unsupported data!");
        return ConfigDescriptionBuilder.create(uri).withParameters(configDescriptionParams)
                .withParameterGroups(configDescriptionGroups).build();
