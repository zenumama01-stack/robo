package org.openhab.core.io.rest.core.internal.item;
import org.openhab.core.io.rest.core.item.EnrichedGroupItemDTO;
import org.openhab.core.io.rest.core.item.EnrichedItemDTO;
import org.openhab.core.io.rest.core.item.EnrichedItemDTOMapper;
import org.openhab.core.library.items.RollershutterItem;
import org.openhab.core.semantics.ItemSemanticsProblem;
import org.openhab.core.semantics.SemanticTagRegistry;
import org.openhab.core.semantics.SemanticsPredicates;
import org.openhab.core.semantics.SemanticsService;
 * This class acts as a REST resource for items and provides different methods to interact with them, like retrieving
 * lists of items, sending commands to them or checking a single status.
 * The typical content types are plain text for status values and XML or JSON(P) for more complex data structures
 * @author Dennis Nobel - Added methods for item management
 * @author Andre Fuechsel - Added tag support
 * @author Chris Jackson - Added method to write complete item bean
 * @author Jörg Plewe - refactoring, error handling
 * @author Stefan Triller - Added bulk item add method
 * @author Mark Herwege - Added option to retrieve item groups with item REST call
@JaxrsName(ItemResource.PATH_ITEMS)
@Path(ItemResource.PATH_ITEMS)
@Tag(name = ItemResource.PATH_ITEMS)
public class ItemResource implements RESTResource {
    private static final String REST_SOURCE = "org.openhab.core.io.rest";
    public static final String PATH_ITEMS = "items";
     * Replaces part of the URI builder by forwarded headers.
     * @param uriBuilder the URI builder
     * @param httpHeaders the HTTP headers
    private static void respectForwarded(final UriBuilder uriBuilder, final @Context HttpHeaders httpHeaders) {
        Optional.ofNullable(httpHeaders.getHeaderString("X-Forwarded-Host")).ifPresent(host -> {
            final int pos1 = host.indexOf("[");
            final int pos2 = host.indexOf("]");
            final String hostWithIpv6 = (pos1 >= 0 && pos2 > pos1) ? host.substring(pos1, pos2 + 1) : null;
            final String[] parts = hostWithIpv6 == null ? host.split(":") : host.substring(pos2 + 1).split(":");
            uriBuilder.host(hostWithIpv6 != null ? hostWithIpv6 : parts[0]);
            if (parts.length > 1) {
                uriBuilder.port(Integer.parseInt(parts[1]));
        Optional.ofNullable(httpHeaders.getHeaderString("X-Forwarded-Proto")).ifPresent(uriBuilder::scheme);
    private final Logger logger = LoggerFactory.getLogger(ItemResource.class);
    private final ManagedMetadataProvider managedMetadataProvider;
    private final MetadataSelectorMatcher metadataSelectorMatcher;
    private final SemanticTagRegistry semanticTagRegistry;
    private final SemanticsService semanticsService;
    private final RegistryChangedRunnableListener<Item> resetLastModifiedItemChangeListener = new RegistryChangedRunnableListener<>(
    private final RegistryChangedRunnableListener<Metadata> resetLastModifiedMetadataChangeListener = new RegistryChangedRunnableListener<>(
    public ItemResource(//
            final @Reference EventPublisher eventPublisher, //
            final @Reference ManagedItemProvider managedItemProvider,
            final @Reference ManagedMetadataProvider managedMetadataProvider,
            final @Reference MetadataSelectorMatcher metadataSelectorMatcher,
            final @Reference SemanticTagRegistry semanticTagRegistry,
            final @Reference SemanticsService semanticsService, final @Reference TimeZoneProvider timeZoneProvider) {
        this.managedMetadataProvider = managedMetadataProvider;
        this.metadataSelectorMatcher = metadataSelectorMatcher;
        this.semanticTagRegistry = semanticTagRegistry;
        this.semanticsService = semanticsService;
        this.itemRegistry.addRegistryChangeListener(resetLastModifiedItemChangeListener);
        this.metadataRegistry.addRegistryChangeListener(resetLastModifiedMetadataChangeListener);
        this.itemRegistry.removeRegistryChangeListener(resetLastModifiedItemChangeListener);
        this.metadataRegistry.removeRegistryChangeListener(resetLastModifiedMetadataChangeListener);
    private UriBuilder uriBuilder(final UriInfo uriInfo, final HttpHeaders httpHeaders) {
        final UriBuilder uriBuilder = uriInfo.getBaseUriBuilder().path(PATH_ITEMS).path("{itemName}");
        respectForwarded(uriBuilder, httpHeaders);
        return uriBuilder;
    @Operation(operationId = "getItems", summary = "Get all available items.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(oneOf = {
                    EnrichedItemDTO.class, EnrichedGroupItemDTO.class })))) })
    public Response getItems(final @Context UriInfo uriInfo, final @Context HttpHeaders httpHeaders,
            @Context Request request,
            @QueryParam("type") @Parameter(description = "item type filter") @Nullable String type,
            @QueryParam("tags") @Parameter(description = "item tag filter") @Nullable String tags,
            @DefaultValue(".*") @QueryParam("metadata") @Parameter(description = "metadata selector - a comma separated list or a regular expression (returns all if no value given)") @Nullable String namespaceSelector,
            @DefaultValue("false") @QueryParam("recursive") @Parameter(description = "get member items recursively") boolean recursive,
            @DefaultValue("false") @QueryParam("parents") @Parameter(description = "get parent group items recursively") boolean parents,
            @QueryParam("fields") @Parameter(description = "limit output to the given fields (comma separated)") @Nullable String fields,
            @DefaultValue("false") @QueryParam("staticDataOnly") @Parameter(description = "provides a cacheable list of values not expected to change regularly and checks the If-Modified-Since header, all other parameters are ignored except \"metadata\"") boolean staticDataOnly) {
        final ZoneId zoneId = timeZoneProvider.getTimeZone();
        final Set<String> namespaces = splitAndFilterNamespaces(namespaceSelector, locale);
        final UriBuilder uriBuilder = uriBuilder(uriInfo, httpHeaders);
            Stream<EnrichedItemDTO> itemStream = getItems(type, tags).stream() //
                    .map(item -> EnrichedItemDTOMapper.map(item, false, null, uriBuilder, locale, zoneId)) //
                    .peek(dto -> addMetadata(dto, namespaces, null)) //
                    .peek(dto -> dto.editable = isEditable(dto));
            itemStream = dtoMapper.limitToFields(itemStream,
                    "name,label,type,groupType,function,category,editable,groupNames,link,tags,metadata,commandDescription,stateDescription");
            return Response.ok(new Stream2JSONInputStream(itemStream)).lastModified(lastModified)
                .map(item -> EnrichedItemDTOMapper.map(item, recursive, null, uriBuilder, locale, zoneId)) //
                .peek(dto -> {
                    if (parents) {
                        addParents(dto, uriInfo, httpHeaders, locale, zoneId);
                }).peek(dto -> addMetadata(dto, namespaces, null)) //
                .peek(dto -> dto.editable = isEditable(dto)) //
                    if (dto instanceof EnrichedGroupItemDTO enrichedGroupItemDTO) {
                        for (EnrichedItemDTO member : enrichedGroupItemDTO.members) {
                            member.editable = isEditable(member);
                    if (dto.parents != null) {
                        for (EnrichedItemDTO parent : dto.parents) {
                            parent.editable = isEditable(parent);
        itemStream = dtoMapper.limitToFields(itemStream, fields);
        return Response.ok(new Stream2JSONInputStream(itemStream)).build();
     * @param itemName name of the item
     * @return the namesspace of that item
    @Path("/{itemName: [a-zA-Z_0-9]+}/metadata/namespaces")
    @Operation(operationId = "getItemNamespaces", summary = "Gets the namespace of an item.", responses = {
            @ApiResponse(responseCode = "404", description = "Item not found") })
    public Response getItemNamespaces(@PathParam("itemName") @Parameter(description = "item name") String itemName,
        final Item item = getItem(itemName);
            final Collection<String> namespaces = metadataRegistry.getAllNamespaces(itemName);
            return Response.ok(new Stream2JSONInputStream(namespaces.stream())).build();
            return getItemNotFoundResponse(itemName);
    @Path("/{itemName: [a-zA-Z_0-9]+}")
    @Operation(operationId = "getItemByName", summary = "Gets a single item.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(oneOf = {
                    EnrichedItemDTO.class, EnrichedGroupItemDTO.class }))),
    public Response getItemByName(final @Context UriInfo uriInfo, final @Context HttpHeaders httpHeaders,
            @DefaultValue("true") @QueryParam("recursive") @Parameter(description = "get member items if the item is a group item") boolean recursive,
            @PathParam("itemName") @Parameter(description = "item name") String itemName) {
        // get item
        Item item = getItem(itemName);
        // if it exists
            EnrichedItemDTO dto = EnrichedItemDTOMapper.map(item, recursive, null, uriBuilder(uriInfo, httpHeaders),
                    locale, zoneId);
            addMetadata(dto, namespaces, null);
            dto.editable = isEditable(dto);
            return JSONResponse.createResponse(Status.OK, dto, null);
    private Set<String> splitAndFilterNamespaces(@Nullable String namespaceSelector, Locale locale) {
        return metadataSelectorMatcher.filterNamespaces(namespaceSelector, locale);
     * @param itemName item name to get the state from
     * @return the state of the item as mime-type text/plain
    @Path("/{itemName: [a-zA-Z_0-9]+}/state")
    @Operation(operationId = "getItemState", summary = "Gets the state of an item.", responses = {
    public Response getPlainItemState(@PathParam("itemName") @Parameter(description = "item name") String itemName) {
            // we cannot use JSONResponse.createResponse() bc. MediaType.TEXT_PLAIN
            // return JSONResponse.createResponse(Status.OK, item.getState().toString(), null);
            return Response.ok(item.getState().toFullString()).build();
     * @param itemName the item from which to get the binary state
     * @return the binary state of the item
            @ApiResponse(responseCode = "400", description = "Item state is not RawType"),
            @ApiResponse(responseCode = "404", description = "Item not found"),
            @ApiResponse(responseCode = "415", description = "MediaType not supported by item state") })
    public Response getBinaryItemState(@HeaderParam("Accept") @Nullable String mediaType,
        List<String> acceptedMediaTypes = Arrays.stream(Objects.requireNonNullElse(mediaType, "").split(","))
            State state = item.getState();
            if (state instanceof RawType type) {
                String mimeType = type.getMimeType();
                byte[] data = type.getBytes();
                if ((acceptedMediaTypes.contains("image/*") && mimeType.startsWith("image/"))
                        || acceptedMediaTypes.contains(mimeType)) {
                    return Response.ok(data).type(mimeType).build();
                } else if (acceptedMediaTypes.contains(MediaType.APPLICATION_OCTET_STREAM)) {
                    return Response.ok(data).type(MediaType.APPLICATION_OCTET_STREAM).build();
                    return Response.status(Status.UNSUPPORTED_MEDIA_TYPE).build();
    @Operation(operationId = "updateItemState", summary = "Updates the state of an item.", requestBody = @RequestBody(description = "Valid item state (e.g., ON, OFF) either as plain text or JSON", required = true, content = {
            @Content(mediaType = MediaType.TEXT_PLAIN, schema = @Schema(type = "string", example = "ON")),
            @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(type = "string", example = "{ \"value\": \"ON\", \"source\": null }")) }), responses = {
                    @ApiResponse(responseCode = "202", description = "Accepted"),
                    @ApiResponse(responseCode = "400", description = "State cannot be parsed") })
    public Response putItemStatePlain(
            @HeaderParam("X-OpenHAB-Source") @Parameter(description = "the source of the event; takes priority over the query parameter or JSON body if multiple are set") @Nullable String headerSource,
            @PathParam("itemName") @Parameter(description = "item name") String itemName,
            @Parameter(description = "valid item state (e.g. ON, OFF)", required = true) String value,
            @QueryParam("source") @Parameter(description = "the source of the event") @Nullable String querySource,
        String source = headerSource != null ? headerSource : querySource;
        return sendItemStateInternal(language, itemName, value, source, securityContext);
    public Response putItemStateJson(@HeaderParam(HttpHeaders.ACCEPT_LANGUAGE) @Nullable String language,
            @HeaderParam("X-OpenHAB-Source") @Nullable String headerSource,
            @QueryParam("source") @Nullable String querySource, @PathParam("itemName") String itemName,
            @Context SecurityContext securityContext, ValueContainer valueContainer) {
        String source;
        if (headerSource != null) {
            source = headerSource;
        } else if (valueContainer.source() != null) {
            source = valueContainer.source();
            source = querySource;
        return sendItemStateInternal(language, itemName, valueContainer.value(), source, securityContext);
    private Response sendItemStateInternal(@Nullable String language, String itemName, String value,
            @Nullable String source, SecurityContext securityContext) {
        String eventSource = buildSource(source, securityContext);
        // get Item
        // if Item exists
            // try to parse a State from the input
            State state = TypeParser.parseState(item.getAcceptedDataTypes(), value);
                // set State and report OK
                eventPublisher.post(ItemEventFactory.createStateEvent(itemName, state, eventSource));
                return getItemResponse(null, Status.ACCEPTED, null, locale, zoneId, null);
                // State could not be parsed
                return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "State could not be parsed: " + value);
            // Item does not exist
    @Operation(operationId = "sendItemCommand", summary = "Sends a command to an item.", requestBody = @RequestBody(description = "Valid item command (e.g., ON, OFF) either as plain text or JSON", required = true, content = {
            @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(type = "string", example = "{ \"value\": \"ON\", \"source\": \"org.openhab.ios\" }")) }), responses = {
                    @ApiResponse(responseCode = "400", description = "Command cannot be parsed") })
    public Response postItemCommandPlain(
            @HeaderParam("X-OpenHAB-Source") @Parameter(description = "the source of the command; takes priority over the query parameter or JSON body if multiple are set") @Nullable String headerSource,
            @Parameter(description = "valid item command (e.g. ON, OFF, UP, DOWN, REFRESH)", required = true) String value,
            @QueryParam("source") @Parameter(description = "the source of the command") @Nullable String querySource,
        return sendItemCommandInternal(itemName, value, source, securityContext);
    public Response postItemCommandJson(@HeaderParam("X-OpenHAB-Source") @Nullable String headerSource,
            @PathParam("itemName") String itemName, @QueryParam("source") @Nullable String querySource,
        return sendItemCommandInternal(itemName, valueContainer.value(), source, securityContext);
    private Response sendItemCommandInternal(String itemName, String value, @Nullable String source,
            SecurityContext securityContext) {
        Command command = null;
            if ("toggle".equalsIgnoreCase(value) && (item instanceof SwitchItem || item instanceof RollershutterItem)) {
                if (OnOffType.ON.equals(item.getStateAs(OnOffType.class))) {
                    command = OnOffType.OFF;
                if (OnOffType.OFF.equals(item.getStateAs(OnOffType.class))) {
                    command = OnOffType.ON;
                if (UpDownType.UP.equals(item.getStateAs(UpDownType.class))) {
                    command = UpDownType.DOWN;
                if (UpDownType.DOWN.equals(item.getStateAs(UpDownType.class))) {
                    command = UpDownType.UP;
                command = TypeParser.parseCommand(item.getAcceptedCommandTypes(), value);
                eventPublisher.post(ItemEventFactory.createCommandEvent(itemName, command, eventSource));
                ResponseBuilder resbuilder = Response.ok();
                resbuilder.type(MediaType.TEXT_PLAIN);
                return resbuilder.build();
    @Path("/{itemName: [a-zA-Z_0-9]+}/members/{memberItemName: [a-zA-Z_0-9]+}")
    @Operation(operationId = "addMemberToGroupItem", summary = "Adds a new member to a group item.", security = {
                    @ApiResponse(responseCode = "404", description = "Item or member item not found or item is not of type group item."),
                    @ApiResponse(responseCode = "405", description = "Member item is not editable.") })
    public Response addMember(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            @PathParam("memberItemName") @Parameter(description = "member item name") String memberItemName) {
            if (!(item instanceof GroupItem)) {
            GroupItem groupItem = (GroupItem) item;
            Item memberItem = itemRegistry.getItem(memberItemName);
            if (!(memberItem instanceof GenericItem)) {
            if (managedItemProvider.get(memberItemName) == null) {
                return Response.status(Status.METHOD_NOT_ALLOWED).build();
            GenericItem genericMemberItem = (GenericItem) memberItem;
            genericMemberItem.addGroupName(groupItem.getName());
            managedItemProvider.update(genericMemberItem);
    @Operation(operationId = "removeMemberFromGroupItem", summary = "Removes an existing member from a group item.", security = {
    public Response removeMember(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            genericMemberItem.removeGroupName(groupItem.getName());
    @Operation(operationId = "removeItemFromRegistry", summary = "Removes an item from the registry.", security = {
                    @ApiResponse(responseCode = "404", description = "Item not found or item is not editable.") })
    public Response removeItem(@PathParam("itemName") @Parameter(description = "item name") String itemName) {
        if (managedItemProvider.remove(itemName) == null) {
    @Path("/{itemName: [a-zA-Z_0-9]+}/tags/{tag}")
    @Operation(operationId = "addTagToItem", summary = "Adds a tag to an item.", security = {
                    @ApiResponse(responseCode = "404", description = "Item not found."),
                    @ApiResponse(responseCode = "405", description = "Item not editable.") })
    public Response addTag(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            @PathParam("tag") @Parameter(description = "tag") String tag) {
        if (managedItemProvider.get(itemName) == null) {
        ((GenericItem) item).addTag(tag);
        managedItemProvider.update(item);
    @Operation(operationId = "removeTagFromItem", summary = "Removes a tag from an item.", security = {
    public Response removeTag(@PathParam("itemName") @Parameter(description = "item name") String itemName,
        ((GenericItem) item).removeTag(tag);
    @Path("/{itemName: [a-zA-Z_0-9]+}/metadata/{namespace}")
    @Operation(operationId = "addMetadataToItem", summary = "Adds metadata to an item.", security = {
            @SecurityRequirement(name = "oauth2", scopes = { "admin" }) }, responses = { //
                    @ApiResponse(responseCode = "200", description = "OK"), //
                    @ApiResponse(responseCode = "201", description = "Created"), //
                    @ApiResponse(responseCode = "404", description = "Item not found."), //
                    @ApiResponse(responseCode = "405", description = "Metadata not editable."),
                    @ApiResponse(responseCode = "503", description = "Managed provider not available.") })
    public Response addMetadata(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            @PathParam("namespace") @Parameter(description = "namespace") String namespace,
            @Parameter(description = "metadata", required = true) MetadataDTO metadata) {
        String value = metadata.value;
            value = "";
        Metadata md = new Metadata(key, value, metadata.config);
                metadataRegistry.add(md);
                return Response.status(Status.CREATED).type(MediaType.TEXT_PLAIN).build();
                if (metadataRegistry.update(md) == null) {
                    // Exists, but not managed
            // Trying to add to a reserved namespace that is in an unmanaged provider
            return JSONResponse.createErrorResponse(Status.METHOD_NOT_ALLOWED, e.getMessage());
            // There is no managed provider available
            return Response.status(Status.SERVICE_UNAVAILABLE).build();
    @Path("/{itemName: [a-zA-Z_0-9]+}/metadata")
    @Operation(operationId = "removeAllMetadataFromItem", summary = "Removes all managed metadata from an item.", security = {
                    @ApiResponse(responseCode = "404", description = "Item not found.") })
    public Response removeAllMetadata(@PathParam("itemName") @Parameter(description = "item name") String itemName) {
        metadataRegistry.removeItemMetadata(itemName);
    @Operation(operationId = "removeMetadataFromItem", summary = "Removes metadata in a specific namespace from an item.", security = {
                    @ApiResponse(responseCode = "404", description = "Item or namespace not found."),
    public Response removeMetadata(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            @PathParam("namespace") @Parameter(description = "namespace") String namespace) {
                    // Exists, but not managed, and not removed in the mean time
            // Trying to remove from a reserved namespace that is in an unmanaged provider
    @Path("/metadata/purge")
    @Operation(operationId = "purgeDatabase", summary = "Remove unused/orphaned metadata.", security = {
    public Response purge() {
        Collection<String> itemNames = itemRegistry.stream().map(Item::getName)
                .collect(Collectors.toCollection(HashSet::new));
        metadataRegistry.getAll().stream().filter(md -> !itemNames.contains(md.getUID().getItemName())).forEach(md -> {
     * Create or Update an item by supplying an item bean.
     * @param itemName the item name
     * @param item the item bean.
     * @return Response configured to represent the Item in depending on the status
    @Operation(operationId = "addOrUpdateItemInRegistry", summary = "Adds a new item to the registry or updates the existing item.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedItemDTO.class))),
                    @ApiResponse(responseCode = "201", description = "Item created."),
                    @ApiResponse(responseCode = "400", description = "Payload invalid."),
                    @ApiResponse(responseCode = "404", description = "Item not found or name in path invalid."),
    public Response createOrUpdateItem(final @Context UriInfo uriInfo, final @Context HttpHeaders httpHeaders,
            @Parameter(description = "item data", required = true) @Nullable GroupItemDTO item) {
        // If we didn't get an item bean, then return!
        } else if (!itemName.equalsIgnoreCase((item.name))) {
                    "Received HTTP PUT request at '{}' with an item name '{}' that does not match the one in the url.",
                    uriInfo.getPath(), item.name);
            Item newItem = ItemDTOMapper.map(item, itemBuilderFactory);
            if (newItem == null) {
                logger.warn("Received HTTP PUT request at '{}' with an invalid item type '{}'.", uriInfo.getPath(),
                        item.type);
            // Save the item
            if (getItem(itemName) == null) {
                // item does not yet exist, create it
                managedItemProvider.add(newItem);
                return getItemResponse(uriBuilder(uriInfo, httpHeaders), Status.CREATED, itemRegistry.get(itemName),
                        locale, zoneId, null);
            } else if (managedItemProvider.get(itemName) != null) {
                // item already exists as a managed item, update it
                managedItemProvider.update(newItem);
                return getItemResponse(uriBuilder(uriInfo, httpHeaders), Status.OK, itemRegistry.get(itemName), locale,
                        zoneId, null);
                // Item exists but cannot be updated
                logger.warn("Cannot update existing item '{}', because is not managed.", itemName);
                return JSONResponse.createErrorResponse(Status.METHOD_NOT_ALLOWED,
                        "Cannot update non-managed Item " + itemName);
            logger.warn("Received HTTP PUT request at '{}' with an invalid item name '{}'.", uriInfo.getPath(),
                    item.name);
     * Create or Update a list of items by supplying a list of item beans.
     * @param items the list of item beans.
     * @return array of status information for each item bean
    @Operation(operationId = "addOrUpdateItemsInRegistry", summary = "Adds a list of items to the registry or updates the existing items.", security = {
                    @ApiResponse(responseCode = "400", description = "Payload is invalid.") })
    public Response createOrUpdateItems(
            @Parameter(description = "array of item data", required = true) GroupItemDTO @Nullable [] items) {
        // If we didn't get an item list bean, then return!
        if (items == null) {
        List<GroupItemDTO> wrongTypes = new ArrayList<>();
        List<Item> activeItems = new ArrayList<>();
        Map<String, Collection<String>> tagMap = new HashMap<>();
        for (GroupItemDTO item : items) {
                    wrongTypes.add(item);
                    tagMap.put(item.name, item.tags);
                    activeItems.add(newItem);
                logger.warn("Received HTTP PUT request with an invalid item name '{}'.", item.name);
        List<Item> createdItems = new ArrayList<>();
        List<Item> updatedItems = new ArrayList<>();
        List<Item> failedItems = new ArrayList<>();
        for (Item activeItem : activeItems) {
            String itemName = activeItem.getName();
                managedItemProvider.add(activeItem);
                createdItems.add(activeItem);
                managedItemProvider.update(activeItem);
                updatedItems.add(activeItem);
                logger.warn("Cannot update existing item '{}', because it is not managed.", itemName);
                failedItems.add(activeItem);
        // build response
        List<JsonObject> responseList = new ArrayList<>();
        for (GroupItemDTO item : wrongTypes) {
            responseList.add(buildStatusObject(item.name, "error",
                    "Received HTTP PUT request with an invalid item type '" + item.type + "'."));
        for (Item item : failedItems) {
            responseList.add(buildStatusObject(item.getName(), "error", "Cannot update non-managed item"));
        for (Item item : createdItems) {
            responseList.add(buildStatusObject(item.getName(), "created", null));
        for (Item item : updatedItems) {
            responseList.add(buildStatusObject(item.getName(), "updated", null));
        return JSONResponse.createResponse(Status.OK, responseList, null);
    @Path("/{itemName: \\w+}/semantic/{semanticClass: \\w+}")
    @Operation(operationId = "getSemanticItem", summary = "Gets the item which defines the requested semantics of an item.", responses = {
    public Response getSemanticItem(final @Context UriInfo uriInfo, final @Context HttpHeaders httpHeaders,
            @PathParam("semanticClass") @Parameter(description = "semantic class") String semanticClassName) {
        Class<? extends org.openhab.core.semantics.Tag> semanticClass = semanticTagRegistry
                .getTagClassById(semanticClassName);
        if (semanticClass == null) {
        Item foundItem = findParentByTag(getItem(itemName), SemanticsPredicates.isA(semanticClass));
        if (foundItem == null) {
        EnrichedItemDTO dto = EnrichedItemDTOMapper.map(foundItem, false, null, uriBuilder(uriInfo, httpHeaders),
    @Path("semantics/health")
    @Operation(operationId = "getSemanticsHealth", summary = "Gets configuration problems with item semantics.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ItemSemanticsProblem.class)))),
    public Response getSemanticsHealth(@Context HttpHeaders headers) {
        List<ItemSemanticsProblem> semanticsProblems = this.itemRegistry.stream().flatMap(item -> {
            return semanticsService.getItemSemanticsProblems(item).stream()
                    .map(p -> p.setEditable(isItemEditable(p.item())));
        return JSONResponse.createResponse(Status.OK, semanticsProblems, null);
    private JsonObject buildStatusObject(String itemName, String status, @Nullable String message) {
        JsonObject jo = new JsonObject();
        jo.addProperty("name", itemName);
        jo.addProperty("status", status);
        jo.addProperty("message", message);
        return jo;
    private @Nullable Item findParentByTag(@Nullable Item item, Predicate<Item> predicate) {
        if (predicate.test(item)) {
        // check parents
        return item.getGroupNames().stream().map(this::getItem).map(i -> findParentByTag(i, predicate))
                .filter(Objects::nonNull).findAny().orElse(null);
     * helper: Response to be sent to client if an item cannot be found
     * @param itemName item name that could not be found
     * @return Response configured for 'item not found'
    private static Response getItemNotFoundResponse(String itemName) {
        String message = "Item " + itemName + " does not exist!";
        return JSONResponse.createResponse(Status.NOT_FOUND, null, message);
     * Prepare a response representing the Item depending on the status.
     * @param status http status
     * @param item can be null
     * @param errormessage optional message in case of error
    private Response getItemResponse(final @Nullable UriBuilder uriBuilder, Status status, @Nullable Item item,
            Locale locale, ZoneId zoneId, @Nullable String errormessage) {
        Object entity = null != item ? EnrichedItemDTOMapper.map(item, true, null, uriBuilder, locale, zoneId) : null;
        return JSONResponse.createResponse(status, entity, errormessage);
     * convenience shortcut
     * @param itemName the name of the item to be retrieved
     * @return Item addressed by itemName
    private @Nullable Item getItem(String itemName) {
        return itemRegistry.get(itemName);
    private Collection<Item> getItems(@Nullable String type, @Nullable String tags) {
        if (tags == null) {
                items = itemRegistry.getItems();
                items = itemRegistry.getItemsOfType(type);
            String[] tagList = tags.split(",");
                items = itemRegistry.getItemsByTag(tagList);
                items = itemRegistry.getItemsByTagAndType(type, tagList);
    private void addMetadata(EnrichedItemDTO dto, Set<String> namespaces, @Nullable Predicate<Metadata> filter) {
        Map<String, Object> metadata = new HashMap<>();
        for (String namespace : namespaces) {
            MetadataKey key = new MetadataKey(namespace, dto.name);
            Metadata md = metadataRegistry.get(key);
            if (md != null && (filter == null || filter.test(md))) {
                MetadataDTO mdDto = new MetadataDTO();
                mdDto.value = md.getValue();
                mdDto.config = md.getConfiguration().isEmpty() ? null : md.getConfiguration();
                mdDto.editable = isEditable(key);
                metadata.put(namespace, mdDto);
        if (dto instanceof EnrichedGroupItemDTO tO) {
            for (EnrichedItemDTO member : tO.members) {
                addMetadata(member, namespaces, filter);
                addMetadata(parent, namespaces, filter);
        if (!metadata.isEmpty()) {
            // we only set it in the dto if there is really data available
            dto.metadata = metadata;
    private void addParents(EnrichedItemDTO dto, UriInfo uriInfo, HttpHeaders httpHeaders, Locale locale,
            ZoneId zoneId) {
        dto.parents = dto.groupNames.stream() //
                .map(groupName -> getItem(groupName)).filter(Objects::nonNull) //
                .map(parentItem -> EnrichedItemDTOMapper.map(parentItem, false, null, uriBuilder(uriInfo, httpHeaders),
                        locale, zoneId)) //
                .peek(parentEnrichedItemDto -> addParents(parentEnrichedItemDto, uriInfo, httpHeaders, locale, zoneId)) //
                .toArray(size -> new EnrichedItemDTO[size]);
    private boolean isEditable(EnrichedItemDTO item) {
        return isItemEditable(item.name);
    private boolean isItemEditable(String itemName) {
        return managedItemProvider.get(itemName) != null;
    private boolean isEditable(MetadataKey metadataKey) {
        return managedMetadataProvider.get(metadataKey) != null;
    private String buildSource(@Nullable String source, SecurityContext securityContext) {
        String username;
        Principal principal = securityContext.getUserPrincipal();
        if (principal != null) {
            username = principal.getName();
            username = null;
        return AbstractEvent.buildDelegatedSource(source, REST_SOURCE, username);
    private record ValueContainer(String value, @Nullable String source) {
