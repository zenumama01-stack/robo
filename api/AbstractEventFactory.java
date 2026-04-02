 * The {@link AbstractEventFactory} defines an abstract implementation of the {@link EventFactory} interface. Subclasses
 * must implement the abstract method {@link #createEventByType(String, String, String, String)} in order to create
 * event instances based on the event type.
public abstract class AbstractEventFactory implements EventFactory {
    private final Set<String> supportedEventTypes;
    private static final Gson JSONCONVERTER = new GsonBuilder()
            .registerTypeAdapter(ZonedDateTime.class, new ZonedDateTimeAdapter()).create();
     * Must be called in subclass constructor to define the supported event types.
     * @param supportedEventTypes the supported event types
    protected AbstractEventFactory(Set<String> supportedEventTypes) {
        this.supportedEventTypes = Set.copyOf(supportedEventTypes);
    public Event createEvent(String eventType, String topic, String payload, @Nullable String source) throws Exception {
        assertValidArguments(eventType, topic, payload);
        if (!getSupportedEventTypes().contains(eventType)) {
            return createEventByType(eventType, topic, payload, source);
    public Set<String> getSupportedEventTypes() {
        return supportedEventTypes;
    private void assertValidArguments(String eventType, String topic, String payload) {
        checkNotNullOrEmpty(eventType, "eventType");
        checkNotNullOrEmpty(topic, "topic");
        checkNotNullOrEmpty(payload, "payload");
     * Create a new event instance based on the event type.
     * @param eventType the event type
     * @param source the source, can be null
     * @return the created event instance
     * @throws Exception if the creation of the event fails
    protected abstract Event createEventByType(String eventType, String topic, String payload, @Nullable String source)
            throws Exception;
     * Serializes the payload object into its equivalent Json representation.
     * @param payloadObject the payload object to serialize
     * @return a serialized Json representation
    protected static String serializePayload(Object payloadObject) {
        return JSONCONVERTER.toJson(payloadObject);
     * Deserializes the Json-payload into an object of the specified class.
     * @param payload the payload from which the object is to be deserialized
     * @param classOfPayload the class T of the payload object
     * @param <T> the type of the returned object
     * @return an object of type T from the payload
    protected static <T> T deserializePayload(String payload, Class<T> classOfPayload) {
        return JSONCONVERTER.fromJson(payload, classOfPayload);
     * Gets the elements of the topic (splitted by '/').
     * @return the topic elements
    protected String[] getTopicElements(String topic) {
        return topic.split("/");
    protected static void checkNotNull(@Nullable Object object, String argumentName) {
            throw new IllegalArgumentException("The argument '" + argumentName + "' must not be null.");
    protected static void checkNotNullOrEmpty(@Nullable String string, String argumentName) {
            throw new IllegalArgumentException("The argument '" + argumentName + "' must not be null or empty.");
    public static class ZonedDateTimeAdapter extends TypeAdapter<ZonedDateTime> {
        public void write(JsonWriter out, @Nullable ZonedDateTime value) throws IOException {
                out.nullValue();
                out.value(value.format(DateTimeFormatter.ISO_ZONED_DATE_TIME));
        public @Nullable ZonedDateTime read(JsonReader in) throws IOException {
            if (in.peek() == JsonToken.NULL) {
                in.nextNull();
            return ZonedDateTime.parse(in.nextString(), DateTimeFormatter.ISO_ZONED_DATE_TIME);
