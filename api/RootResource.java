package org.openhab.core.io.rest.internal.resources;
import org.openhab.core.io.rest.internal.resources.beans.RootBean;
import org.openhab.core.io.rest.internal.resources.beans.RootBean.Links;
import org.osgi.service.jaxrs.runtime.JaxrsServiceRuntime;
import org.osgi.service.jaxrs.runtime.dto.ApplicationDTO;
import org.osgi.service.jaxrs.runtime.dto.ResourceDTO;
import org.osgi.service.jaxrs.runtime.dto.RuntimeDTO;
 * This class acts as an entry point / root resource for the REST API.
 * In good HATEOAS manner, it provides links to other offered resources.
 * The result is returned as JSON
@Component(configurationPid = "org.openhab.restroot")
@JaxrsName(RootResource.RESOURCE_NAME)
@Path("/")
@Tag(name = RootResource.RESOURCE_NAME)
public class RootResource implements RESTResource {
    public static final String RESOURCE_NAME = "root";
    private final Logger logger = LoggerFactory.getLogger(RootResource.class);
    private final JaxrsServiceRuntime runtime;
    public RootResource(final @Reference JaxrsServiceRuntime runtime, final @Reference LocaleProvider localeProvider,
            final @Reference UnitProvider unitProvider, final @Reference TimeZoneProvider timeZoneProvider) {
        this.runtime = runtime;
    @Operation(operationId = "getRoot", summary = "Gets information about the runtime, the API version and links to resources.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = RootBean.class))) })
    public Response getRoot(@Context UriInfo uriInfo) {
        // key: path, value: name (this way we could ensure that ever path is added only once).
        final Map<String, String> collectedLinks = new HashMap<>();
        final RuntimeDTO runtimeDTO = runtime.getRuntimeDTO();
        final RootBean bean = new RootBean(localeProvider, unitProvider, timeZoneProvider);
        for (final ApplicationDTO applicationDTO : runtimeDTO.applicationDTOs) {
            for (final ResourceDTO resourceDTO : applicationDTO.resourceDTOs) {
                // We are using the JAX-RS name per convention for the link type.
                // Let's skip names that begin with a dot (e.g. the generated ones) and empty ones.
                final String name = resourceDTO.name;
                if (name == null || name.isEmpty() || name.startsWith(".") || RESOURCE_NAME.equals(name)) {
                // The path is provided for every resource method by the respective info DTO.
                // We don't want to add every REST endpoint but just the "parent" one.
                // Per convention the name is similar to the path (without the leading "/") for openHAB REST
                // implementations.
                final URI uri = uriInfo.getBaseUriBuilder().path("/" + name).build();
                if (collectedLinks.put(uri.toASCIIString(), name) != null) {
                    logger.warn("Duplicate entry: {}", name);
        collectedLinks.forEach((path, name) -> {
            bean.links.add(new Links(name, path));
        return Response.ok(bean).build();
