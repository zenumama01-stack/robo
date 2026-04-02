package org.openhab.core.model.item.internal.fileconverter;
import org.openhab.core.items.fileconverter.AbstractItemSerializer;
import org.openhab.core.model.item.internal.GenericMetadataProvider;
import org.openhab.core.model.items.ItemsFactory;
import org.openhab.core.model.items.ModelProperty;
 * {@link DslItemConverter} is the DSL converter for {@link Item} objects
 * with the capabilities of parsing and serializing files.
@Component(immediate = true, service = { ItemSerializer.class, ItemParser.class })
public class DslItemConverter extends AbstractItemSerializer implements ItemParser {
    private final Logger logger = LoggerFactory.getLogger(DslItemConverter.class);
    private final Map<String, ItemModel> elementsToGenerate = new ConcurrentHashMap<>();
    private final GenericItemProvider itemProvider;
    private final GenericMetadataProvider metadataProvider;
    public DslItemConverter(final @Reference ModelRepository modelRepository,
            final @Reference GenericItemProvider itemProvider,
            final @Reference GenericMetadataProvider metadataProvider,
    public String getGeneratedFormat() {
        return "DSL";
    public void setItemsToBeSerialized(String id, List<Item> items, Collection<Metadata> metadata,
            Map<String, String> stateFormatters, boolean hideDefaultParameters) {
        ItemModel model = ItemsFactory.eINSTANCE.createItemModel();
            model.getItems().add(buildModelItem(item, getChannelLinks(metadata, item.getName()),
                    getMetadata(metadata, item.getName()), stateFormatters.get(item.getName()), hideDefaultParameters));
        elementsToGenerate.put(id, model);
    public void generateFormat(String id, OutputStream out) {
        ItemModel model = elementsToGenerate.remove(id);
            modelRepository.generateFileFormat(out, "items", model);
    private ModelItem buildModelItem(Item item, List<Metadata> channelLinks, List<Metadata> metadata,
            @Nullable String stateFormatter, boolean hideDefaultParameters) {
        ModelItem model = ItemsFactory.eINSTANCE.createModelItem();
            Item baseItem = groupItem.getBaseItem();
            List<String> groupType = new ArrayList<>();
            groupType.add(groupItem.getType());
                groupType.add(baseItem.getType());
                GroupFunction function = groupItem.getFunction();
                if (function != null) {
                    groupType.add(function.getClass().getSimpleName().toUpperCase());
                    State[] parameters = function.getParameters();
                    for (int i = 0; i < parameters.length; i++) {
                        model.getArgs().add(parameters[i].toString());
            model.setType(groupType.stream().collect(Collectors.joining(ItemUtil.EXTENSION_SEPARATOR)));
            model.setType(item.getType());
        model.setName(item.getName());
        String label = item.getLabel();
        boolean patternInjected = false;
        String defaultPattern = getDefaultStatePattern(item);
        String patternToInject = stateFormatter != null && !stateFormatter.equals(defaultPattern) ? stateFormatter
        if (patternToInject != null) {
            // Inject the pattern in the label
            patternInjected = true;
                model.setLabel("%s [%s]".formatted(label, patternToInject));
                model.setLabel("[%s]".formatted(patternToInject));
            model.setLabel(label);
        String category = item.getCategory();
        if (category != null && !category.isEmpty()) {
            model.setIcon(category);
        for (String group : item.getGroupNames()) {
            model.getGroups().add(group);
        for (String tag : item.getTags().stream().sorted().collect(Collectors.toList())) {
            model.getTags().add(tag);
        for (Metadata md : channelLinks) {
            String namespace = md.getUID().getNamespace();
            ModelBinding binding = ItemsFactory.eINSTANCE.createModelBinding();
            binding.setType(namespace);
            binding.setConfiguration(md.getValue());
            for (ConfigParameter param : getConfigurationParameters(md, hideDefaultParameters)) {
                ModelProperty property = buildModelProperty(param.name(), param.value());
                if (property != null) {
                    binding.getProperties().add(property);
                            "Item \"{}\": configuration parameter \"{}\" for channel link \"{}\" is ignored because its value type is not supported!",
                            item.getName(), param.name(), md.getValue());
            model.getBindings().add(binding);
        for (Metadata md : metadata) {
            String statePattern = null;
            for (ConfigParameter param : getConfigurationParameters(md)) {
                            "Item \"{}\": configuration parameter \"{}\" for metadata namespace \"{}\" is ignored because its value type is not supported!",
                            item.getName(), param.name(), namespace);
                if ("stateDescription".equals(namespace) && "pattern".equals(param.name())) {
                    statePattern = param.value().toString();
            // Ignore state description in case it contains only a state pattern and state pattern was injected
            // in the item label or is the default pattern
            if (!(statePattern != null && binding.getProperties().size() == 1
                    && (patternInjected || statePattern.equals(defaultPattern)))) {
                // Avoid injecting the state pattern in label if already present in stateDescription metadata
                if (patternInjected && statePattern != null) {
    private @Nullable ModelProperty buildModelProperty(String key, Object value) {
        ModelProperty property = ItemsFactory.eINSTANCE.createModelProperty();
        property.setKey(key);
        if (value instanceof List<?> list) {
                for (Object val : list) {
                    if (val instanceof String || val instanceof BigDecimal || val instanceof Boolean) {
                        property.getValue().add(val);
                    } else if (val instanceof Double doubleValue) {
                        property.getValue().add(BigDecimal.valueOf(doubleValue));
        } else if (value instanceof String || value instanceof BigDecimal || value instanceof Boolean) {
            property.getValue().add(value);
        } else if (value instanceof Double doubleValue) {
            // It was discovered that configuration parameter value of an item metadata can be of type Double
            // when the metadata is added through Main UI.
            // A conversion to a BigDecimal is then required to avoid an exception later when generating DSL
     * Get the list of configuration parameters for a channel link.
     * If a profile is set and a configuration description is found for this profile, the parameters are provided
     * in the same order as in this configuration description, and any parameter having the default value is ignored.
     * If no profile is set, the parameters are provided sorted by natural order of their names.
    private List<ConfigParameter> getConfigurationParameters(Metadata metadata, boolean hideDefaultParameters) {
        List<ConfigParameter> parameters = new ArrayList<>();
        Set<String> handledNames = new HashSet<>();
        Map<String, Object> configParameters = metadata.getConfiguration();
        Object profile = configParameters.get("profile");
        List<ConfigDescriptionParameter> configDescriptionParameter = List.of();
        if (profile instanceof String profileStr) {
            parameters.add(new ConfigParameter("profile", profileStr));
            handledNames.add("profile");
                        .getConfigDescription(new URI("profile:" + profileStr));
                    configDescriptionParameter = configDesc.getParameters();
                // Ignored; in practice this will never be thrown
        for (ConfigDescriptionParameter param : configDescriptionParameter) {
            String paramName = param.getName();
            if (handledNames.contains(paramName)) {
            Object value = configParameters.get(paramName);
            Object defaultValue = ConfigUtil.getDefaultValueAsCorrectType(param);
            if (value != null && (!hideDefaultParameters || !value.equals(defaultValue))) {
                parameters.add(new ConfigParameter(paramName, value));
            handledNames.add(paramName);
        for (String paramName : configParameters.keySet().stream().sorted().collect(Collectors.toList())) {
    public String getParserFormat() {
    public @Nullable String startParsingFormat(String syntax, List<String> errors, List<String> warnings) {
        ByteArrayInputStream inputStream = new ByteArrayInputStream(syntax.getBytes());
        return modelRepository.createIsolatedModel("items", inputStream, errors, warnings);
    public Collection<Item> getParsedObjects(String modelName) {
        return itemProvider.getAllFromModel(modelName);
    public Collection<Metadata> getParsedMetadata(String modelName) {
        return metadataProvider.getAllFromModel(modelName);
    public Map<String, String> getParsedStateFormatters(String modelName) {
        return itemProvider.getStateFormattersFromModel(modelName);
    public void finishParsingFormat(String modelName) {
        modelRepository.removeModel(modelName);
