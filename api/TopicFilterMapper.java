import static org.openhab.core.io.websocket.event.EventWebSocket.WEBSOCKET_EVENT_TYPE;
 * The {@link TopicFilterMapper} is used for mapping topic filter expression from the topic filter WebSocketEvent to
 * {@link TopicEventFilter}.
public final class TopicFilterMapper {
    private static final Pattern TOPIC_VALIDATE_PATTERN = Pattern.compile("^(\\w*\\*?/?)+$");
    private TopicFilterMapper() {
    private static String mapTopicToRegEx(String topic) {
        if (TOPIC_VALIDATE_PATTERN.matcher(topic).matches()) {
            // convert to regex: replace any wildcard (*) with the regex pattern (.*)
            return "^" + topic.trim().replace("*", ".*") + "$";
        // assume is already a regex
     * Maps the topic expressions to a {@link TopicEventFilter} for event inclusion.
     * @param topics the topic expressions
     * @return the {@link TopicEventFilter} or `null` if there are no inclusions defined
     * @throws EventProcessingException if a topic expression is invalid, i.e. neither a valid topic value, expression
     *             using the * wildcard or regular expression
    public static @Nullable TopicEventFilter mapTopicsToIncludeFilter(List<String> topics)
            throws EventProcessingException {
        List<String> includeTopics = topics.stream() //
                .filter(t -> !t.startsWith("!")) // include topics (expressions) only
                .map(TopicFilterMapper::mapTopicToRegEx) // map topics (expressions) to RegEx
        if (includeTopics.isEmpty()) {
        TopicEventFilter filter;
            filter = new TopicEventFilter(includeTopics);
            throw new EventProcessingException("Invalid topic expression in topic filter " + WEBSOCKET_EVENT_TYPE, e);
     * Maps the topic expressions to a {@link TopicEventFilter} for event exclusion.
     * @return the {@link TopicEventFilter} or `null` if there are no exclusions defined
    public static @Nullable TopicEventFilter mapTopicsToExcludeFilter(List<String> topics)
        List<String> excludeTopics = topics.stream() //
                .filter(t -> t.startsWith("!")) // exclude topics (expressions) only
                .map(t -> t.substring(1)) // remove the exclamation mark
        if (excludeTopics.isEmpty()) {
            filter = new TopicEventFilter(excludeTopics);
