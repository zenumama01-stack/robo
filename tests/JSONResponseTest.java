import static org.hamcrest.object.IsCompatibleType.typeCompatibleWith;
import java.util.concurrent.ThreadLocalRandom;
 * Tests {@link JSONResponse}.
public class JSONResponseTest {
    private static final String ENTITY_VALUE = "entityValue";
    private static final String ENTITY_JSON_VALUE = "\"" + ENTITY_VALUE + "\"";
    public void creatErrorShouldCreateErrorResponse() {
        Response errorResponse = JSONResponse.createErrorResponse(Status.INTERNAL_SERVER_ERROR, "error");
        assertThat(errorResponse.getMediaType(), is(equalTo(MediaType.APPLICATION_JSON_TYPE)));
        assertThat(errorResponse.getStatus(), is(500));
        JsonObject entity = ((JsonObject) errorResponse.getEntity()).get(JSONResponse.JSON_KEY_ERROR).getAsJsonObject();
        assertThat(entity.get(JSONResponse.JSON_KEY_ERROR_MESSAGE).getAsString(), is("error"));
        assertThat(entity.get(JSONResponse.JSON_KEY_HTTPCODE).getAsInt(), is(500));
    public void createMessageWithErrorStatusShouldCreateErrorResponse() {
        Response errorResponse = JSONResponse.createResponse(Status.INTERNAL_SERVER_ERROR, null, "error");
        assertThat(entity.get(JSONResponse.JSON_KEY_ENTITY), is(nullValue()));
    public void createMessageWithErrorStatusShouldCreateErrorResponseWithEntity() {
        Response errorResponse = JSONResponse.createResponse(Status.INTERNAL_SERVER_ERROR, ENTITY_VALUE, "error");
        assertThat(errorResponse.getMediaType(), is(MediaType.APPLICATION_JSON_TYPE));
        JsonObject resultJson = (JsonObject) errorResponse.getEntity();
        assertThat(resultJson.get(JSONResponse.JSON_KEY_ENTITY).getAsString(), is(ENTITY_VALUE));
        JsonObject errorJson = resultJson.get(JSONResponse.JSON_KEY_ERROR).getAsJsonObject();
        assertThat(errorJson.get(JSONResponse.JSON_KEY_ERROR_MESSAGE).getAsString(), is("error"));
        assertThat(errorJson.get(JSONResponse.JSON_KEY_HTTPCODE).getAsInt(), is(500));
    public void shouldCreateSuccessResponseWithStreamEntity() throws IOException {
        Response response = JSONResponse.createResponse(Status.OK, ENTITY_VALUE, null);
        assertThat(response.getMediaType(), is(MediaType.APPLICATION_JSON_TYPE));
        Object entity = response.getEntity();
        assertThat(entity.getClass(), is(typeCompatibleWith(StreamingOutput.class)));
            ((StreamingOutput) entity).write(buffer);
            assertThat(new String(buffer.toByteArray(), StandardCharsets.UTF_8), is(ENTITY_JSON_VALUE));
    public void shouldCreateSuccessResponseWithNullEntity() throws Exception {
        Response response = JSONResponse.createResponse(Status.ACCEPTED, null, null);
        assertThat(response.getStatus(), is(202));
        assertThat(entity, is(nullValue()));
    public void shouldCreateSuccessResponseWithLargeStreamEntity() throws IOException {
        Response response = JSONResponse.createResponse(Status.OK, new LargeEntity(), null);
            String largeEntityJSON = new String(buffer.toByteArray(), StandardCharsets.UTF_8);
            assertThat(largeEntityJSON, is(notNullValue()));
            assertTrue(largeEntityJSON.startsWith("{"));
            assertTrue(largeEntityJSON.endsWith("}"));
    private static final class LargeEntity {
        private List<BigDecimal> randoms = getRandoms();
        private List<BigDecimal> getRandoms() {
            List<BigDecimal> randoms = new ArrayList<>();
            for (int i = 0; i < 100000; i++) {
                randoms.add(new BigDecimal(ThreadLocalRandom.current().nextDouble()));
            return randoms;
