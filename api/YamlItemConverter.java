package org.openhab.core.model.yaml.internal.items.fileconverter;
import org.openhab.core.items.GroupFunction.Equality;
import org.openhab.core.model.yaml.internal.items.YamlChannelLinkProvider;
import org.openhab.core.model.yaml.internal.items.YamlGroupDTO;
import org.openhab.core.model.yaml.internal.items.YamlItemProvider;
import org.openhab.core.model.yaml.internal.items.YamlMetadataDTO;
import org.openhab.core.model.yaml.internal.items.YamlMetadataProvider;
 * {@link YamlItemConverter} is the YAML converter for {@link Item} objects.
public class YamlItemConverter extends AbstractItemSerializer implements ItemParser {
    private final YamlModelRepository modelRepository;
    private final YamlItemProvider itemProvider;
    private final YamlMetadataProvider metadataProvider;
    private final YamlChannelLinkProvider channelLinkProvider;
    public YamlItemConverter(final @Reference YamlModelRepository modelRepository,
            final @Reference YamlItemProvider itemProvider, final @Reference YamlMetadataProvider metadataProvider,
            final @Reference YamlChannelLinkProvider channelLinkProvider,
        this.channelLinkProvider = channelLinkProvider;
        this.configDescriptionRegistry = configDescRegistry;
        return "YAML";
        List<YamlElement> elements = new ArrayList<>();
            elements.add(buildItemDTO(item, getChannelLinks(metadata, item.getName()),
        modelRepository.addElementsToBeGenerated(id, elements);
        modelRepository.generateFileFormat(id, out);
    private YamlItemDTO buildItemDTO(Item item, List<Metadata> channelLinks, List<Metadata> metadata,
        YamlItemDTO dto = new YamlItemDTO();
        dto.name = item.getName();
        boolean patternSet = false;
            dto.label = item.getLabel();
        String patternToSet = stateFormatter != null && !stateFormatter.equals(defaultPattern) ? stateFormatter : null;
        dto.format = patternToSet;
        patternSet = patternToSet != null;
        dto.type = item.getType();
        String mainType = ItemUtil.getMainItemType(item.getType());
        String dimension = ItemUtil.getItemTypeExtension(item.getType());
        if (CoreItemFactory.NUMBER.equals(mainType) && dimension != null) {
            dto.type = mainType;
            dto.dimension = dimension;
                dto.group = new YamlGroupDTO();
                dto.group.type = baseItem.getType();
                mainType = ItemUtil.getMainItemType(baseItem.getType());
                dimension = ItemUtil.getItemTypeExtension(baseItem.getType());
                    dto.group.type = mainType;
                    dto.group.dimension = dimension;
                if (function != null && !(function instanceof Equality)) {
                    dto.group.function = function.getClass().getSimpleName();
                    List<String> params = new ArrayList<>();
                        params.add(parameters[i].toString());
                    dto.group.parameters = params.isEmpty() ? null : params;
            dto.icon = category;
        if (!item.getGroupNames().isEmpty()) {
            dto.groups = new ArrayList<>();
            item.getGroupNames().forEach(group -> {
                dto.groups.add(group);
        if (!item.getTags().isEmpty()) {
            dto.tags = new LinkedHashSet<>();
            item.getTags().stream().sorted().collect(Collectors.toList()).forEach(tag -> {
                dto.tags.add(tag);
        if (channelLinks.size() == 1 && channelLinks.getFirst().getConfiguration().isEmpty()) {
            dto.channel = channelLinks.getFirst().getValue();
        } else if (!channelLinks.isEmpty()) {
            dto.channels = new LinkedHashMap<>();
            channelLinks.forEach(md -> {
                Map<String, Object> configuration = new LinkedHashMap<>();
                getConfigurationParameters(md, hideDefaultParameters).forEach(param -> {
                    configuration.put(param.name(), param.value());
                dto.channels.put(md.getValue(), configuration);
        Map<String, YamlMetadataDTO> metadataDto = new LinkedHashMap<>();
            String value = md.getValue();
            if ("autoupdate".equals(namespace)) {
                // When autoupdate value is an empty string, treat it as not set since dto.autoupdate only accepts
                // true/false
                    dto.autoupdate = Boolean.valueOf(value);
            } else if ("unit".equals(namespace)) {
                dto.unit = value; // When unit value is empty string, keep it as empty string
            } else if ("expire".equals(namespace)) {
                Map<String, Object> configuration = md.getConfiguration();
                if (configuration.isEmpty()) {
                    dto.expire = value; // When expire value is empty string, keep it as empty string
                    YamlMetadataDTO mdDto = new YamlMetadataDTO();
                    mdDto.value = value.isEmpty() ? null : value;
                    mdDto.config = configuration;
                    metadataDto.put(namespace, mdDto);
                // in field format or is the default pattern
                if (!(statePattern != null && configuration.size() == 1
                        && (patternSet || statePattern.equals(defaultPattern)))) {
                    mdDto.config = configuration.isEmpty() ? null : configuration;
                    if (patternSet && statePattern != null) {
                        dto.format = null;
        dto.metadata = metadataDto.isEmpty() ? null : metadataDto;
        return modelRepository.createIsolatedModel(inputStream, errors, warnings);
        getParsedMetadata(modelName).forEach(md -> {
            if ("stateDescription".equals(md.getUID().getNamespace())) {
                Object pattern = md.getConfiguration().get("pattern");
                if (pattern instanceof String patternStr && !patternStr.isBlank()) {
                    stateFormatters.put(md.getUID().getItemName(), patternStr);
        return stateFormatters;
        modelRepository.removeIsolatedModel(modelName);
