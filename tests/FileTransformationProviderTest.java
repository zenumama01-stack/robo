import static org.hamcrest.Matchers.hasItem;
 * The {@link FileTransformationProviderTest} includes tests for the
 * {@link FileTransformationProvider}
public class FileTransformationProviderTest {
    private static final String FOO_TYPE = "foo";
    private static final String INITIAL_CONTENT = "initial";
    private static final Path INITIAL_FILENAME = Path.of(INITIAL_CONTENT + "." + FOO_TYPE);
    private static final Transformation INITIAL_CONFIGURATION = new Transformation(INITIAL_FILENAME.toString(),
            INITIAL_FILENAME.toString(), FOO_TYPE, Map.of(FUNCTION, INITIAL_CONTENT));
    private static final String ADDED_CONTENT = "added";
    private static final Path ADDED_FILENAME = Path.of(ADDED_CONTENT + "." + FOO_TYPE);
    private @Mock @NonNullByDefault({}) ProviderChangeListener<@NonNull Transformation> listenerMock;
    private @NonNullByDefault({}) FileTransformationProvider provider;
    private @TempDir @NonNullByDefault({}) Path configPath;
    private @NonNullByDefault({}) Path transformationPath;
        when(watchServiceMock.getWatchPath()).thenReturn(configPath);
        transformationPath = configPath.resolve(TransformationService.TRANSFORM_FOLDER_NAME);
        // create transformation directory and set initial content
        Files.createDirectories(transformationPath);
        Files.writeString(transformationPath.resolve(INITIAL_FILENAME), INITIAL_CONTENT);
        provider = new FileTransformationProvider(watchServiceMock);
        provider.addProviderChangeListener(listenerMock);
    public void testInitialConfigurationIsPresent() {
        // assert that initial configuration is present
        assertThat(provider.getAll(), contains(INITIAL_CONFIGURATION));
    public void testAddingConfigurationIsPropagated() throws IOException {
        Path path = transformationPath.resolve(ADDED_FILENAME);
        Files.writeString(path, ADDED_CONTENT);
        Transformation addedConfiguration = new Transformation(ADDED_FILENAME.toString(), ADDED_FILENAME.toString(),
                FOO_TYPE, Map.of(FUNCTION, ADDED_CONTENT));
        provider.processWatchEvent(CREATE, path);
        // assert registry is notified and internal cache updated
        Mockito.verify(listenerMock).added(provider, addedConfiguration);
        assertThat(provider.getAll(), hasItem(addedConfiguration));
    public void testUpdatingConfigurationIsPropagated() throws IOException {
        Path path = transformationPath.resolve(INITIAL_FILENAME);
        Files.writeString(path, "updated");
        Transformation updatedConfiguration = new Transformation(INITIAL_FILENAME.toString(),
                INITIAL_FILENAME.toString(), FOO_TYPE, Map.of(FUNCTION, "updated"));
        provider.processWatchEvent(MODIFY, path);
        Mockito.verify(listenerMock).updated(provider, INITIAL_CONFIGURATION, updatedConfiguration);
        assertThat(provider.getAll(), contains(updatedConfiguration));
        assertThat(provider.getAll(), not(contains(INITIAL_CONFIGURATION)));
    public void testDeletingConfigurationIsPropagated() {
        provider.processWatchEvent(DELETE, transformationPath.resolve(INITIAL_FILENAME));
        Mockito.verify(listenerMock).removed(provider, INITIAL_CONFIGURATION);
    public void testLanguageIsProperlyParsed() throws IOException {
        String fileName = "test_de." + FOO_TYPE;
        Path path = transformationPath.resolve(fileName);
        Files.writeString(path, INITIAL_CONTENT);
        Transformation expected = new Transformation(fileName, fileName, FOO_TYPE, Map.of(FUNCTION, INITIAL_CONTENT));
        assertThat(provider.getAll(), hasItem(expected));
    public void testMissingExtensionIsIgnored() throws IOException {
        Path extensionMissing = transformationPath.resolve("extensionMissing");
        Files.writeString(extensionMissing, INITIAL_CONTENT);
        provider.processWatchEvent(CREATE, extensionMissing);
        provider.processWatchEvent(MODIFY, extensionMissing);
        Mockito.verify(listenerMock, never()).added(any(), any());
        Mockito.verify(listenerMock, never()).updated(any(), any(), any());
    public void testIgnoredExtensionIsIgnored() throws IOException {
        Path extensionIgnored = transformationPath.resolve("extensionIgnore.txt");
        Files.writeString(extensionIgnored, INITIAL_CONTENT);
        provider.processWatchEvent(CREATE, extensionIgnored);
        provider.processWatchEvent(MODIFY, extensionIgnored);
