package org.openhab.core.io.rest.core.internal.persistence;
import org.openhab.core.persistence.FilterCriteria;
import org.openhab.core.persistence.FilterCriteria.Ordering;
import org.openhab.core.persistence.HistoricItem;
import org.openhab.core.persistence.ModifiablePersistenceService;
import org.openhab.core.persistence.PersistenceItemConfiguration;
import org.openhab.core.persistence.PersistenceItemInfo;
import org.openhab.core.persistence.PersistenceManager;
import org.openhab.core.persistence.PersistenceService;
import org.openhab.core.persistence.PersistenceServiceProblem;
import org.openhab.core.persistence.PersistenceServiceRegistry;
import org.openhab.core.persistence.QueryablePersistenceService;
import org.openhab.core.persistence.dto.ItemHistoryDTO;
import org.openhab.core.persistence.dto.PersistenceCronStrategyDTO;
import org.openhab.core.persistence.dto.PersistenceServiceConfigurationDTO;
import org.openhab.core.persistence.dto.PersistenceServiceDTO;
import org.openhab.core.persistence.dto.PersistenceStrategyDTO;
import org.openhab.core.persistence.registry.ManagedPersistenceServiceConfigurationProvider;
import org.openhab.core.persistence.registry.PersistenceServiceConfiguration;
import org.openhab.core.persistence.registry.PersistenceServiceConfigurationDTOMapper;
import org.openhab.core.persistence.registry.PersistenceServiceConfigurationRegistry;
import org.openhab.core.persistence.strategy.PersistenceStrategy;
 * This class acts as a REST resource for history data and provides different methods to interact with the persistence
 * store
 * @author Kai Kreuzer - Refactored to use PersistenceServiceRegistryImpl
 * @author Erdoan Hadzhiyusein - Adapted the convertTime() method to work with the new DateTimeType
 * @author Lyubomir Papazov - Change java.util.Date references to be of type java.time.ZonedDateTime
 * @author Mark Herwege - Implement aliases
 * @author Mark Herwege - Make default strategy to be only a configuration suggestion
@JaxrsName(PersistenceResource.PATH_PERSISTENCE)
@Path(PersistenceResource.PATH_PERSISTENCE)
@Tag(name = PersistenceResource.PATH_PERSISTENCE)
public class PersistenceResource implements RESTResource {
    // The URI path to this resource
    public static final String PATH_PERSISTENCE = "persistence";
    private final Logger logger = LoggerFactory.getLogger(PersistenceResource.class);
    private static final String MODIFYABLE = "Modifiable";
    private static final String QUERYABLE = "Queryable";
    private static final String STANDARD = "Standard";
    private final PersistenceServiceRegistry persistenceServiceRegistry;
    private final PersistenceManager persistenceManager;
    private final PersistenceServiceConfigurationRegistry persistenceServiceConfigurationRegistry;
    private final ManagedPersistenceServiceConfigurationProvider managedPersistenceServiceConfigurationProvider;
    public PersistenceResource( //
            final @Reference PersistenceServiceRegistry persistenceServiceRegistry,
            final @Reference PersistenceManager persistenceManager,
            final @Reference PersistenceServiceConfigurationRegistry persistenceServiceConfigurationRegistry,
            final @Reference ManagedPersistenceServiceConfigurationProvider managedPersistenceServiceConfigurationProvider,
            final @Reference TimeZoneProvider timeZoneProvider,
            final @Reference ConfigurationService configurationService) {
        this.persistenceServiceRegistry = persistenceServiceRegistry;
        this.persistenceManager = persistenceManager;
        this.persistenceServiceConfigurationRegistry = persistenceServiceConfigurationRegistry;
        this.managedPersistenceServiceConfigurationProvider = managedPersistenceServiceConfigurationProvider;
    @Operation(operationId = "getPersistenceServices", summary = "Gets a list of persistence services.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = PersistenceServiceDTO.class)))) })
    public Response httpGetPersistenceServices(@Context HttpHeaders headers,
        Object responseObject = getPersistenceServiceList(locale);
    @Path("{serviceId: [a-zA-Z0-9]+}")
    @Operation(operationId = "getPersistenceServiceConfiguration", summary = "Gets a persistence service configuration.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = PersistenceServiceConfigurationDTO.class))),
                    @ApiResponse(responseCode = "404", description = "Service configuration not found.") })
    public Response httpGetPersistenceServiceConfiguration(@Context HttpHeaders headers,
            @Parameter(description = "Id of the persistence service.") @PathParam("serviceId") String serviceId) {
        PersistenceServiceConfiguration configuration = persistenceServiceConfigurationRegistry.get(serviceId);
            PersistenceServiceConfigurationDTO configurationDTO = PersistenceServiceConfigurationDTOMapper
                    .map(configuration);
            configurationDTO.editable = managedPersistenceServiceConfigurationProvider.get(serviceId) != null;
            return JSONResponse.createResponse(Status.OK, configurationDTO, null);
    @Operation(operationId = "putPersistenceServiceConfiguration", summary = "Sets a persistence service configuration.", security = {
                    @ApiResponse(responseCode = "201", description = "PersistenceServiceConfiguration created"),
                    @ApiResponse(responseCode = "400", description = "Payload invalid"),
                    @ApiResponse(responseCode = "405", description = "PersistenceServiceConfiguration not editable") })
    public Response httpPutPersistenceServiceConfiguration(@Context UriInfo uriInfo, @Context HttpHeaders headers,
            @Parameter(description = "Id of the persistence service.") @PathParam("serviceId") String serviceId,
            @Parameter(description = "service configuration", required = true) @Nullable PersistenceServiceConfigurationDTO serviceConfigurationDTO) {
        if (serviceConfigurationDTO == null) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Payload must not be null.");
        if (!serviceId.equals(serviceConfigurationDTO.serviceId)) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "serviceId in payload '"
                    + serviceConfigurationDTO.serviceId + "' differs from serviceId in URL '" + serviceId + "'");
        PersistenceServiceConfiguration persistenceServiceConfiguration;
            persistenceServiceConfiguration = PersistenceServiceConfigurationDTOMapper.map(serviceConfigurationDTO);
            logger.warn("Received HTTP PUT request at '{}' with an invalid payload: '{}'.", uriInfo.getPath(),
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, e.getMessage());
        if (persistenceServiceConfigurationRegistry.get(serviceId) == null) {
            managedPersistenceServiceConfigurationProvider.add(persistenceServiceConfiguration);
            return JSONResponse.createResponse(Status.CREATED, serviceConfigurationDTO, null);
        } else if (managedPersistenceServiceConfigurationProvider.get(serviceId) != null) {
            managedPersistenceServiceConfigurationProvider.update(persistenceServiceConfiguration);
            return JSONResponse.createResponse(Status.OK, serviceConfigurationDTO, null);
            // Configuration exists but cannot be updated
            logger.warn("Cannot update existing persistence service configuration '{}', because is not managed.",
                    serviceId);
                    "Cannot update non-managed persistence service configuration " + serviceId);
    @Operation(operationId = "deletePersistenceServiceConfiguration", summary = "Deletes a persistence service configuration.", security = {
                    @ApiResponse(responseCode = "404", description = "Persistence service configuration not found"),
                    @ApiResponse(responseCode = "405", description = "Persistence service configuration not editable") })
    public Response httpDeletePersistenceServiceConfiguration(@Context UriInfo uriInfo, @Context HttpHeaders headers,
        if (managedPersistenceServiceConfigurationProvider.remove(serviceId) == null) {
    @Operation(operationId = "getItemsForPersistenceService", summary = "Gets a list of stored Items available via a specific persistence service with their stored name.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = PersistenceItemInfoDTO.class), uniqueItems = true))),
                    @ApiResponse(responseCode = "404", description = "Unknown persistence service or Item not found in persistence store"),
                    @ApiResponse(responseCode = "405", description = "Persistence service not queryable or getting Item info not allowed") })
    public Response httpGetPersistenceServiceItems(@Context HttpHeaders headers,
            @Parameter(description = "Id of the persistence service. If not provided the default service will be used") @QueryParam("serviceId") @Nullable String serviceId,
            @Parameter(description = "An Item name, if provided response will only contain information for this Item") @QueryParam("itemName") @Nullable String itemName) {
        return getServiceItemListDTO(serviceId, itemName);
    @Path("/items/{itemName: [a-zA-Z_0-9]+}")
    @Operation(operationId = "getItemDataFromPersistenceService", summary = "Gets Item persistence data from the persistence service.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ItemHistoryDTO.class))),
            @ApiResponse(responseCode = "405", description = "Persistence service not queryable") })
    public Response httpGetPersistenceItemData(@Context HttpHeaders headers,
            @Parameter(description = "The Item name") @PathParam("itemName") String itemName,
            @Parameter(description = "Start time of the data to return. Will default to 1 day before endtime. ["
                    + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS
                    + "]") @QueryParam("starttime") @Nullable String startTime,
            @Parameter(description = "End time of the data to return. Will default to current time. ["
                    + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS + "]") @QueryParam("endtime") @Nullable String endTime,
            @Parameter(description = "Page number of data to return. This parameter will enable paging.") @QueryParam("page") int pageNumber,
            @Parameter(description = "The length of each page.") @QueryParam("pagelength") int pageLength,
            @Parameter(description = "Gets one value before and after the requested period.") @QueryParam("boundary") boolean boundary,
            @Parameter(description = "Adds the current Item state into the requested period (the Item state will be before or at the endtime)") @QueryParam("itemState") boolean itemState) {
        return getItemHistoryDTO(serviceId, itemName, startTime, endTime, pageNumber, pageLength, boundary, itemState);
    @Operation(operationId = "deleteItemFromPersistenceService", summary = "Deletes Item persistence data from a specific persistence service in a given time range.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = String.class)))),
                    @ApiResponse(responseCode = "400", description = "Invalid filter parameters"),
                    @ApiResponse(responseCode = "404", description = "Unknown persistence service") })
    public Response httpDeletePersistenceServiceItem(@Context HttpHeaders headers,
            @Parameter(description = "Id of the persistence service.", required = true) @QueryParam("serviceId") String serviceId,
            @Parameter(description = "The Item name.") @PathParam("itemName") String itemName,
            @Parameter(description = "Start of the time range to be deleted. ["
                    + "]", required = true) @QueryParam("starttime") String startTime,
            @Parameter(description = "End of the time range to be deleted. [" + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS
                    + "]", required = true) @QueryParam("endtime") String endTime) {
        return deletePersistenceItemData(serviceId, itemName, startTime, endTime);
    @Operation(operationId = "storeItemDataInPersistenceService", summary = "Stores Item persistence data into the persistence service.", security = {
                    @ApiResponse(responseCode = "400", description = "Item not found, invalid state, invalid time format, or persistence service not found or not modifiable"),
                    @ApiResponse(responseCode = "404", description = "Unknown Item or persistence service") })
    public Response httpPutPersistenceItemData(@Context HttpHeaders headers,
            @Parameter(description = "Time of the data to be stored. Will default to current time. ["
                    + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS + "]", required = true) @QueryParam("time") String time,
            @Parameter(description = "The state to store.", required = true) @QueryParam("state") String value) {
        return putItemState(serviceId, itemName, value, time);
    @Path("strategysuggestions")
    @Operation(operationId = "getPersistenceServiceStrategySuggestions", summary = "Gets a persistence service suggested strategies.", security = {
                            PersistenceStrategyDTO.class, PersistenceCronStrategyDTO.class }), uniqueItems = true))),
                    @ApiResponse(responseCode = "404", description = "Suggested strategies not found.") })
    public Response httpGetPersistenceServiceStrategySuggestions(@Context HttpHeaders headers,
            @Parameter(description = "Id of the persistence service.") @QueryParam("serviceId") String serviceId) {
        PersistenceService service = persistenceServiceRegistry.get(serviceId);
            return JSONResponse.createResponse(Status.OK, service.getSuggestedStrategies(), null);
    @Path("health")
    @Operation(operationId = "getPersistenceHealth", summary = "Gets configuration problems with persistence services.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = PersistenceServiceProblem.class), uniqueItems = true))) })
    public Response httpGetPersistenceHealth(@Context HttpHeaders headers) {
        List<PersistenceServiceProblem> persistenceProblems = new ArrayList<>();
        List<String> configurationConflicts = persistenceServiceConfigurationRegistry
                .getServiceConfigurationConflicts();
        for (String serviceId : configurationConflicts) {
            persistenceProblems.add(new PersistenceServiceProblem(
                    PersistenceServiceProblem.PERSISTENCE_DUPLICATE_CONFIG, serviceId, null, false));
        Set<PersistenceService> persistenceServices = persistenceServiceRegistry.getAll();
        if (persistenceServices.size() > 1) {
                Configuration configuration = configurationService.get("org.openhab.persistence");
                if (configuration == null || configuration.get("default") == null) {
                            PersistenceServiceProblem.PERSISTENCE_NO_DEFAULT, null, null, true));
                logger.warn("Unable to retrieve configuration for 'org.openhab.persistence': {}", e.getMessage());
        for (PersistenceService service : persistenceServices) {
            String serviceId = service.getId();
            PersistenceServiceConfiguration serviceConfig = persistenceServiceConfigurationRegistry.get(serviceId);
            if (serviceConfig == null) {
                persistenceProblems.add(new PersistenceServiceProblem(PersistenceServiceProblem.PERSISTENCE_NO_CONFIG,
                        serviceId, null, true));
                boolean editable = managedPersistenceServiceConfigurationProvider.get(serviceId) != null;
                List<PersistenceItemConfiguration> configs = serviceConfig.getConfigs();
                if (configs.isEmpty()) {
                            PersistenceServiceProblem.PERSISTENCE_NO_ITEMS, serviceId, null, editable));
                    for (PersistenceItemConfiguration config : configs) {
                        List<PersistenceStrategy> strategies = config.strategies();
                        List<String> items = config.items().stream()
                                .map(PersistenceServiceConfigurationDTOMapper::persistenceConfigToString).toList();
                        if (strategies.isEmpty()) {
                                    PersistenceServiceProblem.PERSISTENCE_NO_STRATEGY, serviceId, items, editable));
                        } else if (strategies.size() == 1
                                && PersistenceStrategy.Globals.RESTORE.equals(strategies.getFirst())) {
                                    PersistenceServiceProblem.PERSISTENCE_NO_STORE_STRATEGY, serviceId, items,
                                    editable));
        return JSONResponse.createResponse(Status.OK, persistenceProblems, null);
    private ZonedDateTime convertTime(String sTime) {
        DateTimeType dateTime = new DateTimeType(sTime);
    private Response getItemHistoryDTO(@Nullable String serviceId, String itemName, @Nullable String timeBegin,
            @Nullable String timeEnd, int pageNumber, int pageLength, boolean boundary, boolean itemState) {
        // Benchmarking timer...
        long timerStart = System.currentTimeMillis();
        // If serviceId is null, then use the default service
        PersistenceService service;
        String effectiveServiceId = serviceId != null ? serviceId : persistenceServiceRegistry.getDefaultId();
        service = effectiveServiceId != null ? persistenceServiceRegistry.get(effectiveServiceId) : null;
        if (effectiveServiceId == null || service == null) {
            logger.debug("Persistence service not found '{}'.", effectiveServiceId);
                    "Persistence service not found: " + effectiveServiceId);
        if (!(service instanceof QueryablePersistenceService)) {
            logger.debug("Persistence service not queryable '{}'.", effectiveServiceId);
                    "Persistence service not queryable: " + effectiveServiceId);
        QueryablePersistenceService qService = (QueryablePersistenceService) service;
        ItemHistoryDTO dto = createDTO(qService, itemName, timeBegin, timeEnd, pageNumber, pageLength, boundary,
                itemState);
        if (dto == null) {
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Item not found: " + itemName);
        logger.debug("Persistence returned {} rows in {}ms", dto.datapoints, System.currentTimeMillis() - timerStart);
        return JSONResponse.createResponse(Status.OK, dto, "");
    protected @Nullable ItemHistoryDTO createDTO(QueryablePersistenceService qService, String itemName,
            @Nullable String timeBegin, @Nullable String timeEnd, int pageNumber, int pageLength, boolean boundary,
            boolean itemState) {
        String serviceId = qService.getId();
        PersistenceServiceConfiguration config = persistenceServiceConfigurationRegistry.get(serviceId);
        String alias = config != null ? config.getAliases().get(itemName) : null;
        ZonedDateTime dateTimeBegin = ZonedDateTime.now();
        ZonedDateTime dateTimeEnd = dateTimeBegin;
        if (timeBegin != null) {
            dateTimeBegin = convertTime(timeBegin);
        if (timeEnd != null) {
            dateTimeEnd = convertTime(timeEnd);
        // End now...
        if (dateTimeEnd.toEpochSecond() == 0) {
            dateTimeEnd = ZonedDateTime.of(LocalDateTime.now(), timeZoneProvider.getTimeZone());
        if (dateTimeBegin.toEpochSecond() == 0) {
            // Default to 1 days data if the times are the same or the start time is newer
            // than the end time
            dateTimeBegin = ZonedDateTime.of(dateTimeEnd.toLocalDateTime().plusDays(-1),
                    timeZoneProvider.getTimeZone());
        if (dateTimeBegin.isAfter(dateTimeEnd) || dateTimeBegin.isEqual(dateTimeEnd)) {
        Iterable<HistoricItem> result;
        long quantity = 0L;
        ItemHistoryDTO dto = new ItemHistoryDTO();
        dto.name = itemName;
        // If "boundary" is true then we want to get one value before and after the requested period
        // This is necessary for values that don't change often otherwise data will start after the start of the graph
        // (or not at all if there's no change during the graph period)
            // Get the value before the start time.
            FilterCriteria filterBeforeStart = new FilterCriteria();
            filterBeforeStart.setItemName(itemName);
            filterBeforeStart.setEndDate(dateTimeBegin);
            filterBeforeStart.setPageSize(1);
            filterBeforeStart.setOrdering(Ordering.DESCENDING);
            result = qService.query(filterBeforeStart, alias);
            if (result.iterator().hasNext()) {
                dto.addData(dateTimeBegin.toInstant().toEpochMilli(), result.iterator().next().getState());
                quantity++;
        FilterCriteria filter = new FilterCriteria();
        filter.setItemName(itemName);
        if (pageLength == 0) {
            filter.setPageNumber(0);
            filter.setPageSize(Integer.MAX_VALUE);
            filter.setPageNumber(pageNumber);
            filter.setPageSize(pageLength);
        filter.setBeginDate(dateTimeBegin);
        filter.setEndDate(dateTimeEnd);
        filter.setOrdering(Ordering.ASCENDING);
        result = qService.query(filter, alias);
        Iterator<HistoricItem> it = result.iterator();
        // Iterate through the data
        State lastState = null;
            HistoricItem historicItem = it.next();
            State state = historicItem.getState();
            long timestamp = historicItem.getInstant().toEpochMilli();
            // For 'binary' states, we need to replicate the data
            // to avoid diagonal lines
            if (state instanceof OnOffType || state instanceof OpenClosedType) {
                if (lastState != null && !lastState.equals(state)) {
                    dto.addData(timestamp, lastState);
            dto.addData(timestamp, state);
            lastState = state;
        boolean addedBoundaryEnd = false;
            // Get the value after the end time.
            FilterCriteria filterAfterEnd = new FilterCriteria();
            filterAfterEnd.setItemName(itemName);
            filterAfterEnd.setBeginDate(dateTimeEnd);
            filterAfterEnd.setPageSize(1);
            filterAfterEnd.setOrdering(Ordering.ASCENDING);
            result = qService.query(filterAfterEnd, alias);
                dto.addData(dateTimeEnd.toInstant().toEpochMilli(), result.iterator().next().getState());
                addedBoundaryEnd = true;
        // only add the item state if it was requested and the boundary end was not added
        // if the boundary end was added, there is no need to add the item state moved to the end time
        if (itemState && !addedBoundaryEnd) {
                long time = Instant.now().toEpochMilli();
                // if the current time is after the requested end time, move the item state to the end time
                if (time > dateTimeEnd.toInstant().toEpochMilli()) {
                    time = dateTimeEnd.toInstant().toEpochMilli();
                State state = itemRegistry.getItem(itemName).getState();
                if (state instanceof UnDefType) {
                    logger.debug("State of Item '{}' is undefined, not adding it to the response.", itemName);
                    logger.debug("Adding state of Item '{}' to the response: {} - {}", itemName, time, state);
                    dto.addData(time, state);
                    dto.sortData();
                logger.debug("Item '{}' not found, not adding the state to the response.", itemName);
        dto.datapoints = Long.toString(quantity);
     * Gets a list of persistence services currently configured in the system
     * @return list of persistence services
    private List<PersistenceServiceDTO> getPersistenceServiceList(Locale locale) {
        List<PersistenceServiceDTO> dtoList = new ArrayList<>();
        for (PersistenceService service : persistenceServiceRegistry.getAll()) {
            PersistenceServiceDTO serviceDTO = new PersistenceServiceDTO();
            serviceDTO.id = service.getId();
            serviceDTO.label = service.getLabel(locale);
            if (service instanceof ModifiablePersistenceService) {
                serviceDTO.type = MODIFYABLE;
            } else if (service instanceof QueryablePersistenceService) {
                serviceDTO.type = QUERYABLE;
                serviceDTO.type = STANDARD;
            dtoList.add(serviceDTO);
        return dtoList;
    private Response getServiceItemListDTO(@Nullable String serviceId, @Nullable String itemName) {
        if (effectiveServiceId == null) {
            logger.debug("No default persistence service.");
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "No default persistence service.");
        PersistenceService service = persistenceServiceRegistry.get(effectiveServiceId);
            Set<PersistenceItemInfoDTO> itemInfo = createDTO(qService, itemName);
            if (itemInfo == null) {
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Item '" + itemName
                        + "' could not be found in persistence service '" + effectiveServiceId + "'");
            return JSONResponse.createResponse(Status.OK, itemInfo, "");
                    "Not supported for persistence service: " + effectiveServiceId);
    protected @Nullable Set<PersistenceItemInfoDTO> createDTO(QueryablePersistenceService qService,
            @Nullable String itemName) throws UnsupportedOperationException {
        Map<String, String> itemToAlias = config != null ? config.getAliases() : Map.of();
        Set<PersistenceItemInfo> itemInfo;
            String alias = itemToAlias.get(itemName);
            PersistenceItemInfo singleItemInfo = qService.getItemInfo(itemName, alias);
            if (singleItemInfo == null) {
            itemInfo = Set.of(singleItemInfo);
            itemInfo = qService.getItemInfo();
        Set<PersistenceItemInfoDTO> mappedItemInfo = itemInfo.stream()
                .map(info -> new PersistenceItemInfoDTO(info.getName(), info.getCount(), info.getEarliest(),
                        info.getLatest()))
        return mappedItemInfo;
    @Schema(name = "PersistenceItemInfo")
    public record PersistenceItemInfoDTO(String name, @Nullable Integer count, @Nullable Date earliest,
            @Nullable Date latest) {
    private Response deletePersistenceItemData(@Nullable String serviceId, String itemName, @Nullable String timeBegin,
            @Nullable String timeEnd) {
        // For deleting, we must specify a service id - don't use the default service
        if (serviceId == null || serviceId.isEmpty()) {
            logger.debug("Persistence service must be specified for delete operations.");
                    "Persistence service must be specified for delete operations.");
            logger.debug("Persistence service not found '{}'.", serviceId);
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Persistence service not found: " + serviceId);
        if (!(service instanceof ModifiablePersistenceService)) {
            logger.warn("Persistence service not modifiable '{}'.", serviceId);
                    "Persistence service not modifiable: " + serviceId);
        if (timeBegin == null || timeEnd == null) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "The start and end time must be set");
        ZonedDateTime dateTimeBegin = convertTime(timeBegin);
        ZonedDateTime dateTimeEnd = convertTime(timeEnd);
        if (dateTimeEnd.isBefore(dateTimeBegin)) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Start time must be earlier than end time");
        // First, get the value at the start time.
        ModifiablePersistenceService mService = (ModifiablePersistenceService) service;
            mService.remove(filter, alias);
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Invalid filter parameters.");
        return Response.status(Status.OK).build();
    private Response putItemState(@Nullable String serviceId, String itemName, String value, @Nullable String time) {
            logger.warn("Persistence service not found '{}'.", effectiveServiceId);
            logger.warn("Persistence service not modifiable '{}'.", effectiveServiceId);
                    "Persistence service not modifiable: " + effectiveServiceId);
            logger.warn("Item not found '{}'.", itemName);
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Item not found: " + itemName);
        // Try to parse a State from the input
            logger.warn("Can't persist Item {} with invalid state '{}'.", itemName, value);
        ZonedDateTime dateTime = null;
        if (time != null && !time.isEmpty()) {
            dateTime = convertTime(time);
        if (dateTime == null || dateTime.toEpochSecond() == 0) {
            logger.warn("Error with persistence store to {}. Time badly formatted {}.", itemName, time);
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Time badly formatted.");
        PersistenceServiceConfiguration config = persistenceServiceConfigurationRegistry.get(effectiveServiceId);
        mService.store(item, dateTime, state, alias);
        persistenceManager.handleExternalPersistenceDataChange(mService, item);
