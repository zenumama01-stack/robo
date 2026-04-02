package org.openhab.core.io.rest.core.internal.addons;
import java.text.Collator;
import org.openhab.core.io.rest.core.config.ConfigurationService;
 * This class acts as a REST resource for add-ons and provides methods to install and uninstall them.
 * @author Franck Dechavanne - Added DTOs to ApiResponses
 * @author Yannick Schaus - Add service-related parameters & operations
@JaxrsName(AddonResource.PATH_ADDONS)
@Path(AddonResource.PATH_ADDONS)
@Tag(name = AddonResource.PATH_ADDONS)
public class AddonResource implements RESTResource {
    private static final String THREAD_POOL_NAME = "addonService";
    public static final String PATH_ADDONS = "addons";
    public static final String DEFAULT_ADDON_SERVICE = "karaf";
    private final Logger logger = LoggerFactory.getLogger(AddonResource.class);
    private final ConfigurationService configurationService;
    private final AddonSuggestionService addonSuggestionService;
    public AddonResource(final @Reference EventPublisher eventPublisher, final @Reference LocaleService localeService,
            final @Reference ConfigurationService configurationService,
            final @Reference AddonInfoRegistry addonInfoRegistry,
            final @Reference AddonSuggestionService addonSuggestionService) {
        this.configurationService = configurationService;
        this.addonSuggestionService = addonSuggestionService;
    @Operation(operationId = "getAddons", summary = "Get all add-ons.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = Addon.class)))),
            @ApiResponse(responseCode = "404", description = "Service not found") })
    public Response getAddon(
            @QueryParam("serviceId") @Parameter(description = "service ID") @Nullable String serviceId) {
        logger.debug("Received HTTP GET request at '{}'", uriInfo.getPath());
        if ("all".equals(serviceId)) {
            return Response.ok(new Stream2JSONInputStream(getAllAddons(locale))).build();
            AddonService addonService = (serviceId != null) ? getServiceById(serviceId) : getDefaultService();
            if (addonService == null) {
                return Response.status(HttpStatus.NOT_FOUND_404).build();
            return Response.ok(new Stream2JSONInputStream(addonService.getAddons(locale).stream())).build();
    @Path("/services")
    @Operation(operationId = "getAddonTypes", summary = "Get all add-on types.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = AddonType.class)))) })
    public Response getServices(
        Stream<AddonServiceDTO> addonTypeStream = addonServices.stream().map(s -> convertToAddonServiceDTO(s, locale));
        return Response.ok(new Stream2JSONInputStream(addonTypeStream)).build();
    @Path("/suggestions")
    @Operation(operationId = "getSuggestedAddons", summary = "Get suggested add-ons to be installed.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = Addon.class)))), })
    public Response getSuggestions(
        return Response.ok(new Stream2JSONInputStream(addonSuggestionService.getSuggestedAddons(locale).stream()))
    @Path("/types")
    @Operation(operationId = "getAddonServices", summary = "Get add-on services.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = AddonType.class)))),
    public Response getTypes(
        if (serviceId != null) {
            AddonService service = getServiceById(serviceId);
                Stream<AddonType> addonTypeStream = getAddonTypesForService(service, locale).stream().distinct();
            Stream<AddonType> addonTypeStream = getAllAddonTypes(locale).stream().distinct();
    @Path("/{addonId: [a-zA-Z_0-9-:]+}")
    @Operation(operationId = "getAddonById", summary = "Get add-on with given ID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = Addon.class))),
            @ApiResponse(responseCode = "404", description = "Not found") })
    public Response getById(
            @PathParam("addonId") @Parameter(description = "addon ID") String addonId,
        logger.debug("Received HTTP GET request at '{}'.", uriInfo.getPath());
        Addon responseObject = addonService.getAddon(addonId, locale);
        if (responseObject != null) {
            return Response.ok(responseObject).build();
    @Path("/{addonId: [a-zA-Z_0-9-:]+}/install")
    @Operation(operationId = "installAddonById", summary = "Installs the add-on with the given ID.", responses = {
    public Response installAddon(final @PathParam("addonId") @Parameter(description = "addon ID") String addonId,
        if (addonService == null || addonService.getAddon(addonId, null) == null) {
        ThreadPoolManager.getPool(THREAD_POOL_NAME).submit(() -> {
                addonService.install(addonId);
                logger.error("Exception while installing add-on: {}", e.getMessage());
                postFailureEvent(addonId, e.getMessage());
    @Path("/url/{url}/install")
    @Operation(operationId = "installAddonFromURL", summary = "Installs the add-on from the given URL.", responses = {
            @ApiResponse(responseCode = "400", description = "The given URL is malformed or not valid.") })
    public Response installAddonByURL(
            final @PathParam("url") @Parameter(description = "addon install URL") String url) {
            URI addonURI = new URI(url);
            String addonId = getAddonId(addonURI);
            installAddon(addonId, getAddonServiceForAddonId(addonURI));
        } catch (URISyntaxException | IllegalArgumentException e) {
            logger.error("Exception while parsing the addon URL '{}': {}", url, e.getMessage());
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "The given URL is malformed or not valid.");
    @Path("/{addonId: [a-zA-Z_0-9-:]+}/uninstall")
    @Operation(operationId = "uninstallAddon", summary = "Uninstalls the add-on with the given ID.", responses = {
    public Response uninstallAddon(final @PathParam("addonId") @Parameter(description = "addon ID") String addonId,
                addonService.uninstall(addonId);
                logger.error("Exception while uninstalling add-on: {}", e.getMessage());
    @Path("/{addonId: [a-zA-Z_0-9-:]+}/config")
    @Operation(operationId = "getAddonConfiguration", summary = "Get add-on configuration for given add-on ID.", responses = {
            @ApiResponse(responseCode = "404", description = "Add-on does not exist"),
            @ApiResponse(responseCode = "500", description = "Configuration can not be read due to internal error") })
    public Response getConfiguration(final @PathParam("addonId") @Parameter(description = "addon ID") String addonId,
            String infoUid = addon.getType() + "-" + addon.getId();
            AddonInfo addonInfo = addonInfoRegistry.getAddonInfo(infoUid);
            if (addonInfo == null) {
            Configuration configuration = configurationService.get(addonInfo.getServiceId());
            return configuration != null ? Response.ok(configuration.getProperties()).build()
                    : Response.ok(Map.of()).build();
            logger.error("Cannot get configuration for service {}: {}", addonId, e.getMessage(), e);
            return Response.status(Status.INTERNAL_SERVER_ERROR).build();
    @Operation(operationId = "updateAddonConfiguration", summary = "Updates an add-on configuration for given ID and returns the old configuration.", responses = {
            @ApiResponse(responseCode = "204", description = "No old configuration"),
            @ApiResponse(responseCode = "500", description = "Configuration can not be updated due to internal error") })
    public Response updateConfiguration(@PathParam("addonId") @Parameter(description = "Add-on id") String addonId,
            @QueryParam("serviceId") @Parameter(description = "service ID") @Nullable String serviceId,
            @Nullable Map<String, @Nullable Object> configuration) {
            Configuration oldConfiguration = configurationService.get(addonInfo.getServiceId());
            configurationService.update(addonInfo.getServiceId(),
                    new Configuration(normalizeConfiguration(configuration, infoUid)));
            return oldConfiguration != null ? Response.ok(oldConfiguration.getProperties()).build()
                    : Response.noContent().build();
            logger.error("Cannot update configuration for service {}: {}", addonId, ex.getMessage(), ex);
    private @Nullable Map<String, @Nullable Object> normalizeConfiguration(
            @Nullable Map<String, @Nullable Object> properties, String addonId) {
        if (properties == null || properties.isEmpty()) {
        AddonInfo addonInfo = addonInfoRegistry.getAddonInfo(addonId);
        if (addonInfo == null || addonInfo.getConfigDescriptionURI() == null) {
        String configDescriptionURI = addonInfo.getConfigDescriptionURI();
        if (configDescriptionURI != null) {
            ConfigDescription configDesc = configDescriptionRegistry
                    .getConfigDescription(URI.create(configDescriptionURI));
            if (configDesc != null) {
                return ConfigUtil.normalizeTypes(properties, List.of(configDesc));
    private void postFailureEvent(String addonId, @Nullable String msg) {
        Event event = AddonEventFactory.createAddonFailureEvent(addonId, msg);
    private @Nullable AddonService getDefaultService() {
        return addonServices.stream().filter(addonService -> DEFAULT_ADDON_SERVICE.equals(addonService.getId()))
                .findFirst().orElse(addonServices.stream().findFirst().orElse(null));
    private Stream<Addon> getAllAddons(Locale locale) {
        return addonServices.stream().map(s -> s.getAddons(locale)).flatMap(Collection::stream);
    private Set<AddonType> getAllAddonTypes(Locale locale) {
        final Collator coll = Collator.getInstance(locale);
        coll.setStrength(Collator.PRIMARY);
        Set<AddonType> ret = new TreeSet<>((o1, o2) -> coll.compare(o1.getLabel(), o2.getLabel()));
            ret.addAll(addonService.getTypes(locale));
    private Set<AddonType> getAddonTypesForService(AddonService addonService, Locale locale) {
    private @Nullable AddonService getServiceById(final String serviceId) {
            if (addonService.getId().equals(serviceId)) {
                return addonService;
    private String getAddonId(URI addonURI) {
            String addonId = addonService.getAddonId(addonURI);
            if (addonId != null && !addonId.isBlank()) {
                return addonId;
        throw new IllegalArgumentException("No add-on service registered for URI " + addonURI);
    private String getAddonServiceForAddonId(URI addonURI) {
                return addonService.getId();
    private AddonServiceDTO convertToAddonServiceDTO(AddonService addonService, Locale locale) {
        return new AddonServiceDTO(addonService.getId(), addonService.getName(),
                getAddonTypesForService(addonService, locale));
