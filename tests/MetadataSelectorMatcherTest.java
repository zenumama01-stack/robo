import static org.hamcrest.core.IsIterableContaining.hasItem;
 * Test the {@link MetadataSelectorMatcher}.
public class MetadataSelectorMatcherTest {
    private @NonNullByDefault({}) MetadataSelectorMatcher matcher;
    private @Mock @NonNullByDefault({}) MetadataRegistry metadataRegistryMock;
    public void setup() throws Exception {
        when(metadataRegistryMock.getAll())
                .thenReturn(List.of(new Metadata(new MetadataKey("magic", "test_item"), "test", Map.of()),
                        new Metadata(new MetadataKey("magic2", "test_item"), "test", Map.of()),
                        new Metadata(new MetadataKey("homekit", "test_item"), "test", Map.of()),
                        new Metadata(new MetadataKey("alexa", "test_item"), "test", Map.of())));
        when(metadataRegistryMock.isInternalNamespace(anyString())).thenReturn(false);
        matcher = new MetadataSelectorMatcher(metadataRegistryMock);
    public void nullSelectorShouldReturnEmptySet() {
        assertThat(matcher.filterNamespaces(null, null), is(Set.of()));
    public void emptySelectorShouldReturnEmptySet() {
        assertThat(matcher.filterNamespaces("", null), is(Set.of()));
    public void specificSelectorShouldReturnSpecificNamespace() {
        assertThat(matcher.filterNamespaces("alexa", null), hasSize(1));
        assertThat(matcher.filterNamespaces("alexa", null), hasItem("alexa"));
        assertThat(matcher.filterNamespaces("magic", null), hasSize(1));
        assertThat(matcher.filterNamespaces("magic", null), hasItem("magic"));
        assertThat(matcher.filterNamespaces("magic2", null), hasSize(1));
        assertThat(matcher.filterNamespaces("magic2", null), hasItem("magic2"));
        assertThat(matcher.filterNamespaces("unknown", null), hasSize(1));
    public void regularExpressionShouldMatchSubset() {
        assertThat(matcher.filterNamespaces(".*", null), hasSize(4));
        assertThat(matcher.filterNamespaces("magic.?", null), hasSize(2));
    public void nonConfigDescriptionSelectorShouldBeResult() {
        assertThat(matcher.filterNamespaces("magic, foo, bar", null), hasItems("magic", "foo", "bar"));
