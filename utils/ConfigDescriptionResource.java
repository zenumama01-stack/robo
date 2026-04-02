package org.openhab.core.io.rest.core.internal.config;
import javax.ws.rs.core.UriBuilder;
import org.openhab.core.io.rest.core.config.EnrichedConfigDescriptionDTOMapper;
import org.openhab.core.io.rest.core.config.EnrichedConfigDescriptionParameterDTO;
 * {@link ConfigDescriptionResource} provides access to {@link ConfigDescription}s via REST.
 * @author Chris Jackson - Modify response to use JSONResponse
@JaxrsName(ConfigDescriptionResource.PATH_CONFIG_DESCRIPTIONS)
@Path(ConfigDescriptionResource.PATH_CONFIG_DESCRIPTIONS)
@Tag(name = ConfigDescriptionResource.PATH_CONFIG_DESCRIPTIONS)
public class ConfigDescriptionResource implements RESTResource {
    public static final String PATH_CONFIG_DESCRIPTIONS = "config-descriptions";
    public ConfigDescriptionResource( //
    @Operation(operationId = "getConfigDescriptions", summary = "Gets all available config descriptions.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedConfigDescriptionParameterDTO.class)))) })
            @HeaderParam("Accept-Language") @Parameter(description = "language") @Nullable String language, //
            @QueryParam("scheme") @Parameter(description = "scheme filter") @Nullable String scheme) {
        Collection<ConfigDescription> configDescriptions = configDescriptionRegistry.getConfigDescriptions(locale);
        return Response.ok(new Stream2JSONInputStream(configDescriptions.stream()
                .filter(configDescription -> scheme == null || scheme.equals(configDescription.getUID().getScheme()))
                .map(EnrichedConfigDescriptionDTOMapper::map))).build();
    @Path("/{uri}")
    @Operation(operationId = "getConfigDescriptionByURI", summary = "Gets a config description by URI.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedConfigDescriptionParameterDTO.class))),
            @ApiResponse(responseCode = "400", description = "Invalid URI syntax"),
    public Response getByURI(
            @PathParam("uri") @Parameter(description = "uri") String uri) {
        URI uriObject = UriBuilder.fromPath(uri).build();
        ConfigDescription configDescription = configDescriptionRegistry.getConfigDescription(uriObject, locale);
        return configDescription != null
                ? JSONResponse.createResponse(Status.OK, EnrichedConfigDescriptionDTOMapper.map(configDescription),
                : JSONResponse.createErrorResponse(Status.NOT_FOUND, "Configuration not found: " + uri);
