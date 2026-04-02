import org.openhab.core.config.core.ConfigDescription;
import org.openhab.core.config.core.ConfigDescriptionBuilder;
import org.openhab.core.config.core.xml.util.ConverterAttributeMapValidator;
 * The {@link AddonInfoConverter} is a concrete implementation of the {@code XStream}
 * {@link com.thoughtworks.xstream.converters.Converter} interface used
 * to convert add-on information within an XML document into a {@link AddonInfoXmlResult} object.
 * This converter converts {@code addon} XML tags.
 * @author Andrew Fiddian-Green - Added discovery methods
public class AddonInfoConverter extends GenericUnmarshaller<AddonInfoXmlResult> {
    private static final String CONFIG_DESCRIPTION_URI_PLACEHOLDER = "addonInfoConverter:placeHolder";
    private final ConverterAttributeMapValidator attributeMapValidator;
    public AddonInfoConverter() {
        super(AddonInfoXmlResult.class);
        attributeMapValidator = new ConverterAttributeMapValidator(Map.of("id", true, "schemaLocation", false));
    private @Nullable ConfigDescription readConfigDescription(NodeIterator nodeIterator) {
        Object nextNode = nodeIterator.next();
        if (nextNode != null) {
            if (nextNode instanceof ConfigDescription configDescription) {
                return configDescription;
            nodeIterator.revert();
        // read attributes
        Map<String, String> attributes = attributeMapValidator.readValidatedAttributes(reader);
        String id = requireNonEmpty(attributes.get("id"), "Add-on id attribute is null or empty");
        // set automatically extracted URI for a possible 'config-description' section
        context.put("config-description.uri", CONFIG_DESCRIPTION_URI_PLACEHOLDER);
        // read values
        String type = requireNonEmpty((String) nodeIterator.nextValue("type", true), "Add-on type is null or empty");
        String name = requireNonEmpty((String) nodeIterator.nextValue("name", true),
                "Add-on name attribute is null or empty");
        String description = requireNonEmpty((String) nodeIterator.nextValue("description", true),
                "Add-on description is null or empty");
        AddonInfo.Builder addonInfo = AddonInfo.builder(id, type).withName(name).withDescription(description);
        addonInfo.withKeywords((String) nodeIterator.nextValue("keywords", false));
        addonInfo.withConnection((String) nodeIterator.nextValue("connection", false));
        addonInfo.withCountries((String) nodeIterator.nextValue("countries", false));
        addonInfo.withServiceId((String) nodeIterator.nextValue("service-id", false));
        String configDescriptionURI = nodeIterator.nextAttribute("config-description-ref", "uri", false);
        ConfigDescription configDescription = null;
        if (configDescriptionURI == null) {
            configDescription = readConfigDescription(nodeIterator);
            if (configDescription != null) {
                configDescriptionURI = configDescription.getUID().toString();
                // if config description is missing the URI, recreate it with correct URI
                if (CONFIG_DESCRIPTION_URI_PLACEHOLDER.equals(configDescriptionURI)) {
                    configDescriptionURI = type + ":" + id;
                    configDescription = ConfigDescriptionBuilder.create(URI.create(configDescriptionURI))
                            .withParameterGroups(configDescription.getParameterGroups())
                            .withParameters(configDescription.getParameters()).build();
        addonInfo.withConfigDescriptionURI(configDescriptionURI);
        Object object = nodeIterator.nextList("discovery-methods", false);
        addonInfo.withDiscoveryMethods(!(object instanceof List<?> list) ? null
                : list.stream().filter(AddonDiscoveryMethod.class::isInstance).map(e -> ((AddonDiscoveryMethod) e))
                        .toList());
        // create object
        return new AddonInfoXmlResult(addonInfo.build(), configDescription);
