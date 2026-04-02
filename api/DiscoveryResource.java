package org.openhab.core.io.rest.core.internal.discovery;
import org.openhab.core.io.rest.core.discovery.DiscoveryInfoDTO;
 * This class acts as a REST resource for discovery and is registered with the
 * Jersey servlet.
 * @author Kai Kreuzer - refactored for using the OSGi JAX-RS connector
 * @author Yordan Zhelev - Added Swagger annotations
 * @author Ivaylo Ivanov - Added payload to the response of <code>scan</code>
@Component(service = { RESTResource.class, DiscoveryResource.class })
@JaxrsName(DiscoveryResource.PATH_DISCOVERY)
@Path(DiscoveryResource.PATH_DISCOVERY)
@Tag(name = DiscoveryResource.PATH_DISCOVERY)
public class DiscoveryResource implements RESTResource {
    public static final String PATH_DISCOVERY = "discovery";
    private final Logger logger = LoggerFactory.getLogger(DiscoveryResource.class);
    public DiscoveryResource(final @Reference DiscoveryServiceRegistry discoveryServiceRegistry,
            final @Reference TranslationProvider translationProvider, final @Reference LocaleService localeService) {
        this.i18nProvider = translationProvider;
    @Operation(operationId = "getBindingsWithDiscoverySupport", summary = "Gets all bindings that support discovery.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = String.class), uniqueItems = true))) })
    public Response getDiscoveryServices() {
        Collection<String> supportedBindings = discoveryServiceRegistry.getSupportedBindings();
        return Response.ok(new LinkedHashSet<>(supportedBindings)).build();
    @Path("/bindings/{bindingId}/info")
    @Operation(operationId = "getDiscoveryServicesInfo", summary = "Gets information about the discovery services for a binding.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = DiscoveryInfoDTO.class))),
            @ApiResponse(responseCode = "404", description = "Discovery service not found") })
    public Response getDiscoveryServicesInfo(
            @PathParam("bindingId") @Parameter(description = "binding Id") final String bindingId,
        String label = null;
        String description = null;
        boolean supported = false;
        Set<DiscoveryService> discoveryServices = discoveryServiceRegistry.getDiscoveryServices(bindingId);
        if (discoveryServices.isEmpty()) {
            return JSONResponse.createResponse(Status.NOT_FOUND, null,
                    "No discovery service found for binding " + bindingId);
            if (discoveryService.isScanInputSupported()) {
                Bundle bundle = FrameworkUtil.getBundle(discoveryService.getClass());
                label = discoveryService.getScanInputLabel();
                    label = i18nProvider.getText(bundle, I18nUtil.stripConstant(label), label, locale);
                description = discoveryService.getScanInputDescription();
                    description = i18nProvider.getText(bundle, I18nUtil.stripConstant(description), description,
        return Response.ok(new DiscoveryInfoDTO(supported, label, description)).build();
    @Path("/bindings/{bindingId}/scan")
    @Operation(operationId = "scan", summary = "Starts asynchronous discovery process for a binding and returns the timeout in seconds of the discovery operation.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = Integer.class))),
    public Response scan(@PathParam("bindingId") @Parameter(description = "binding Id") final String bindingId,
            @QueryParam("input") @Parameter(description = "input parameter to start the discovery") @Nullable String input) {
        if (discoveryServiceRegistry.getDiscoveryServices(bindingId).isEmpty()) {
        discoveryServiceRegistry.startScan(bindingId, input, new ScanListener() {
                logger.error("Error occurred while scanning for binding '{}'", bindingId, exception);
                logger.debug("Scan for binding '{}' successfully finished.", bindingId);
        return Response.ok(discoveryServiceRegistry.getMaxScanTimeout(bindingId)).build();
