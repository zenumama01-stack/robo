package org.openhab.core.io.rest.core.internal.thing;
import org.openhab.core.config.core.status.ConfigStatusMessage;
import org.openhab.core.config.core.status.ConfigStatusService;
import org.openhab.core.io.rest.core.thing.EnrichedThingDTO;
import org.openhab.core.io.rest.core.thing.EnrichedThingDTOMapper;
import org.openhab.core.items.ItemFactory;
import org.openhab.core.thing.ThingManager;
import org.openhab.core.thing.binding.firmware.Firmware;
import org.openhab.core.thing.dto.ChannelDTOMapper;
import org.openhab.core.thing.firmware.FirmwareRegistry;
import org.openhab.core.thing.firmware.FirmwareStatusInfo;
import org.openhab.core.thing.firmware.FirmwareUpdateService;
import org.openhab.core.thing.firmware.dto.FirmwareDTO;
import org.openhab.core.thing.firmware.dto.FirmwareStatusDTO;
import org.openhab.core.thing.i18n.ThingStatusInfoI18nLocalizationService;
 * This class acts as a REST resource for things and is registered with the
 *         refactored create and update methods
 * @author Thomas Höfer - added validation of configuration and localization of thing status
 * @author Chris Jackson - added channel configuration updates,
 *         return empty set for config/status if no status available,
 *         add editable flag to thing responses
 * @author Dimitar Ivanov - replaced Firmware UID with thing UID and firmware version
 * @author Andrew Fiddian-Green - Added semanticEquipmentTag
@JaxrsName(ThingResource.PATH_THINGS)
@Path(ThingResource.PATH_THINGS)
@Tag(name = ThingResource.PATH_THINGS)
public class ThingResource implements RESTResource {
    private final Logger logger = LoggerFactory.getLogger(ThingResource.class);
    public static final String PATH_THINGS = "things";
    private final ConfigStatusService configStatusService;
    private final FirmwareRegistry firmwareRegistry;
    private final FirmwareUpdateService firmwareUpdateService;
    private final ThingManager thingManager;
    private final ThingStatusInfoI18nLocalizationService thingStatusInfoI18nLocalizationService;
    private final RegistryChangedRunnableListener<Thing> resetLastModifiedChangeListener = new RegistryChangedRunnableListener<>(
    public ThingResource( //
            final @Reference DTOMapper dtoMapper, final @Reference ChannelTypeRegistry channelTypeRegistry,
            final @Reference ConfigStatusService configStatusService,
            final @Reference ConfigDescriptionRegistry configDescRegistry,
            final @Reference FirmwareRegistry firmwareRegistry,
            final @Reference FirmwareUpdateService firmwareUpdateService,
            final @Reference ItemChannelLinkRegistry itemChannelLinkRegistry, //
            final @Reference ItemFactory itemFactory, //
            final @Reference ManagedItemChannelLinkProvider managedItemChannelLinkProvider,
            final @Reference ManagedThingProvider managedThingProvider, //
            final @Reference ThingManager thingManager, //
            final @Reference ThingRegistry thingRegistry,
            final @Reference ThingStatusInfoI18nLocalizationService thingStatusInfoI18nLocalizationService,
            final @Reference ThingTypeRegistry thingTypeRegistry) {
        this.configStatusService = configStatusService;
        this.firmwareRegistry = firmwareRegistry;
        this.firmwareUpdateService = firmwareUpdateService;
        this.managedThingProvider = managedThingProvider;
        this.thingManager = thingManager;
        this.thingStatusInfoI18nLocalizationService = thingStatusInfoI18nLocalizationService;
        this.thingRegistry.addRegistryChangeListener(resetLastModifiedChangeListener);
        this.thingRegistry.removeRegistryChangeListener(resetLastModifiedChangeListener);
     * create a new Thing
     * @param thingBean
     * @return Response holding the newly created Thing or error information
    @Operation(operationId = "createThingInRegistry", summary = "Creates a new thing and adds it to the registry.", security = {
                    @ApiResponse(responseCode = "201", description = "Created", content = @Content(schema = @Schema(implementation = EnrichedThingDTO.class))),
                    @ApiResponse(responseCode = "400", description = "A uid must be provided, if no binding can create a thing of this type."),
                    @ApiResponse(responseCode = "409", description = "A thing with the same uid already exists.") })
            @Parameter(description = "thing data", required = true) ThingDTO thingBean) {
        ThingUID thingUID = thingBean.UID == null ? null : new ThingUID(thingBean.UID);
        ThingTypeUID thingTypeUID = new ThingTypeUID(thingBean.thingTypeUID);
            // check if a thing with this UID already exists
                return getThingResponse(Status.CONFLICT, thing, locale,
                        "Thing " + thingUID.toString() + " already exists!");
        if (thingBean.bridgeUID != null) {
            bridgeUID = new ThingUID(thingBean.bridgeUID);
                normalizeConfiguration(thingBean.configuration, thingTypeUID, thingUID));
            normalizeChannels(thingBean, thingUID);
        Thing thing = thingRegistry.createThingOfType(thingTypeUID, thingUID, bridgeUID, thingBean.label,
            if (thingBean.properties != null) {
                for (Entry<String, String> entry : thingBean.properties.entrySet()) {
                List<Channel> channels = new ArrayList<>();
                for (ChannelDTO channelDTO : thingBean.channels) {
                    channels.add(ChannelDTOMapper.map(channelDTO));
                ThingHelper.addChannelsToThing(thing, channels);
            if (thingBean.location != null) {
                thing.setLocation(thingBean.location);
            thing = ThingDTOMapper.map(thingBean, thingTypeRegistry.getThingType(thingTypeUID) instanceof BridgeType);
            return getThingResponse(Status.BAD_REQUEST, thing, locale,
                    "A UID must be provided, since no binding can create the thing!");
        return getThingResponse(Status.CREATED, thing, locale, null);
    @Operation(operationId = "getThings", summary = "Get all available things.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedThingDTO.class), uniqueItems = true))) })
    public Response getAll(@Context Request request,
            @DefaultValue("false") @QueryParam("staticDataOnly") @Parameter(description = "provides a cacheable list of values not expected to change regularly and checks the If-Modified-Since header") boolean staticDataOnly) {
        Stream<EnrichedThingDTO> thingStream = thingRegistry.stream().map(t -> convertToEnrichedThingDTO(t, locale))
                .distinct();
            thingStream = dtoMapper.limitToFields(thingStream,
                    "UID,label,bridgeUID,thingTypeUID,location,editable,semanticEquipmentTag");
            return Response.ok(new Stream2JSONInputStream(thingStream)).lastModified(lastModified)
                    "UID,label,bridgeUID,thingTypeUID,statusInfo,firmwareStatus,location,editable,semanticEquipmentTag");
        return Response.ok(new Stream2JSONInputStream(thingStream)).build();
    @Operation(operationId = "getThingById", summary = "Gets thing by UID.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedThingDTO.class))),
                    @ApiResponse(responseCode = "404", description = "Thing not found.") })
            @PathParam("thingUID") @Parameter(description = "thingUID") String thingUID) {
        Thing thing = thingRegistry.get((new ThingUID(thingUID)));
        // return Thing data if it does exist
            return getThingResponse(Status.OK, thing, locale, null);
            return getThingNotFoundResponse(thingUID);
     * Delete a Thing, if possible. Thing deletion might be impossible if the
     * Thing is not managed, will return CONFLICT. Thing deletion might happen
     * delayed, will return ACCEPTED.
     * @param thingUID
     * @param force
     * @return Response with status/error information
    @Operation(operationId = "removeThingById", summary = "Removes a thing from the registry. Set \'force\' to __true__ if you want the thing to be removed immediately.", security = {
                    @ApiResponse(responseCode = "202", description = "ACCEPTED for asynchronous deletion."),
                    @ApiResponse(responseCode = "404", description = "Thing not found."),
                    @ApiResponse(responseCode = "409", description = "Thing could not be deleted because it's not editable.") })
            @DefaultValue("false") @QueryParam("force") @Parameter(description = "force") boolean force) {
        // check whether thing exists and throw 404 if not
        Thing thing = thingRegistry.get(thingUIDObject);
            logger.info("Received HTTP DELETE request for update at '{}' for the unknown thing '{}'.",
                    uriInfo.getPath(), thingUID);
        // ask whether the Thing exists as a managed thing, so it can get
        // updated, 409 otherwise
        Thing managed = managedThingProvider.get(thingUIDObject);
        if (managed == null) {
            logger.info("Received HTTP DELETE request for update at '{}' for an unmanaged thing '{}'.",
                    "Cannot delete Thing " + thingUID + " as it is not editable.");
        // only move on if Thing is known to be managed, so it can get updated
        if (force) {
            if (thingRegistry.forceRemove(thingUIDObject) == null) {
                return getThingResponse(Status.INTERNAL_SERVER_ERROR, thing, locale,
                        "Cannot delete Thing " + thingUID + " for unknown reasons.");
            if (thingRegistry.remove(thingUIDObject) != null) {
                return getThingResponse(Status.ACCEPTED, thing, locale, null);
     * Update Thing.
     * @return Response with the updated Thing or error information
    @Operation(operationId = "updateThing", summary = "Updates a thing.", security = {
                    @ApiResponse(responseCode = "409", description = "Thing could not be updated as it is not editable.") })
            @Parameter(description = "thing", required = true) ThingDTO thingBean) throws IOException {
        // ask whether the Thing exists at all, 404 otherwise
            logger.info("Received HTTP PUT request for update at '{}' for the unknown thing '{}'.", uriInfo.getPath(),
            logger.info("Received HTTP PUT request for update at '{}' for an unmanaged thing '{}'.", uriInfo.getPath(),
                    "Cannot update Thing " + thingUID + " as it is not editable.");
        // check configuration
        thingBean.configuration = normalizeConfiguration(thingBean.configuration, thing.getThingTypeUID(),
        normalizeChannels(thingBean, thing.getUID());
        thing = ThingHelper.merge(thing, thingBean);
        // update, returns null in case Thing cannot be found
        Thing oldthing = managedThingProvider.update(thing);
        if (oldthing == null) {
        // everything went well
     * Updates Thing configuration.
     * @param configurationParameters
    @Path("/{thingUID}/config")
    @Operation(operationId = "updateThingConfig", summary = "Updates thing's configuration.", security = {
                    @ApiResponse(responseCode = "400", description = "Configuration of the thing is not valid."),
                    @ApiResponse(responseCode = "404", description = "Thing not found"),
            @PathParam("thingUID") @Parameter(description = "thing") String thingUID,
            @Parameter(description = "configuration parameters") @Nullable Map<String, @Nullable Object> configurationParameters)
            logger.info("Received HTTP PUT request for update configuration at '{}' for the unknown thing '{}'.",
        // ask whether the Thing exists as a managed thing, so it can get updated, 409 otherwise
            logger.info("Received HTTP PUT request for update configuration at '{}' for an unmanaged thing '{}'.",
        // check if handler of Thing is available, so it can be updated, 409 otherwise
        ThingHandler thingHandler = thing.getHandler();
            logger.info("Received HTTP PUT request for update configuration at '{}' for an uninitialized thing '{}'.",
                    "Cannot update Thing " + thingUID + " as it is not initialized.");
        // only move on if Thing is known to be managed and handler is available, so it can get updated
            // note that we create a Configuration instance here in order to have normalized types
            thingRegistry.updateConfiguration(thingUIDObject,
                    new Configuration(
                            normalizeConfiguration(configurationParameters, thing.getThingTypeUID(), thing.getUID()))
                            .getProperties());
        } catch (ConfigValidationException ex) {
            logger.debug("Config description validation exception occurred for thingUID {} - Messages: {}", thingUID,
                    ex.getValidationMessages());
            return Response.status(Status.BAD_REQUEST).entity(ex.getValidationMessages(locale)).build();
            logger.error("Exception during HTTP PUT request for update config at '{}'", uriInfo.getPath(), ex);
            return JSONResponse.createResponse(Status.INTERNAL_SERVER_ERROR, null, ex.getMessage());
    @Path("/{thingUID}/status")
    @Operation(operationId = "getThingStatus", summary = "Gets thing status.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ThingStatusInfo.class))),
    public Response getStatus(
            @PathParam("thingUID") @Parameter(description = "thing") String thingUID) throws IOException {
        // Check if the Thing exists, 404 if not
            logger.info("Received HTTP GET request for thing config status at '{}' for the unknown thing '{}'.",
        ThingStatusInfo thingStatusInfo = thingStatusInfoI18nLocalizationService.getLocalizedThingStatusInfo(thing,
                localeService.getLocale(language));
        return Response.ok().entity(thingStatusInfo).build();
    @Path("/{thingUID}/enable")
    @Operation(operationId = "enableThing", summary = "Sets the thing enabled status.", security = {
    public Response setEnabled(
            @Parameter(description = "enabled") String enabled) throws IOException {
            logger.info("Received HTTP PUT request for set enabled at '{}' for the unknown thing '{}'.",
        thingManager.setEnabled(thingUIDObject, Boolean.parseBoolean(enabled));
    @Path("/{thingUID}/config/status")
    @Operation(operationId = "getThingConfigStatus", summary = "Gets thing config status.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ConfigStatusMessage.class)))),
    public Response getConfigStatus(
            @HeaderParam(HttpHeaders.ACCEPT_LANGUAGE) @Parameter(description = "language") String language,
        ConfigStatusInfo info = configStatusService.getConfigStatus(thingUID, localeService.getLocale(language));
            return Response.ok().entity(info.getConfigStatusMessages()).build();
        return Response.ok().entity(Set.of()).build();
    @Path("/{thingUID}/firmware/{firmwareVersion}")
    @Operation(operationId = "updateThingFirmware", summary = "Update thing firmware.", security = {
                    @ApiResponse(responseCode = "400", description = "Firmware update preconditions not satisfied."),
    public Response updateFirmware(
            @PathParam("firmwareVersion") @Parameter(description = "version") String firmwareVersion)
            logger.info("Received HTTP PUT request for firmware update at '{}' for the unknown thing '{}'.",
        if (firmwareVersion.isEmpty()) {
                    "Received HTTP PUT request for firmware update at '{}' for thing '{}' with unknown firmware version '{}'.",
                    uriInfo.getPath(), thingUID, firmwareVersion);
            return JSONResponse.createResponse(Status.BAD_REQUEST, null, "Firmware version is empty");
            firmwareUpdateService.updateFirmware(thing.getUID(), firmwareVersion, localeService.getLocale(language));
        } catch (IllegalArgumentException | IllegalStateException ex) {
            return JSONResponse.createResponse(Status.BAD_REQUEST, null,
                    "Firmware update preconditions not satisfied.");
    @Path("/{thingUID}/firmware/status")
    @Operation(operationId = "getThingFirmwareStatus", summary = "Gets thing's firmware status.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = FirmwareStatusDTO.class))),
                    @ApiResponse(responseCode = "204", description = "No firmware status provided by this Thing.") })
    public Response getFirmwareStatus(
        FirmwareStatusDTO firmwareStatusDto = getThingFirmwareStatusInfo(thingUIDObject);
        if (firmwareStatusDto == null) {
            return Response.status(Status.NO_CONTENT).build();
        return Response.ok(firmwareStatusDto, MediaType.APPLICATION_JSON).build();
    @Path("/{thingUID}/firmwares")
    @Operation(operationId = "getAvailableFirmwaresForThing", summary = "Get all available firmwares for provided thing UID", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = FirmwareDTO.class), uniqueItems = true))),
                    @ApiResponse(responseCode = "204", description = "No firmwares found.") })
    public Response getFirmwares(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID,
        Thing thing = thingRegistry.get(aThingUID);
                    "Received HTTP GET request for listing available firmwares at {} for unknown thing with UID '{}'",
        Collection<Firmware> firmwares = firmwareRegistry.getFirmwares(thing, localeService.getLocale(language));
        if (firmwares.isEmpty()) {
        Stream<FirmwareDTO> firmwareStream = firmwares.stream().map(this::convertToFirmwareDTO);
        return Response.ok().entity(new Stream2JSONInputStream(firmwareStream)).build();
    private FirmwareDTO convertToFirmwareDTO(Firmware firmware) {
        return new FirmwareDTO(firmware.getThingTypeUID().getAsString(), firmware.getVendor(), firmware.getModel(),
                firmware.isModelRestricted(), firmware.getDescription(), firmware.getVersion(),
                firmware.getPrerequisiteVersion(), firmware.getChangelog());
    private @Nullable FirmwareStatusDTO getThingFirmwareStatusInfo(ThingUID thingUID) {
        FirmwareStatusInfo info = firmwareUpdateService.getFirmwareStatusInfo(thingUID);
            return buildFirmwareStatusDTO(info);
    private FirmwareStatusDTO buildFirmwareStatusDTO(FirmwareStatusInfo info) {
        return new FirmwareStatusDTO(info.getFirmwareStatus().name(), info.getUpdatableFirmwareVersion());
     * helper: Response to be sent to client if a Thing cannot be found
     * @return Response configured for NOT_FOUND
    private static Response getThingNotFoundResponse(String thingUID) {
        String message = "Thing " + thingUID + " does not exist!";
     * helper: create a Response holding a Thing and/or error information.
     * @param thing
     * @param errormessage an optional error message (may be null), ignored if the status family is successful
     * @return Response
    private Response getThingResponse(Status status, @Nullable Thing thing, Locale locale,
            @Nullable String errormessage) {
        boolean managed = thing != null && managedThingProvider.get(thing.getUID()) != null;
        EnrichedThingDTO enrichedThingDTO = thing != null
                ? EnrichedThingDTOMapper.map(thing, thingStatusInfo, this.getThingFirmwareStatusInfo(thing.getUID()),
                        getLinkedItemsMap(thing), managed)
        return JSONResponse.createResponse(status, enrichedThingDTO, errormessage);
    private EnrichedThingDTO convertToEnrichedThingDTO(Thing thing, Locale locale) {
        boolean managed = managedThingProvider.get(thing.getUID()) != null;
        return EnrichedThingDTOMapper.map(thing, thingStatusInfo, this.getThingFirmwareStatusInfo(thing.getUID()),
                getLinkedItemsMap(thing), managed);
    private Map<String, Set<String>> getLinkedItemsMap(Thing thing) {
        Map<String, Set<String>> linkedItemsMap = new HashMap<>();
        for (Channel channel : thing.getChannels()) {
            Set<String> linkedItems = itemChannelLinkRegistry.getLinkedItemNames(channel.getUID());
            linkedItemsMap.put(channel.getUID().getId(), linkedItems);
        return linkedItemsMap;
