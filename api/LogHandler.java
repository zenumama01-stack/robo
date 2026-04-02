import java.util.concurrent.ConcurrentLinkedDeque;
@JaxrsName(LogHandler.PATH_LOG)
@Path(LogHandler.PATH_LOG)
@Tag(name = LogHandler.PATH_LOG)
public class LogHandler implements RESTResource {
    private final Logger logger = LoggerFactory.getLogger(LogHandler.class);
    public static final String PATH_LOG = "log";
    private static final String TEMPLATE_INTERNAL_ERROR = "{\"error\":\"%s\",\"severity\":\"%s\"}";
     * Rolling array to store the last LOG_BUFFER_LIMIT messages. Those can be fetched e.g. by a
     * diagnostic UI to display errors of other clients, where e.g. the logs are not easily accessible.
    private final ConcurrentLinkedDeque<LogMessage> logBuffer = new ConcurrentLinkedDeque<>();
     * Container for a log message
    public static class LogMessage {
        public @Nullable String severity;
        public @Nullable URL url;
        public @Nullable String message;
    @Path("/levels")
    @Operation(operationId = "getLogLevels", summary = "Get log severities, which are logged by the current logger settings.", responses = {
            @ApiResponse(responseCode = "200", description = "This depends on the current log settings at the backend.") })
    public Response getLogLevels() {
        return Response.ok(createLogLevelsMap()).build();
    @Operation(operationId = "getLastLogMessagesForFrontend", summary = "Returns the last logged frontend messages. The amount is limited to the "
            + LogConstants.LOG_BUFFER_LIMIT + " last entries.")
    public Response getLastLogs(@DefaultValue(LogConstants.LOG_BUFFER_LIMIT
            + "") @QueryParam("limit") @Parameter(name = "limit", schema = @Schema(implementation = Integer.class, minimum = "1", maximum = ""
                    + LogConstants.LOG_BUFFER_LIMIT)) @Nullable Integer limit) {
        if (logBuffer.isEmpty()) {
            return Response.ok("[]").build();
        int effectiveLimit;
        if (limit == null || limit <= 0 || limit > LogConstants.LOG_BUFFER_LIMIT) {
            effectiveLimit = logBuffer.size();
            effectiveLimit = limit;
        if (effectiveLimit >= logBuffer.size()) {
            return Response.ok(logBuffer.toArray()).build();
            final List<LogMessage> result = new ArrayList<>();
            Iterator<LogMessage> iter = logBuffer.descendingIterator();
                result.add(iter.next());
            } while (iter.hasNext() && result.size() < effectiveLimit);
            Collections.reverse(result);
    @Operation(operationId = "logMessageToBackend", summary = "Log a frontend log message to the backend.", responses = {
            @ApiResponse(responseCode = "403", description = LogConstants.LOG_SEVERITY_IS_NOT_SUPPORTED) })
    public Response log(
            final @Parameter(name = "logMessage", description = "Severity is required and can be one of error, warn, info or debug, depending on activated severities which you can GET at /logLevels.", example = "{\"severity\": \"error\", \"url\": \"http://example.org\", \"message\": \"Error message\"}") @Nullable LogMessage logMessage) {
        if (logMessage == null) {
            logger.debug("Received null log message model!");
            return Response.status(500)
                    .entity(String.format(TEMPLATE_INTERNAL_ERROR, LogConstants.LOG_HANDLE_ERROR, "ERROR")).build();
        logMessage.timestamp = Instant.now().toEpochMilli();
        if (!doLog(logMessage)) {
            return Response.status(403).entity(String.format(TEMPLATE_INTERNAL_ERROR,
                    LogConstants.LOG_SEVERITY_IS_NOT_SUPPORTED, logMessage.severity)).build();
        logBuffer.add(logMessage);
        if (logBuffer.size() > LogConstants.LOG_BUFFER_LIMIT) {
            logBuffer.pollLast(); // Remove last element of Deque
     * Executes the logging call.
     * @param logMessage
     * @return Falls if severity is not supported, true if successfully logged.
    private boolean doLog(LogMessage logMessage) {
        String severity = logMessage.severity;
        severity = severity != null ? severity.toLowerCase() : "";
            case "error":
                logger.error(LogConstants.FRONTEND_LOG_PATTERN, logMessage.url, logMessage.message);
            case "warn":
                logger.warn(LogConstants.FRONTEND_LOG_PATTERN, logMessage.url, logMessage.message);
            case "info":
                logger.info(LogConstants.FRONTEND_LOG_PATTERN, logMessage.url, logMessage.message);
            case "debug":
                logger.debug(LogConstants.FRONTEND_LOG_PATTERN, logMessage.url, logMessage.message);
     * Return map of currently logged messages. They can change at runtime.
    private Map<String, Boolean> createLogLevelsMap() {
        Map<String, Boolean> result = new HashMap<>();
        result.put("error", logger.isErrorEnabled());
        result.put("warn", logger.isWarnEnabled());
        result.put("info", logger.isInfoEnabled());
        result.put("debug", logger.isDebugEnabled());
