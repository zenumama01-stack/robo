package org.openhab.core.io.rest.core.internal.fileformat;
import javax.ws.rs.BadRequestException;
import org.openhab.core.io.rest.core.fileformat.ExtendedFileFormatDTO;
import org.openhab.core.io.rest.core.fileformat.FileFormatDTO;
import org.openhab.core.io.rest.core.fileformat.FileFormatItemDTO;
import org.openhab.core.io.rest.core.fileformat.FileFormatItemDTOMapper;
import org.openhab.core.items.fileconverter.ItemParser;
import org.openhab.core.items.fileconverter.ItemSerializer;
import org.openhab.core.thing.dto.ChannelDTO;
import org.openhab.core.thing.dto.ThingDTOMapper;
import org.openhab.core.thing.fileconverter.ThingParser;
import org.openhab.core.thing.fileconverter.ThingSerializer;
import org.openhab.core.thing.type.BridgeType;
import org.openhab.core.thing.util.ThingHelper;
import org.openhab.core.types.StateDescription;
import io.swagger.v3.oas.annotations.parameters.RequestBody;
 * This class acts as a REST resource and provides different methods to generate file format
 * for existing items and things.
 * This resource is registered with the Jersey servlet.
 * @author Laurent Garnier - Add YAML output for things
 * @author Laurent Garnier - Add new API for conversion between file format and JSON
@JaxrsName(FileFormatResource.PATH_FILE_FORMAT)
@Path(FileFormatResource.PATH_FILE_FORMAT)
@Tag(name = FileFormatResource.PATH_FILE_FORMAT)
public class FileFormatResource implements RESTResource {
    public static final String PATH_FILE_FORMAT = "file-format";
    private static final String DSL_THINGS_EXAMPLE = """
            Bridge binding:typeBridge:idBridge "Label bridge" @ "Location bridge" [stringParam="my value"] {
                Thing type id "Label thing" @ "Location thing" [booleanParam=true, decimalParam=2.5]
    private static final String YAML_THINGS_EXAMPLE = """
            version: 1
            things:
              binding:typeBridge:idBridge:
                isBridge: true
                label: Label bridge
                location: Location bridge
                config:
                  stringParam: my value
              binding:type:idBridge:id:
                bridge: binding:typeBridge:idBridge
                label: Label thing
                location: Location thing
                  booleanParam: true
                  decimalParam: 2.5
    private static final String DSL_ITEMS_EXAMPLE = """
            Group Group1 "Label"
            Group:Switch:OR(ON,OFF) Group2 "Label"
            Switch MyItem "Label" <icon> (Group1, Group2) [Tag1, Tag2] { channel="binding:type:id:channelid", namespace="my value" [param="my param value"] }
    private static final String YAML_ITEMS_EXAMPLE = """
            items:
              Group1:
                type: Group
                label: Label
              Group2:
                group:
                  type: Switch
                  function: Or
                  parameters:
                    - "ON"
                    - "OFF"
              MyItem:
                icon: icon
                groups:
                  - Group1
                  - Group2
                tags:
                  - Tag1
                  - Tag2
                channel: binding:type:id:channelid
                  namespace:
                    value: my value
                      param: my param value
    private static final String YAML_ITEMS_AND_THINGS_EXAMPLE = """
                channel: binding:type:idBridge:id:channelid
    private static final String GEN_ID_PATTERN = "gen_file_format_%d";
    private final Logger logger = LoggerFactory.getLogger(FileFormatResource.class);
    private final ItemBuilderFactory itemBuilderFactory;
    private final Map<String, ItemSerializer> itemSerializers = new ConcurrentHashMap<>();
    private final Map<String, ItemParser> itemParsers = new ConcurrentHashMap<>();
    private final Map<String, ThingSerializer> thingSerializers = new ConcurrentHashMap<>();
    private final Map<String, ThingParser> thingParsers = new ConcurrentHashMap<>();
    private int counter;
    public FileFormatResource(//
            final @Reference ItemBuilderFactory itemBuilderFactory, //
            final @Reference ItemRegistry itemRegistry, //
            final @Reference ThingRegistry thingRegistry, //
            final @Reference Inbox inbox, //
            final @Reference ThingTypeRegistry thingTypeRegistry, //
            final @Reference ChannelTypeRegistry channelTypeRegistry, //
            final @Reference ConfigDescriptionRegistry configDescRegistry) {
        this.itemBuilderFactory = itemBuilderFactory;
    protected void addItemSerializer(ItemSerializer itemSerializer) {
        itemSerializers.put(itemSerializer.getGeneratedFormat(), itemSerializer);
    protected void removeItemSerializer(ItemSerializer itemSerializer) {
        itemSerializers.remove(itemSerializer.getGeneratedFormat());
    protected void addItemParser(ItemParser itemParser) {
        itemParsers.put(itemParser.getParserFormat(), itemParser);
    protected void removeItemParser(ItemParser itemParser) {
        itemParsers.remove(itemParser.getParserFormat());
    protected void addThingSerializer(ThingSerializer thingSerializer) {
        thingSerializers.put(thingSerializer.getGeneratedFormat(), thingSerializer);
    protected void removeThingSerializer(ThingSerializer thingSerializer) {
        thingSerializers.remove(thingSerializer.getGeneratedFormat());
    protected void addThingParser(ThingParser thingParser) {
        thingParsers.put(thingParser.getParserFormat(), thingParser);
    protected void removeThingParser(ThingParser thingParser) {
        thingParsers.remove(thingParser.getParserFormat());
    @Path("/items")
    @Produces({ "text/vnd.openhab.dsl.item", "application/yaml" })
    @Operation(operationId = "createFileFormatForItems", summary = "Create file format for a list of items in registry.", security = {
            @SecurityRequirement(name = "oauth2", scopes = { "admin" }) }, responses = {
                    @ApiResponse(responseCode = "200", description = "OK", content = {
                            @Content(mediaType = "text/vnd.openhab.dsl.item", schema = @Schema(example = DSL_ITEMS_EXAMPLE)),
                            @Content(mediaType = "application/yaml", schema = @Schema(example = YAML_ITEMS_EXAMPLE)) }),
                    @ApiResponse(responseCode = "404", description = "One or more items not found in registry."),
                    @ApiResponse(responseCode = "415", description = "Unsupported media type.") })
    public Response createFileFormatForItems(final @Context HttpHeaders httpHeaders,
            @DefaultValue("true") @QueryParam("hideDefaultParameters") @Parameter(description = "hide the configuration parameters having the default value") boolean hideDefaultParameters,
            @Parameter(description = "Array of item names. If empty or omitted, return all Items.") @Nullable List<String> itemNames) {
        String acceptHeader = httpHeaders.getHeaderString(HttpHeaders.ACCEPT);
        logger.debug("createFileFormatForItems: mediaType = {}, itemNames = {}", acceptHeader, itemNames);
        ItemSerializer serializer = getItemSerializer(acceptHeader);
        if (serializer == null) {
            return Response.status(Response.Status.UNSUPPORTED_MEDIA_TYPE)
                    .entity("Unsupported media type '" + acceptHeader + "'!").build();
        List<Item> items;
        if (itemNames == null || itemNames.isEmpty()) {
            items = getAllItemsSorted();
            items = new ArrayList<>();
            for (String itemname : itemNames) {
                Item item = itemRegistry.get(itemname);
                    return Response.status(Response.Status.NOT_FOUND)
                            .entity("Item with name '" + itemname + "' not found in the items registry!").build();
                items.add(item);
        String genId = newIdForSerialization();
        Map<String, String> stateFormatters = new HashMap<>();
        items.forEach(item -> {
            StateDescription stateDescr = item.getStateDescription();
            String format = stateDescr == null ? null : stateDescr.getPattern();
            if (format != null) {
                stateFormatters.put(item.getName(), format);
        serializer.setItemsToBeSerialized(genId, items, getMetadata(items), stateFormatters, hideDefaultParameters);
        serializer.generateFormat(genId, outputStream);
        return Response.ok(new String(outputStream.toByteArray())).build();
    @Path("/things")
    @Produces({ "text/vnd.openhab.dsl.thing", "application/yaml" })
    @Operation(operationId = "createFileFormatForThings", summary = "Create file format for a list of things in things or discovery registry.", security = {
                            @Content(mediaType = "text/vnd.openhab.dsl.thing", schema = @Schema(example = DSL_THINGS_EXAMPLE)),
                            @Content(mediaType = "application/yaml", schema = @Schema(example = YAML_THINGS_EXAMPLE)) }),
                    @ApiResponse(responseCode = "404", description = "One or more things not found in registry."),
    public Response createFileFormatForThings(final @Context HttpHeaders httpHeaders,
            @Parameter(description = "Array of Thing UIDs. If empty or omitted, return all Things from the Registry.") @Nullable List<String> thingUIDs) {
        logger.debug("createFileFormatForThings: mediaType = {}, thingUIDs = {}", acceptHeader, thingUIDs);
        ThingSerializer serializer = getThingSerializer(acceptHeader);
        List<Thing> things;
        if (thingUIDs == null || thingUIDs.isEmpty()) {
            things = getAllThingsSorted();
                things = getThingsOrDiscoveryResult(thingUIDs);
                return Response.status(Response.Status.NOT_FOUND).entity(e.getMessage()).build();
        serializer.setThingsToBeSerialized(genId, things, true, hideDefaultParameters);
    @Path("/create")
    @Consumes({ MediaType.APPLICATION_JSON })
    @Produces({ "text/vnd.openhab.dsl.thing", "text/vnd.openhab.dsl.item", "application/yaml" })
    @Operation(operationId = "create", summary = "Create file format.", security = {
                            @Content(mediaType = "application/yaml", schema = @Schema(example = YAML_ITEMS_AND_THINGS_EXAMPLE)) }),
                    @ApiResponse(responseCode = "400", description = "Invalid JSON data."),
    public Response create(final @Context HttpHeaders httpHeaders,
            @DefaultValue("false") @QueryParam("hideDefaultParameters") @Parameter(description = "hide the configuration parameters having the default value") boolean hideDefaultParameters,
            @DefaultValue("false") @QueryParam("hideDefaultChannels") @Parameter(description = "hide the non extensible channels having a default configuration") boolean hideDefaultChannels,
            @DefaultValue("false") @QueryParam("hideChannelLinksAndMetadata") @Parameter(description = "hide the channel links and metadata for items") boolean hideChannelLinksAndMetadata,
            @RequestBody(description = "JSON data", required = true, content = @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(implementation = FileFormatDTO.class))) FileFormatDTO data) {
        logger.debug("create: mediaType = {}", acceptHeader);
        List<Thing> things = new ArrayList<>();
        List<Item> items = new ArrayList<>();
        List<Metadata> metadata = new ArrayList<>();
        List<String> errors = new ArrayList<>();
        if (!convertFromFileFormatDTO(data, things, items, metadata, stateFormatters, errors)) {
            return Response.status(Response.Status.BAD_REQUEST).entity(String.join("\n", errors)).build();
        ThingSerializer thingSerializer = getThingSerializer(acceptHeader);
        ItemSerializer itemSerializer = getItemSerializer(acceptHeader);
        switch (acceptHeader) {
            case "text/vnd.openhab.dsl.thing":
                if (thingSerializer == null) {
                } else if (things.isEmpty()) {
                    return Response.status(Response.Status.BAD_REQUEST).entity("No thing loaded from input").build();
                thingSerializer.setThingsToBeSerialized(genId, things, hideDefaultChannels, hideDefaultParameters);
                thingSerializer.generateFormat(genId, outputStream);
            case "text/vnd.openhab.dsl.item":
                if (itemSerializer == null) {
                } else if (items.isEmpty()) {
                    return Response.status(Response.Status.BAD_REQUEST).entity("No item loaded from input").build();
                itemSerializer.setItemsToBeSerialized(genId, items, hideChannelLinksAndMetadata ? List.of() : metadata,
                        stateFormatters, hideDefaultParameters);
                itemSerializer.generateFormat(genId, outputStream);
            case "application/yaml":
                if (thingSerializer != null) {
                if (itemSerializer != null) {
                    itemSerializer.setItemsToBeSerialized(genId, items,
                            hideChannelLinksAndMetadata ? List.of() : metadata, stateFormatters, hideDefaultParameters);
                } else if (itemSerializer != null) {
    @Path("/parse")
    @Consumes({ "text/vnd.openhab.dsl.thing", "text/vnd.openhab.dsl.item", "application/yaml" })
    @Operation(operationId = "parse", summary = "Parse file format.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(implementation = ExtendedFileFormatDTO.class))),
                    @ApiResponse(responseCode = "400", description = "Invalid input data."),
                    @ApiResponse(responseCode = "415", description = "Unsupported content type.") })
    public Response parse(final @Context HttpHeaders httpHeaders,
            @RequestBody(description = "file format syntax", required = true, content = {
                    @Content(mediaType = "application/yaml", schema = @Schema(example = YAML_ITEMS_AND_THINGS_EXAMPLE)) }) String input) {
        String contentTypeHeader = httpHeaders.getHeaderString(HttpHeaders.CONTENT_TYPE);
        logger.debug("parse: contentType = {}", contentTypeHeader);
        // First parse the input
        Collection<Thing> things = List.of();
        Collection<Item> items = List.of();
        Collection<Metadata> metadata = List.of();
        Collection<ItemChannelLink> channelLinks = List.of();
        Map<String, String> stateFormatters = Map.of();
        List<String> warnings = new ArrayList<>();
        ThingParser thingParser = getThingParser(contentTypeHeader);
        ItemParser itemParser = getItemParser(contentTypeHeader);
        String modelName2 = null;
        switch (contentTypeHeader) {
                if (thingParser == null) {
                            .entity("Unsupported content type '" + contentTypeHeader + "'!").build();
                modelName = thingParser.startParsingFormat(input, errors, warnings);
                if (modelName == null) {
                things = thingParser.getParsedObjects(modelName);
                if (things.isEmpty()) {
                    thingParser.finishParsingFormat(modelName);
                if (itemParser == null) {
                modelName2 = itemParser.startParsingFormat(input, errors, warnings);
                if (modelName2 == null) {
                items = itemParser.getParsedObjects(modelName2);
                if (items.isEmpty()) {
                    itemParser.finishParsingFormat(modelName2);
                metadata = itemParser.getParsedMetadata(modelName2);
                stateFormatters = itemParser.getParsedStateFormatters(modelName2);
                // We need to go through the thing parser to retrieve the items channel links
                // But there is no need to parse again the input
                if (thingParser != null) {
                    channelLinks = thingParser.getParsedChannelLinks(modelName2);
                    channelLinks = thingParser.getParsedChannelLinks(modelName);
                if (itemParser != null) {
                    // Avoid parsing the input a second time
                            return Response.status(Response.Status.BAD_REQUEST).entity(String.join("\n", errors))
                    String modelNameToUse = modelName != null ? modelName : Objects.requireNonNull(modelName2);
                    items = itemParser.getParsedObjects(modelNameToUse);
                    metadata = itemParser.getParsedMetadata(modelNameToUse);
                    stateFormatters = itemParser.getParsedStateFormatters(modelNameToUse);
        ExtendedFileFormatDTO result = convertToFileFormatDTO(things, items, metadata, stateFormatters, channelLinks,
                warnings);
        if (modelName != null && thingParser != null) {
        if (modelName2 != null && itemParser != null) {
    private String newIdForSerialization() {
        return GEN_ID_PATTERN.formatted(++counter);
     * Get all the metadata for a list of items including channel links mapped to metadata in the namespace "channel"
    private Collection<Metadata> getMetadata(Collection<Item> items) {
            String itemName = item.getName();
            metadataRegistry.getAll().stream().filter(md -> md.getUID().getItemName().equals(itemName)).forEach(md -> {
                metadata.add(md);
            itemChannelLinkRegistry.getLinks(itemName).forEach(link -> {
                MetadataKey key = new MetadataKey("channel", itemName);
                Metadata md = new Metadata(key, link.getLinkedUID().getAsString(),
                        link.getConfiguration().getProperties());
     * Get all items from registry sorted in such a way:
     * - group items are before non group items
     * - group items are sorted to have as much as possible ancestors before their children
     * - items not linked to a channel are before items linked to a channel
     * - items linked to a channel are grouped by thing UID
     * - items linked to the same thing UID are sorted by item name
    private List<Item> getAllItemsSorted() {
        Collection<Item> items = itemRegistry.getAll();
        List<Item> groups = items.stream().filter(item -> item instanceof GroupItem).sorted((item1, item2) -> {
            return item1.getName().compareTo(item2.getName());
        List<Item> topGroups = groups.stream().filter(group -> group.getGroupNames().isEmpty())
                .sorted((group1, group2) -> {
                    return group1.getName().compareTo(group2.getName());
        List<Item> groupTree = new ArrayList<>();
        for (Item group : topGroups) {
            fillGroupTree(groupTree, group);
        if (groupTree.size() != groups.size()) {
            logger.warn("Something went wrong when sorting groups; failback to a sort by name.");
            groupTree = groups;
        List<Item> nonGroups = items.stream().filter(item -> !(item instanceof GroupItem)).sorted((item1, item2) -> {
            Set<ItemChannelLink> channelLinks1 = itemChannelLinkRegistry.getLinks(item1.getName());
            String thingUID1 = channelLinks1.isEmpty() ? null
                    : channelLinks1.iterator().next().getLinkedUID().getThingUID().getAsString();
            Set<ItemChannelLink> channelLinks2 = itemChannelLinkRegistry.getLinks(item2.getName());
            String thingUID2 = channelLinks2.isEmpty() ? null
                    : channelLinks2.iterator().next().getLinkedUID().getThingUID().getAsString();
            if (thingUID1 == null && thingUID2 != null) {
            } else if (thingUID1 != null && thingUID2 == null) {
            } else if (thingUID1 != null && thingUID2 != null && !thingUID1.equals(thingUID2)) {
                return thingUID1.compareTo(thingUID2);
        return Stream.of(groupTree, nonGroups).flatMap(List::stream).toList();
    private void fillGroupTree(List<Item> groups, Item item) {
        if (item instanceof GroupItem group && !groups.contains(group)) {
            groups.add(group);
            List<Item> members = group.getMembers().stream().sorted((member1, member2) -> {
                return member1.getName().compareTo(member2.getName());
            for (Item member : members) {
                fillGroupTree(groups, member);
     * Get all things from registry sorted in such a way:
     * - things are grouped by binding, sorted by natural order of binding name
     * - all things of a binding are sorted to follow the tree, that is bridge thing is before its sub-things
     * - all things of a binding at a certain tree depth are sorted by thing UID
    private List<Thing> getAllThingsSorted() {
        Collection<Thing> things = thingRegistry.getAll();
        List<Thing> thingTree = new ArrayList<>();
        Set<String> bindings = things.stream().map(thing -> thing.getUID().getBindingId()).collect(Collectors.toSet());
        for (String binding : bindings.stream().sorted().toList()) {
            List<Thing> topThings = things.stream()
                    .filter(thing -> thing.getUID().getBindingId().equals(binding) && thing.getBridgeUID() == null)
                    .sorted((thing1, thing2) -> {
                        return thing1.getUID().getAsString().compareTo(thing2.getUID().getAsString());
            for (Thing thing : topThings) {
                fillThingTree(thingTree, thing);
        return thingTree;
    private void fillThingTree(List<Thing> things, Thing thing) {
        if (!things.contains(thing)) {
            things.add(thing);
            if (thing instanceof Bridge bridge) {
                List<Thing> subThings = bridge.getThings().stream().sorted((thing1, thing2) -> {
                for (Thing subThing : subThings) {
                    fillThingTree(things, subThing);
     * Create a thing from a discovery result without inserting it in the thing registry
    private Thing simulateThing(DiscoveryResult result, ThingType thingType) {
        Map<String, Object> configParams = new HashMap<>();
        List<ConfigDescriptionParameter> configDescriptionParameters = List.of();
                configDescriptionParameters = desc.getParameters();
        for (ConfigDescriptionParameter param : configDescriptionParameters) {
            Object value = result.getProperties().get(param.getName());
            Object normalizedValue = value != null ? ConfigUtil.normalizeType(value, param) : null;
            if (normalizedValue != null) {
                configParams.put(param.getName(), normalizedValue);
        Configuration config = new Configuration(configParams);
        return ThingFactory.createThing(thingType, result.getThingUID(), config, result.getBridgeUID(),
                configDescRegistry);
    private @Nullable ItemSerializer getItemSerializer(String mediaType) {
        return switch (mediaType) {
            case "text/vnd.openhab.dsl.item" -> itemSerializers.get("DSL");
            case "application/yaml" -> itemSerializers.get("YAML");
    private @Nullable ThingSerializer getThingSerializer(String mediaType) {
            case "text/vnd.openhab.dsl.thing" -> thingSerializers.get("DSL");
            case "application/yaml" -> thingSerializers.get("YAML");
    private @Nullable ItemParser getItemParser(String contentType) {
        return switch (contentType) {
            case "text/vnd.openhab.dsl.item" -> itemParsers.get("DSL");
            case "application/yaml" -> itemParsers.get("YAML");
    private @Nullable ThingParser getThingParser(String contentType) {
            case "text/vnd.openhab.dsl.thing" -> thingParsers.get("DSL");
            case "text/vnd.openhab.dsl.item" -> thingParsers.get("DSL");
            case "application/yaml" -> thingParsers.get("YAML");
    private List<Thing> getThingsOrDiscoveryResult(List<String> thingUIDs) {
        return thingUIDs.stream().distinct().map(uid -> {
            ThingUID thingUID = new ThingUID(uid);
            DiscoveryResult discoveryResult = inbox.stream().filter(forThingUID(thingUID)).findFirst()
                    .orElseThrow(() -> new IllegalArgumentException(
                            "Thing with UID '" + uid + "' not found in the things or discovery registry!"));
            ThingType thingType = thingTypeRegistry.getThingType(thingTypeUID);
                throw new IllegalArgumentException("Thing type with UID '" + thingTypeUID + "' does not exist!");
            return simulateThing(discoveryResult, thingType);
    private boolean convertFromFileFormatDTO(FileFormatDTO data, List<Thing> things, List<Item> items,
            List<Metadata> metadata, Map<String, String> stateFormatters, List<String> errors) {
        boolean ok = true;
        if (data.things != null) {
            for (ThingDTO thingData : data.things) {
                ThingUID thingUID = thingData.UID == null ? null : new ThingUID(thingData.UID);
                ThingTypeUID thingTypeUID = new ThingTypeUID(thingData.thingTypeUID);
                ThingUID bridgeUID = null;
                if (thingData.bridgeUID != null) {
                    bridgeUID = new ThingUID(thingData.bridgeUID);
                // turn the ThingDTO's configuration into a Configuration
                Configuration configuration = new Configuration(
                        normalizeConfiguration(thingData.configuration, thingTypeUID, thingUID));
                    normalizeChannels(thingData, thingUID);
                Thing thing = thingRegistry.createThingOfType(thingTypeUID, thingUID, bridgeUID, thingData.label,
                        configuration);
                    if (thingData.properties != null) {
                        for (Map.Entry<String, String> entry : thingData.properties.entrySet()) {
                            thing.setProperty(entry.getKey(), entry.getValue());
                    if (thingData.location != null) {
                        thing.setLocation(thingData.location);
                    if (thingData.channels != null) {
                        // The provided channels replace the channels provided by the thing type.
                        ThingDTO thingChannels = new ThingDTO();
                        thingChannels.channels = thingData.channels;
                        thing = ThingHelper.merge(thing, thingChannels);
                } else if (thingUID != null) {
                    // if there wasn't any ThingFactory capable of creating the thing,
                    // we create the Thing exactly the way we received it, i.e. we
                    // cannot take its thing type into account for automatically
                    // populating channels and properties.
                    thing = ThingDTOMapper.map(thingData,
                            thingTypeRegistry.getThingType(thingTypeUID) instanceof BridgeType);
                    errors.add("A thing UID must be provided, since no binding can create the thing!");
        if (data.items != null) {
            for (FileFormatItemDTO itemData : data.items) {
                String name = itemData.name;
                    errors.add("Missing item name in items data!");
                Item item;
                    item = FileFormatItemDTOMapper.map(itemData, itemBuilderFactory);
                        errors.add("Invalid item type in items data!");
                    errors.add("Invalid item name in items data!");
                metadata.addAll(FileFormatItemDTOMapper.mapMetadata(itemData));
                if (itemData.format != null) {
                    stateFormatters.put(name, itemData.format);
    private ExtendedFileFormatDTO convertToFileFormatDTO(Collection<Thing> things, Collection<Item> items,
            Collection<Metadata> metadata, Map<String, String> stateFormatters,
            Collection<ItemChannelLink> channelLinks, List<String> warnings) {
        ExtendedFileFormatDTO dto = new ExtendedFileFormatDTO();
        dto.warnings = warnings.isEmpty() ? null : warnings;
        if (!things.isEmpty()) {
            dto.things = new ArrayList<>();
            things.forEach(thing -> {
                // Normalize thing configuration
                Map<String, @Nullable Object> normalizedThingConfig = normalizeConfiguration(
                        thing.getConfiguration().getProperties(), thing.getThingTypeUID(), thing.getUID());
                if (normalizedThingConfig != null) {
                    thing.getConfiguration().keySet().forEach(paramName -> {
                        Object normalizedValue = normalizedThingConfig.get(paramName);
                            thing.getConfiguration().put(paramName, normalizedValue);
                // Normalize channel configuration
                thing.getChannels().forEach(channel -> {
                    ChannelTypeUID channelTypeUID = channel.getChannelTypeUID();
                    if (channelTypeUID != null) {
                        Map<String, @Nullable Object> normalizedChannelConfig = normalizeConfiguration(
                                channel.getConfiguration().getProperties(), channelTypeUID, channel.getUID());
                        if (normalizedChannelConfig != null) {
                            channel.getConfiguration().keySet().forEach(paramName -> {
                                Object normalizedValue = normalizedChannelConfig.get(paramName);
                                    channel.getConfiguration().put(paramName, normalizedValue);
                dto.things.add(ThingDTOMapper.map(thing));
            dto.items = new ArrayList<>();
                dto.items.add(
                        FileFormatItemDTOMapper.map(item, metadata, stateFormatters.get(item.getName()), channelLinks));
            @Nullable Map<String, @Nullable Object> properties, ThingTypeUID thingTypeUID,
            @Nullable ThingUID thingUID) {
        List<ConfigDescription> configDescriptions = new ArrayList<>(2);
            ConfigDescription typeConfigDesc = configDescRegistry.getConfigDescription(descURI);
            if (typeConfigDesc != null) {
                configDescriptions.add(typeConfigDesc);
            ConfigDescription thingConfigDesc = configDescRegistry
                    .getConfigDescription(getConfigDescriptionURI(thingUID));
            if (thingConfigDesc != null) {
                configDescriptions.add(thingConfigDesc);
        return ConfigUtil.normalizeTypes(properties, configDescriptions);
    private @Nullable Map<String, @Nullable Object> normalizeConfiguration(Map<String, @Nullable Object> properties,
            ChannelTypeUID channelTypeUID, ChannelUID channelUID) {
        ChannelType channelType = channelTypeRegistry.getChannelType(channelTypeUID);
        URI descURI = channelType.getConfigDescriptionURI();
        if (getConfigDescriptionURI(channelUID) != null) {
            ConfigDescription channelConfigDesc = configDescRegistry
                    .getConfigDescription(getConfigDescriptionURI(channelUID));
            if (channelConfigDesc != null) {
                configDescriptions.add(channelConfigDesc);
    private void normalizeChannels(ThingDTO thingBean, ThingUID thingUID) {
        if (thingBean.channels != null) {
            for (ChannelDTO channelBean : thingBean.channels) {
                if (channelBean.channelTypeUID != null) {
                    channelBean.configuration = normalizeConfiguration(channelBean.configuration,
                            new ChannelTypeUID(channelBean.channelTypeUID), new ChannelUID(thingUID, channelBean.id));
    private URI getConfigDescriptionURI(ThingUID thingUID) {
        String uriString = "thing:" + thingUID;
            return new URI(uriString);
            throw new BadRequestException("Invalid URI syntax: " + uriString);
    private URI getConfigDescriptionURI(ChannelUID channelUID) {
        String uriString = "channel:" + channelUID;
