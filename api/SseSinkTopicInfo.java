 * The specific information we need to hold for a SSE sink which subscribes to event topics.
public class SseSinkTopicInfo {
    private final List<String> regexFilters;
    public SseSinkTopicInfo(String topicFilter) {
        this.regexFilters = SseUtil.convertToRegex(topicFilter);
    public static Predicate<SseSinkTopicInfo> matchesTopic(final String topic) {
        return info -> info.regexFilters.stream().anyMatch(topic::matches);
