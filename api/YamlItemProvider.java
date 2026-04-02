 * {@link YamlItemProvider} is an OSGi service, that allows to define items in YAML configuration files.
 * Files can be added, updated or removed at runtime.
 * These items are automatically exposed to the {@link org.openhab.core.items.ItemRegistry}.
@Component(immediate = true, service = { ItemProvider.class, YamlItemProvider.class, YamlModelListener.class })
public class YamlItemProvider extends AbstractProvider<Item> implements ItemProvider, YamlModelListener<YamlItemDTO> {
    private final Logger logger = LoggerFactory.getLogger(YamlItemProvider.class);
    private CoreItemFactory itemFactory;
    private final YamlChannelLinkProvider itemChannelLinkProvider;
    private final YamlMetadataProvider metaDataProvider;
    public YamlItemProvider(final @Reference CoreItemFactory itemFactory,
            final @Reference YamlChannelLinkProvider itemChannelLinkProvider,
            final @Reference YamlMetadataProvider metaDataProvider, Map<String, Object> properties) {
        this.itemFactory = itemFactory;
        this.metaDataProvider = metaDataProvider;
        itemsMap.clear();
        return itemsMap.keySet().stream().filter(name -> !isIsolatedModel(name))
                .map(name -> itemsMap.getOrDefault(name, List.of())).flatMap(list -> list.stream()).toList();
    public Class<YamlItemDTO> getElementClass() {
        return YamlItemDTO.class;
    public boolean isVersionSupported(int version) {
        return version >= 1;
    public boolean isDeprecated() {
    public void addedModel(String modelName, Collection<YamlItemDTO> elements) {
        Map<Item, YamlItemDTO> added = new LinkedHashMap<>();
        elements.forEach(elt -> {
            Item item = mapItem(elt);
                added.put(item, elt);
        Collection<Item> modelItems = Objects
                .requireNonNull(itemsMap.computeIfAbsent(modelName, k -> new ArrayList<>()));
        modelItems.addAll(added.keySet());
        added.forEach((item, itemDTO) -> {
            String name = item.getName();
            logger.debug("model {} added item {}", modelName, name);
            processChannelLinks(modelName, name, itemDTO);
            processMetadata(modelName, name, itemDTO);
    public void updatedModel(String modelName, Collection<YamlItemDTO> elements) {
        Map<Item, YamlItemDTO> updated = new LinkedHashMap<>();
                updated.put(item, elt);
        updated.forEach((item, itemDTO) -> {
            modelItems.stream().filter(i -> i.getName().equals(name)).findFirst().ifPresentOrElse(oldItem -> {
                modelItems.remove(oldItem);
                modelItems.add(item);
                logger.debug("model {} updated item {}", modelName, name);
            }, () -> {
    public void removedModel(String modelName, Collection<YamlItemDTO> elements) {
        Collection<Item> modelItems = itemsMap.getOrDefault(modelName, List.of());
        elements.stream().map(elt -> elt.name).forEach(name -> {
                logger.debug("model {} removed item {}", modelName, name);
            }, () -> logger.debug("model {} item {} not found", modelName, name));
            processChannelLinks(modelName, name, null);
            processMetadata(modelName, name, null);
        if (modelItems.isEmpty()) {
    private @Nullable Item mapItem(YamlItemDTO itemDTO) {
        String name = itemDTO.name;
            if (GroupItem.TYPE.equalsIgnoreCase(itemDTO.type)) {
                YamlGroupDTO groupDTO = itemDTO.group;
                if (groupDTO != null) {
                    Item baseItem = createItemOfType(groupDTO.getBaseType(), name);
                        GroupFunctionDTO groupFunctionDto = new GroupFunctionDTO();
                        groupFunctionDto.name = groupDTO.getFunction();
                        groupFunctionDto.params = groupDTO.getParameters().toArray(new String[0]);
                        item = new GroupItem(name, baseItem, ItemDTOMapper.mapFunction(baseItem, groupFunctionDto));
                        item = new GroupItem(name);
                item = createItemOfType(itemDTO.getType(), name);
            logger.warn("Error creating item '{}', item will be ignored: {}", name, e.getMessage());
            item = null;
            genericItem.setLabel(itemDTO.label);
            genericItem.setCategory(itemDTO.icon);
            for (String tag : itemDTO.getTags()) {
                genericItem.addTag(tag);
            for (String groupName : itemDTO.getGroups()) {
                genericItem.addGroupName(groupName);
        Item item = itemFactory.createItem(itemType, itemName);
            logger.debug("Created item '{}' of type '{}'", itemName, itemType);
        logger.warn("CoreItemFactory cannot create item '{}' of type '{}'", itemName, itemType);
    private void processChannelLinks(String modelName, String itemName, @Nullable YamlItemDTO itemDTO) {
        Map<String, Configuration> channelLinks = new HashMap<>(2);
        if (itemDTO != null) {
            if (itemDTO.channel != null) {
                channelLinks.put(itemDTO.channel, new Configuration());
            if (itemDTO.channels != null) {
                itemDTO.channels.forEach((channelUID, config) -> {
                    channelLinks.put(channelUID, new Configuration(config));
            itemChannelLinkProvider.updateItemChannelLinks(modelName, itemName, channelLinks);
            logger.warn("Channel links configuration of item '{}' could not be parsed correctly.", itemName, e);
    private void processMetadata(String modelName, String itemName, @Nullable YamlItemDTO itemDTO) {
        Map<String, YamlMetadataDTO> metadata = new HashMap<>();
            boolean hasAutoUpdateMetadata = false;
            boolean hasUnitMetadata = false;
            boolean hasStateDescriptionMetadata = false;
            boolean hasExpireMetadata = false;
            if (itemDTO.metadata != null) {
                for (Map.Entry<String, YamlMetadataDTO> entry : itemDTO.metadata.entrySet()) {
                    if ("autoupdate".equals(entry.getKey())) {
                        hasAutoUpdateMetadata = true;
                    } else if ("unit".equals(entry.getKey())) {
                        hasUnitMetadata = true;
                    } else if ("stateDescription".equals(entry.getKey())) {
                        hasStateDescriptionMetadata = true;
                    } else if ("expire".equals(entry.getKey())) {
                        hasExpireMetadata = true;
                    Map<String, Object> config = entry.getValue().config;
                    if (itemDTO.format != null && "stateDescription".equals(entry.getKey())
                            && (entry.getValue().config == null || entry.getValue().config.get("pattern") == null)) {
                        config = new HashMap<>();
                        if (entry.getValue().config != null) {
                            for (Map.Entry<String, Object> confEntry : entry.getValue().config.entrySet()) {
                                config.put(confEntry.getKey(), confEntry.getValue());
                            config.put("pattern", itemDTO.format);
                    YamlMetadataDTO mdDTO = new YamlMetadataDTO();
                    mdDTO.value = entry.getValue().value;
                    mdDTO.config = config;
                    metadata.put(entry.getKey(), mdDTO);
            if (!hasAutoUpdateMetadata && itemDTO.autoupdate != null) {
                mdDTO.value = String.valueOf(itemDTO.autoupdate);
                metadata.put("autoupdate", mdDTO);
            if (!hasUnitMetadata && itemDTO.unit != null) {
                mdDTO.value = itemDTO.unit;
                metadata.put("unit", mdDTO);
            if (!hasStateDescriptionMetadata && itemDTO.format != null) {
                mdDTO.config = Map.of("pattern", itemDTO.format);
                metadata.put("stateDescription", mdDTO);
            if (!hasExpireMetadata && itemDTO.expire != null) {
                mdDTO.value = itemDTO.expire;
                metadata.put("expire", mdDTO);
        metaDataProvider.updateMetadata(modelName, itemName, metadata);
