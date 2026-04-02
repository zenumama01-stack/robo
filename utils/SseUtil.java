package org.openhab.core.io.rest.sse.internal.util;
 * Utility class containing helper methods for the SSE implementation.
 * @author Dennis Nobel - Changed EventBean
 * @author Markus Rathgeb - Don't depend on specific application but use APIs if possible
public class SseUtil {
    static final String TOPIC_VALIDATE_PATTERN = "(\\w*\\*?\\/?,?:?-?\\s*)*";
    public static EventDTO buildDTO(final Event event) {
        EventDTO dto = new EventDTO();
        dto.topic = event.getTopic();
        dto.type = event.getType();
        dto.payload = event.getPayload();
     * Creates a new {@link OutboundSseEvent} object containing an {@link EventDTO} created for the given {@link Event}.
     * @param eventBuilder the builder that should be used
     * @param event the event data transfer object
     * @return a new OutboundEvent
    public static OutboundSseEvent buildEvent(OutboundSseEvent.Builder eventBuilder, EventDTO event) {
        return eventBuilder.name("message") //
                .mediaType(MediaType.APPLICATION_JSON_TYPE) //
                .data(event) //
     * Validates the given topicFilter
     * @param topicFilter
     * @return true if the given input filter is empty or a valid topic filter string
    public static boolean isValidTopicFilter(@Nullable String topicFilter) {
        return topicFilter == null || topicFilter.isEmpty() || topicFilter.matches(TOPIC_VALIDATE_PATTERN);
     * Splits the given topicFilter at any commas (",") and for each token replaces any wildcards(*) with the regex
     * pattern (.*)
    public static List<String> convertToRegex(@Nullable String topicFilter) {
        List<String> filters = new ArrayList<>();
        if (topicFilter == null || topicFilter.isEmpty()) {
            filters.add(".*");
            StringTokenizer tokenizer = new StringTokenizer(topicFilter, ",");
            while (tokenizer.hasMoreElements()) {
                String regex = tokenizer.nextToken().trim().replace("*", ".*") + "$";
                filters.add(regex);
        return filters;
