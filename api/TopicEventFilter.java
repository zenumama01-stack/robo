 * The {@link TopicEventFilter} is a default openHAB {@link EventFilter} implementation that ensures filtering
 * of events based on a single event topic or multiple event topics.
 * Thread-safe.
 * @author Florian Hotze - Add support for filtering of events by multiple event topics
public class TopicEventFilter implements EventFilter {
    private final List<Pattern> topicsRegexes;
     * Constructs a new topic event filter.
     * @param topicRegex the regular expression of a topic
     * @see <a href="https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/util/regex/Pattern.html">Java
     *      Regex</a>
    public TopicEventFilter(String topicRegex) {
        this.topicsRegexes = List.of(Pattern.compile(topicRegex));
     * @param topicsRegexes the regular expressions of multiple topics
     * @throws PatternSyntaxException indicate a syntax error in any of the regular-expression patterns
    public TopicEventFilter(List<String> topicsRegexes) throws PatternSyntaxException {
        List<Pattern> tmpTopicsRegexes = new ArrayList<>();
        for (String topicRegex : topicsRegexes) {
            tmpTopicsRegexes.add(Pattern.compile(topicRegex));
        this.topicsRegexes = Collections.unmodifiableList(tmpTopicsRegexes);
        return topicsRegexes.stream().anyMatch(p -> p.matcher(event.getTopic()).matches());
