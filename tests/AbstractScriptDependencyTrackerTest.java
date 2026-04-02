import static org.mockito.ArgumentMatchers.eq;
import org.junit.jupiter.api.io.TempDir;
 * The {@link AbstractScriptDependencyTrackerTest} contains tests for the {@link AbstractScriptDependencyTracker}
public class AbstractScriptDependencyTrackerTest {
    private static final String WATCH_DIR = "test";
    private static final Path DEPENDENCY = Path.of("depFile");
    private static final Path DEPENDENCY2 = Path.of("depFile2");
    private @NonNullByDefault({}) AbstractScriptDependencyTracker scriptDependencyTracker;
    private @Mock @NonNullByDefault({}) WatchService watchServiceMock;
    private @TempDir @NonNullByDefault({}) Path rootWatchPath;
    private @NonNullByDefault({}) Path depPath;
    private @NonNullByDefault({}) Path depPath2;
        when(watchServiceMock.getWatchPath()).thenReturn(rootWatchPath);
        scriptDependencyTracker = new AbstractScriptDependencyTracker(watchServiceMock, WATCH_DIR) {
        depPath = rootWatchPath.resolve(WATCH_DIR).resolve(DEPENDENCY);
        depPath2 = rootWatchPath.resolve(WATCH_DIR).resolve(DEPENDENCY2);
        Files.createFile(depPath);
        Files.createFile(depPath2);
        scriptDependencyTracker.deactivate();
    public void testScriptLibraryWatcherIsCreatedAndActivated() {
        verify(watchServiceMock).registerListener(eq(scriptDependencyTracker), eq(rootWatchPath.resolve(WATCH_DIR)));
    public void testScriptLibraryWatchersIsDeactivatedOnShutdown() {
        verify(watchServiceMock).unregisterListener(eq(scriptDependencyTracker));
    public void testDependencyChangeIsForwardedToMultipleListeners() throws IOException {
        ScriptDependencyTracker.Listener listener1 = mock(ScriptDependencyTracker.Listener.class);
        ScriptDependencyTracker.Listener listener2 = mock(ScriptDependencyTracker.Listener.class);
        scriptDependencyTracker.addChangeTracker(listener1);
        scriptDependencyTracker.addChangeTracker(listener2);
        scriptDependencyTracker.startTracking("scriptId", depPath.toString());
        scriptDependencyTracker.processWatchEvent(WatchService.Kind.CREATE, depPath);
        verify(listener1).onDependencyChange(eq("scriptId"));
        verify(listener2).onDependencyChange(eq("scriptId"));
        verifyNoMoreInteractions(listener1);
        verifyNoMoreInteractions(listener2);
    public void testDependencyChangeIsForwardedForMultipleScriptIds() {
        ScriptDependencyTracker.Listener listener = mock(ScriptDependencyTracker.Listener.class);
        scriptDependencyTracker.addChangeTracker(listener);
        scriptDependencyTracker.startTracking("scriptId1", depPath.toString());
        scriptDependencyTracker.startTracking("scriptId2", depPath.toString());
        scriptDependencyTracker.processWatchEvent(WatchService.Kind.MODIFY, depPath);
        verify(listener).onDependencyChange(eq("scriptId1"));
        verify(listener).onDependencyChange(eq("scriptId2"));
        verifyNoMoreInteractions(listener);
    public void testDependencyChangeIsForwardedForMultipleDependencies() {
        scriptDependencyTracker.startTracking("scriptId", depPath2.toString());
        scriptDependencyTracker.processWatchEvent(WatchService.Kind.DELETE, depPath2);
        verify(listener, times(2)).onDependencyChange(eq("scriptId"));
    public void testDependencyChangeIsForwardedForCorrectDependencies() {
        scriptDependencyTracker.startTracking("scriptId2", depPath2.toString());
