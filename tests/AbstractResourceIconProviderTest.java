 * Tests for {@link AbstractResourceIconProvider}.
public class AbstractResourceIconProviderTest {
    private @NonNullByDefault({}) IconProvider provider;
    private @Mock @NonNullByDefault({}) TranslationProvider i18nProviderMock;
        provider = new AbstractResourceIconProvider(i18nProviderMock) {
            protected @Nullable InputStream getResource(String iconset, String resourceName) {
                return switch (resourceName) {
                    case "x-30.png" -> new ByteArrayInputStream("x-30.png".getBytes());
                    case "x-y z.png" -> new ByteArrayInputStream("x-y z.png".getBytes());
                    case "a-bb-ccc-30.png" -> new ByteArrayInputStream("a-bb-ccc-30.png".getBytes());
                    case "a-bb-ccc-y z.png" -> new ByteArrayInputStream("a-bb-ccc-y z.png".getBytes());
            protected boolean hasResource(String iconset, String resourceName) {
                String state = substringAfterLast(resourceName, "-");
                state = substringBeforeLast(state, ".");
                return "30".equals(state) || "y z".equals(state);
            public Integer getPriority() {
    public void testScanningForState() throws IOException {
        try (InputStream is = provider.getIcon("x", "classic", "34", Format.PNG)) {
            assertNotNull(is);
            assertThat(new String(is.readAllBytes(), StandardCharsets.UTF_8), is("x-30.png"));
        try (InputStream is = provider.getIcon("x", "classic", "25", Format.PNG)) {
            assertNull(is);
    public void testScanningIconWithHyphensForState() throws IOException {
        try (InputStream is = provider.getIcon("a-bb-ccc", "classic", "34", Format.PNG)) {
            assertThat(new String(is.readAllBytes(), StandardCharsets.UTF_8), is("a-bb-ccc-30.png"));
        try (InputStream is = provider.getIcon("a-bb-ccc", "classic", "25", Format.PNG)) {
    public void testWithQuantityTypeState() throws IOException {
        try (InputStream is = provider.getIcon("x", "classic", "34 °C", Format.PNG)) {
    public void testIconWithHyphensWithQuantityTypeState() throws IOException {
        try (InputStream is = provider.getIcon("a-bb-ccc", "classic", "34 °C", Format.PNG)) {
    public void testWithStringTypeState() throws IOException {
        try (InputStream is = provider.getIcon("x", "classic", "y z", Format.PNG)) {
            assertThat(new String(is.readAllBytes(), StandardCharsets.UTF_8), is("x-y z.png"));
    public void testIconWithHyphensWithStringTypeState() throws IOException {
        try (InputStream is = provider.getIcon("a-bb-ccc", "classic", "y z", Format.PNG)) {
            assertThat(new String(is.readAllBytes(), StandardCharsets.UTF_8), is("a-bb-ccc-y z.png"));
