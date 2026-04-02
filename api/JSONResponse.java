import java.io.BufferedWriter;
import com.google.gson.stream.JsonWriter;
 * Static helper methods to build up JSON-like Response objects and error handling.
 * @author Joerg Plewe - Initial contribution
 * @author Henning Treu - Provide streaming capabilities
 * @author Jörg Sautter - Improve streaming capabilities
public class JSONResponse {
    private static final JSONResponse INSTANCE = new JSONResponse();
    private final Gson gson = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS).create();
    static final String JSON_KEY_ERROR_MESSAGE = "message";
    static final String JSON_KEY_ERROR = "error";
    static final String JSON_KEY_HTTPCODE = "http-code";
    static final String JSON_KEY_ENTITY = "entity";
     * avoid instantiation apart from {@link #createResponse}.
    private JSONResponse() {
     * in case of error (404 and such)
     * @param errormessage
     * @return Response containing a status and the errormessage in JSON format
    public static Response createErrorResponse(Response.StatusType status, String errormessage) {
        return createResponse(status, null, errormessage);
     * Depending on the status, create a Response object containing either the entity alone or an error JSON
     * which might hold the entity as well.
     * @param status the status for the response.
     * @param entity the entity which is transformed into a JSON stream.
     * @return Response configure for error or success
    public static Response createResponse(Response.StatusType status, Object entity, String errormessage) {
        if (status.getFamily() != Response.Status.Family.SUCCESSFUL) {
            return INSTANCE.createErrorResponse(status, entity, errormessage);
        return INSTANCE.createResponse(status, entity);
     * basic configuration of a ResponseBuilder
     * @return ResponseBuilder configured for "Content-Type" MediaType.APPLICATION_JSON
    private ResponseBuilder responseBuilder(Response.StatusType status) {
        return Response.status(status).header("Content-Type", MediaType.APPLICATION_JSON);
     * setup JSON depending on the content
     * @param message a message (may be null)
     * @param ex
    private JsonElement createErrorJson(String message, Response.StatusType status, Object entity, Exception ex) {
        JsonObject resultJson = new JsonObject();
        JsonObject errorJson = new JsonObject();
        resultJson.add(JSON_KEY_ERROR, errorJson);
        errorJson.addProperty(JSON_KEY_ERROR_MESSAGE, message);
        // in case we have a http status code, report it
            errorJson.addProperty(JSON_KEY_HTTPCODE, status.getStatusCode());
        // in case there is an entity...
        if (entity != null) {
            // return the existing object
            resultJson.add(JSON_KEY_ENTITY, gson.toJsonTree(entity));
        // is there an exception?
        if (ex != null) {
            // JSONify the Exception
            JsonObject exceptionJson = new JsonObject();
            exceptionJson.addProperty("class", ex.getClass().getName());
            exceptionJson.addProperty("message", ex.getMessage());
            exceptionJson.addProperty("localized-message", ex.getLocalizedMessage());
            exceptionJson.addProperty("cause", null != ex.getCause() ? ex.getCause().getClass().getName() : null);
            errorJson.add("exception", exceptionJson);
        return resultJson;
    private Response createErrorResponse(Response.StatusType status, Object entity, String errormessage) {
        ResponseBuilder rp = responseBuilder(status);
        JsonElement errorJson = createErrorJson(errormessage, status, entity, null);
        rp.entity(errorJson);
        return rp.build();
    private Response createResponse(Response.StatusType status, final Object entity) {
        if (entity == null) {
        rp.entity((StreamingOutput) (target) -> {
            // target must not be closed, see javadoc of javax.ws.rs.ext.MessageBodyWriter
            JsonWriter jsonWriter = new JsonWriter(
                    new BufferedWriter(new OutputStreamWriter(target, StandardCharsets.UTF_8)));
            gson.toJson(entity, entity.getClass(), jsonWriter);
            jsonWriter.flush();
     * A piped input stream that is marked to produce JSON string.
    private static class PipedJSONInputStream extends PipedInputStream implements JSONInputStream {
        public PipedJSONInputStream(PipedOutputStream src) throws IOException {
            super(src);
     * trap exceptions
     * @author Joerg Plewe
    public static class ExceptionMapper implements javax.ws.rs.ext.ExceptionMapper<Exception> {
        private final Logger logger = LoggerFactory.getLogger(ExceptionMapper.class);
        public Response toResponse(Exception e) {
            logger.debug("Exception during REST handling.", e);
            Response.StatusType status = Response.Status.INTERNAL_SERVER_ERROR;
            // in case the Exception is a WebApplicationException, it already carries a Status
            if (e instanceof WebApplicationException exception) {
                status = exception.getResponse().getStatusInfo();
            JsonElement ret = INSTANCE.createErrorJson(e.getMessage(), status, null, e);
            return INSTANCE.responseBuilder(status).entity(INSTANCE.gson.toJson(ret)).build();
