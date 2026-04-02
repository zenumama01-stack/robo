public class ConfigDispatcherFileWatcherTest {
    private @NonNullByDefault({}) ConfigDispatcherFileWatcher configDispatcherFileWatcher;
    private @Mock @NonNullByDefault({}) ConfigDispatcher configDispatcherMock;
    private @Mock @NonNullByDefault({}) WatchService watchService;
    private @TempDir @NonNullByDefault({}) Path tempDir;
    private @NonNullByDefault({}) Path cfgPath;
    private @NonNullByDefault({}) Path nonCfgPath;
        configDispatcherFileWatcher = new ConfigDispatcherFileWatcher(configDispatcherMock, watchService);
        verify(configDispatcherMock).processConfigFile(any());
        cfgPath = tempDir.resolve("myPath.cfg");
        nonCfgPath = tempDir.resolve("myPath");
        Files.createFile(cfgPath);
        Files.createFile(nonCfgPath);
    public void configurationFileCreated() throws IOException {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.CREATE, cfgPath);
        verify(configDispatcherMock).processConfigFile(cfgPath.toAbsolutePath().toFile());
    public void configurationFileModified() throws IOException {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.MODIFY, cfgPath);
    public void nonConfigurationFileCreated() {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.CREATE, nonCfgPath);
        verifyNoMoreInteractions(configDispatcherMock);
    public void nonConfigurationFileModified() {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.MODIFY, nonCfgPath);
    public void configurationFileRemoved() {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.DELETE, cfgPath);
        verify(configDispatcherMock).fileRemoved(cfgPath.toAbsolutePath().toString());
    public void nonConfigurationFileRemoved() {
        configDispatcherFileWatcher.processWatchEvent(WatchService.Kind.DELETE, nonCfgPath);
