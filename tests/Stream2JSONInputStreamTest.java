 * Tests {@link Stream2JSONInputStream}.
public class Stream2JSONInputStreamTest {
    private static final Gson GSON = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_JSON_COMPAT).create();
    public void shouldReturnForEmptyStream() throws Exception {
        List<Object> emptyList = List.of();
        Stream2JSONInputStream collection2InputStream = new Stream2JSONInputStream(emptyList.stream());
        assertThat(inputStreamToString(collection2InputStream), is(GSON.toJson(emptyList)));
    public void shouldStreamSingleObjectToJSON() throws Exception {
        DummyObject dummyObject = new DummyObject("demoKey", "demoValue");
        List<DummyObject> dummyList = List.of(dummyObject);
        Stream2JSONInputStream collection2InputStream = new Stream2JSONInputStream(Stream.of(dummyObject));
        assertThat(JsonParser.parseString(inputStreamToString(collection2InputStream)),
                is(JsonParser.parseString(GSON.toJson(dummyList))));
    public void shouldStreamCollectionStreamToJSON() throws Exception {
        DummyObject dummyObject1 = new DummyObject("demoKey1", "demoValue1");
        DummyObject dummyObject2 = new DummyObject("demoKey2", "demoValue2");
        List<DummyObject> dummyCollection = List.of(dummyObject1, dummyObject2);
        Stream2JSONInputStream collection2InputStream = new Stream2JSONInputStream(dummyCollection.stream());
                is(JsonParser.parseString(GSON.toJson(dummyCollection))));
    private String inputStreamToString(InputStream in) throws IOException {
    private static class DummyObject {
        private final String value;
        DummyObject(String key, String value) {
