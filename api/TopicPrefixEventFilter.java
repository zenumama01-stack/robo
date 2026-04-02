 * The {@link TopicPrefixEventFilter} is a default openHAB {@link EventFilter} implementation that ensures filtering
 * of events based on the prefix of an event topic.
public class TopicPrefixEventFilter implements EventFilter {
    private final String topicPrefix;
     * @param topicPrefix the prefix event topics must start with
    public TopicPrefixEventFilter(String topicPrefix) {
        this.topicPrefix = topicPrefix;
        return event.getTopic().startsWith(topicPrefix);
