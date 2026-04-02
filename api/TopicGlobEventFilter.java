import java.nio.file.PathMatcher;
 * The {@link TopicGlobEventFilter} is a default openHAB {@link EventFilter} implementation that ensures filtering
 * of events based on an event topic.
 * The syntax for the filter is the glob syntax documented at
 * https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/nio/file/FileSystem.html#getPathMatcher(java.lang.String)
public class TopicGlobEventFilter implements EventFilter {
    private final PathMatcher topicMatcher;
     * @param topicGlob the glob
     * @see <a href=
     *      "https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/nio/file/FileSystem.html#getPathMatcher(java.lang.String)">Java
     *      Glob</a>
    public TopicGlobEventFilter(String topicGlob) {
        this.topicMatcher = FileSystems.getDefault().getPathMatcher("glob:" + topicGlob);
        return topicMatcher.matches(Path.of(event.getTopic()));
