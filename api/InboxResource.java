 * This class acts as a REST resource for the inbox and is registered with the
 * @author Kai Kreuzer - refactored for using the OSGi JAX-RS connector and
 *         removed ThingSetupManager
 * @author Chris Jackson - Updated to use JSONResponse. Fixed null response from
 *         approve. Improved error reporting.
 * @author Laurent Garnier - Added optional parameter newThingId to approve API
@Component(service = { RESTResource.class, InboxResource.class })
@JaxrsName(InboxResource.PATH_INBOX)
@Path(InboxResource.PATH_INBOX)
@Tag(name = InboxResource.PATH_INBOX)
public class InboxResource implements RESTResource {
    private final Logger logger = LoggerFactory.getLogger(InboxResource.class);
    public static final String PATH_INBOX = "inbox";
    public InboxResource(final @Reference Inbox inbox) {
    @Path("/{thingUID}/approve")
    @Operation(operationId = "approveInboxItemById", summary = "Approves the discovery result by adding the thing to the registry.", responses = {
            @ApiResponse(responseCode = "400", description = "Invalid new thing ID."),
            @ApiResponse(responseCode = "404", description = "Thing unable to be approved."),
            @ApiResponse(responseCode = "409", description = "No binding found that supports this thing.") })
    public Response approve(
            @PathParam("thingUID") @Parameter(description = "thingUID") String thingUID,
            @Parameter(description = "thing label") @Nullable String label,
            @QueryParam("newThingId") @Parameter(description = "new thing ID") @Nullable String newThingId) {
        ThingUID thingUIDObject = new ThingUID(thingUID);
        String notEmptyLabel = label != null && !label.isEmpty() ? label : null;
        String notEmptyNewThingId = newThingId != null && !newThingId.isEmpty() ? newThingId : null;
        Thing thing;
            thing = inbox.approve(thingUIDObject, notEmptyLabel, notEmptyNewThingId);
            logger.error("Thing {} unable to be approved: {}", thingUID, e.getLocalizedMessage());
            String errMsg = e.getMessage();
            return errMsg != null
                    && (errMsg.contains("must not contain multiple segments") || errMsg.startsWith("Invalid thing UID"))
                            ? JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Invalid new thing ID.")
                            : JSONResponse.createErrorResponse(Status.NOT_FOUND, "Thing unable to be approved.");
        // inbox.approve returns null if no handler is found that supports this thing
            return JSONResponse.createErrorResponse(Status.CONFLICT, "No binding found that can create the thing");
    @Operation(operationId = "removeItemFromInbox", summary = "Removes the discovery result from the inbox.", responses = {
            @ApiResponse(responseCode = "404", description = "Discovery result not found in the inbox.") })
    public Response delete(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID) {
        if (inbox.remove(new ThingUID(thingUID))) {
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Thing not found in inbox");
    @Operation(operationId = "getDiscoveredInboxItems", summary = "Get all discovered things.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = DiscoveryResultDTO.class)))) })
            @QueryParam("includeIgnored") @DefaultValue("true") @Parameter(description = "If true, include ignored inbox entries. Defaults to true") boolean includeIgnored) {
        Stream<DiscoveryResult> discoveryStream = inbox.getAll().stream();
        if (!includeIgnored) {
            discoveryStream = discoveryStream
                    .filter(discoveryResult -> discoveryResult.getFlag() != DiscoveryResultFlag.IGNORED);
        return Response.ok(new Stream2JSONInputStream(discoveryStream.map(DiscoveryResultDTOMapper::map))).build();
    @Path("/{thingUID}/ignore")
    @Operation(operationId = "flagInboxItemAsIgnored", summary = "Flags a discovery result as ignored for further processing.", responses = {
            @ApiResponse(responseCode = "200", description = "OK") })
    public Response ignore(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID) {
        inbox.setFlag(new ThingUID(thingUID), DiscoveryResultFlag.IGNORED);
    @Path("/{thingUID}/unignore")
    @Operation(operationId = "removeIgnoreFlagOnInboxItem", summary = "Removes ignore flag from a discovery result.", responses = {
    public Response unignore(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID) {
        inbox.setFlag(new ThingUID(thingUID), DiscoveryResultFlag.NEW);
