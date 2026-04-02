import org.apache.karaf.log.core.Level;
import org.apache.karaf.log.core.LogService;
 * This class acts as a REST resource for changing logging configuration.
@JaxrsName(LoggerResource.PATH_LOGGING)
@Path(LoggerResource.PATH_LOGGING)
@Tag(name = LoggerResource.PATH_LOGGING)
public class LoggerResource implements RESTResource {
    public static final String PATH_LOGGING = "logging";
    private static final Set<String> LOG_LEVELS = Set.of(Level.strings());
    private static final Pattern BUNDLE_REGEX = Pattern.compile("\\w[\\w. -]*");
    private final LogService logService;
    public LoggerResource(@Reference LogService logService) {
        this.logService = logService;
    @Operation(operationId = "getLogger", summary = "Get all loggers", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = LoggerBean.class))) })
    public Response getLoggers(@Context UriInfo uriInfo) {
        final LoggerBean bean = new LoggerBean(logService.getLevel("ALL"));
    @Path("/{loggerName: \\w(%20|[\\w.-])*}")
    @Operation(operationId = "putLogger", summary = "Modify or add logger", responses = {
    public Response putLoggers(@PathParam("loggerName") @Parameter(description = "logger name") String loggerName,
            @Parameter(description = "logger", required = true) LoggerBean.@Nullable LoggerInfo logger,
            @Context UriInfo uriInfo) {
        if (logger == null || !BUNDLE_REGEX.matcher(logger.loggerName).matches() || !LOG_LEVELS.contains(logger.level)
                || !logger.loggerName.equals(loggerName)) {
            return Response.status(Response.Status.BAD_REQUEST).build();
        logService.setLevel(logger.loggerName, logger.level);
    @Operation(operationId = "getLogger", summary = "Get a single logger.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = LoggerBean.LoggerInfo.class))) })
    public Response getLogger(@PathParam("loggerName") @Parameter(description = "logger name") String loggerName,
        final LoggerBean bean = new LoggerBean(logService.getLevel(loggerName));
    @Operation(operationId = "removeLogger", summary = "Remove a single logger.", responses = {
    public Response removeLogger(@PathParam("loggerName") @Parameter(description = "logger name") String loggerName,
        logService.setLevel(loggerName, "DEFAULT");
