import javax.ws.rs.ClientErrorException;
import javax.ws.rs.ext.ExceptionMapper;
 * Trap exceptions.
public class JSONResponseExceptionMapper implements ExceptionMapper<Exception> {
    private final Logger logger = LoggerFactory.getLogger(JSONResponseExceptionMapper.class);
    private final ExceptionMapper<Exception> delegate = new JSONResponse.ExceptionMapper();
    public @Nullable Response toResponse(Exception e) {
        if (e instanceof IOException) {
            // Returning null results in a Response.Status.NO_CONTENT response.
        } else if (e instanceof ClientErrorException cee) {
            // see https://github.com/openhab/openhab-distro/issues/1616
            logger.debug("Requested resource not (yet) found", cee);
            return cee.getResponse();
        } else if (e instanceof IllegalArgumentException) {
            logger.debug("Invalid argument submitted for REST request", e);
            return JSONResponse.createErrorResponse(Response.Status.BAD_REQUEST, e.getMessage());
            logger.error("Unexpected exception occurred while processing REST request.", e);
            return delegate.toResponse(e);
