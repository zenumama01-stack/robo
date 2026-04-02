import org.openhab.core.io.rest.internal.resources.beans.SystemInfoBean;
import org.openhab.core.io.rest.internal.resources.beans.UoMInfoBean;
import org.osgi.service.cm.ConfigurationEvent;
import org.osgi.service.cm.ConfigurationListener;
 * This class acts as a REST resource for system information.
@JaxrsName(SystemInfoResource.PATH_SYSTEMINFO)
@Path(SystemInfoResource.PATH_SYSTEMINFO)
@Tag(name = SystemInfoResource.PATH_SYSTEMINFO)
public class SystemInfoResource implements RESTResource, ConfigurationListener {
    public static final String PATH_SYSTEMINFO = "systeminfo";
    public SystemInfoResource(@Reference StartLevelService startLevelService, @Reference UnitProvider unitProvider) {
    public void configurationEvent(@Nullable ConfigurationEvent event) {
        if (Objects.equals(event.getPid(), "org.openhab.i18n")) {
            lastModified = null;
    @Operation(operationId = "getSystemInformation", summary = "Gets information about the system.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = SystemInfoBean.class))) })
    public Response getSystemInfo(@Context UriInfo uriInfo) {
        final SystemInfoBean bean = new SystemInfoBean(startLevelService.getStartLevel());
    @Path("/uom")
    @Operation(operationId = "getUoMInformation", summary = "Get all supported dimensions and their system units.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = UoMInfoBean.class))) })
    public Response getUoMInfo(final @Context Request request, final @Context UriInfo uriInfo) {
        final UoMInfoBean bean = new UoMInfoBean(unitProvider);
        return Response.ok(bean).lastModified(lastModified).cacheControl(RESTConstants.CACHE_CONTROL).build();
