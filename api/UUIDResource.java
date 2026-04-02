package org.openhab.core.id.internal;
import org.openhab.core.id.InstanceUUID;
 * This class acts as a REST resource for accessing the UUID of the instance
@JaxrsName(UUIDResource.PATH_UUID)
@Path(UUIDResource.PATH_UUID)
@RolesAllowed({ Role.ADMIN, Role.USER })
@Tag(name = UUIDResource.PATH_UUID)
public class UUIDResource implements RESTResource {
    public static final String PATH_UUID = "uuid";
     * Retrieves the instance UUID via REST endpoint.
     * This method exposes the unique instance identifier through a REST API endpoint.
     * The UUID is generated once and persisted, remaining constant across restarts.
     * @return a Response containing the instance UUID as plain text, or null if the UUID cannot be retrieved
    @Operation(operationId = "getUUID", summary = "A unified unique id.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = String.class))) })
    public Response getInstanceUUID() {
        return Response.ok(InstanceUUID.get()).build();
