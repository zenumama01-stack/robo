import static org.openhab.core.OpenHAB.CONFIG_DIR_PROG_ARGUMENT;
import static org.openhab.core.service.WatchService.Kind.*;
import java.util.concurrent.ScheduledThreadPoolExecutor;
import org.mockito.InOrder;
import org.openhab.core.automation.module.script.ScriptEngineFactory;
import org.opentest4j.AssertionFailedError;
 * Test for {@link AbstractScriptFileWatcher}, covering differing start levels and dependency tracking
 * @author Jan N. Klug - Refactoring and improvements
class AbstractScriptFileWatcherTest extends JavaTest {
    private static final int DEFAULT_TEST_TIMEOUT_MS = 10000;
    private @NonNullByDefault({}) AbstractScriptFileWatcher scriptFileWatcher;
    private @Mock @NonNullByDefault({}) ScriptEngineManager scriptEngineManagerMock;
    private @Mock @NonNullByDefault({}) ScriptDependencyTracker scriptDependencyTrackerMock;
    private @Mock @NonNullByDefault({}) StartLevelService startLevelServiceMock;
    private @Mock @NonNullByDefault({}) ReadyService readyServiceMock;
    protected @TempDir @NonNullByDefault({}) Path tempScriptDir;
    private final AtomicInteger atomicInteger = new AtomicInteger();
    private int currentStartLevel = 0;
    public void setUp() {
        System.setProperty(CONFIG_DIR_PROG_ARGUMENT, tempScriptDir.toString());
        when(watchServiceMock.getWatchPath()).thenReturn(tempScriptDir);
        atomicInteger.set(0);
        currentStartLevel = 0;
        // ensure initialize is not called on initialization
        when(startLevelServiceMock.getStartLevel()).thenAnswer(invocation -> currentStartLevel);
        scriptFileWatcher.deactivate();
    public void testLoadOneDefaultFileAlreadyStarted() {
        scriptFileWatcher = createScriptFileWatcher(true);
        when(scriptEngineManagerMock.isSupported("js")).thenReturn(true);
        ScriptEngineContainer scriptEngineContainer = mock(ScriptEngineContainer.class);
        when(scriptEngineContainer.getScriptEngine()).thenReturn(mock(ScriptEngine.class));
        when(scriptEngineManagerMock.createScriptEngine(anyString(), anyString())).thenReturn(scriptEngineContainer);
        when(scriptEngineManagerMock.loadScript(any(), any())).thenReturn(true);
        updateStartLevel(100);
        Path p = getFile("script.js");
        scriptFileWatcher.processWatchEvent(CREATE, p);
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).createScriptEngine("js", p.toString());
    public void testSubDirectoryIncludedInInitialImport() {
        Path p0 = getFile("script.js");
        Path p1 = getFile("dir/script.js");
        awaitEmptyQueue();
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).createScriptEngine("js",
                ScriptFileReference.getScriptIdentifier(p0));
                ScriptFileReference.getScriptIdentifier(p1));
    public void testSubDirectoryIgnoredInInitialImport() {
        scriptFileWatcher = createScriptFileWatcher(false);
        verify(scriptEngineManagerMock, never()).createScriptEngine("js", ScriptFileReference.getScriptIdentifier(p1));
    public void testLoadOneDefaultFileWaitUntilStarted() {
        updateStartLevel(20);
        // verify not yet called
        verify(scriptEngineManagerMock, never()).createScriptEngine(anyString(), anyString());
        // verify is called when the start level increases
    public void testLoadOneCustomFileWaitUntilStarted() {
        updateStartLevel(50);
        Path p = getFile("script.sl60.js");
    public void testLoadTwoCustomFilesDifferentStartLevels() {
        Path p1 = getFile("script.sl70.js");
        Path p2 = getFile("script.sl50.js");
        scriptFileWatcher.processWatchEvent(CREATE, p1);
        scriptFileWatcher.processWatchEvent(CREATE, p2);
        updateStartLevel(40);
        updateStartLevel(60);
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).createScriptEngine("js", p2.toString());
        verify(scriptEngineManagerMock, never()).createScriptEngine(anyString(), eq(p1.toString()));
        updateStartLevel(80);
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).createScriptEngine("js", p1.toString());
    public void testLoadTwoCustomFilesAlternativePatternDifferentStartLevels() {
        Path p1 = getFile("sl70/script.js");
        Path p2 = getFile("sl50/script.js");
    public void testLoadOneDefaultFileDelayedSupport() {
        when(scriptEngineManagerMock.isSupported("js")).thenReturn(false);
        // verify not yet called but checked
        waitForAssert(() -> verify(scriptEngineManagerMock).isSupported(anyString()));
        // add support is added for .js files
        scriptFileWatcher.factoryAdded("js");
        // verify script has now been processed
    public void testOrderingWithinSingleStartLevel() {
        Path p64 = getFile("script.sl64.js");
        scriptFileWatcher.processWatchEvent(CREATE, p64);
        Path p66 = getFile("script.sl66.js");
        scriptFileWatcher.processWatchEvent(CREATE, p66);
        Path p65 = getFile("script.sl65.js");
        scriptFileWatcher.processWatchEvent(CREATE, p65);
        updateStartLevel(70);
        InOrder inOrder = inOrder(scriptEngineManagerMock);
        inOrder.verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).createScriptEngine("js",
                p64.toString());
                p65.toString());
                p66.toString());
    public void testOrderingStartlevelFolders() {
        Path p50 = getFile("a_script.js");
        scriptFileWatcher.processWatchEvent(CREATE, p50);
        Path p40 = getFile("sl40/b_script.js");
        scriptFileWatcher.processWatchEvent(CREATE, p40);
        Path p30 = getFile("sl30/script.js");
        scriptFileWatcher.processWatchEvent(CREATE, p30);
                p30.toString());
                p40.toString());
                p50.toString());
    public void testReloadActiveWhenDependencyChanged() {
        ScriptEngineFactory scriptEngineFactoryMock = mock(ScriptEngineFactory.class);
        when(scriptEngineFactoryMock.getDependencyTracker()).thenReturn(scriptDependencyTrackerMock);
        when(scriptEngineContainer.getFactory()).thenReturn(scriptEngineFactoryMock);
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS).times(1)).createScriptEngine("js",
                p.toString());
        scriptFileWatcher.onDependencyChange(p.toString());
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS).times(2)).createScriptEngine("js",
    public void testNotReloadInactiveWhenDependencyChanged() {
        updateStartLevel(10);
        verify(scriptEngineManagerMock, never()).createScriptEngine("js", p.toString());
    public void testRemoveBeforeReAdd() {
        String scriptIdentifier = ScriptFileReference.getScriptIdentifier(p);
                scriptIdentifier);
        scriptFileWatcher.processWatchEvent(MODIFY, p);
        inOrder.verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).removeEngine(scriptIdentifier);
    public void testDirectoryAdded() {
        Path p2 = getFile("dir/script2.js");
        Path d = p1.getParent();
        scriptFileWatcher.processWatchEvent(CREATE, d);
    public void testDirectoryAddedSubDirIncluded() {
        Path p2 = getFile("dir/sub/script.js");
                ScriptFileReference.getScriptIdentifier(p2));
    public void testDirectoryAddedSubDirIgnored() {
        verify(scriptEngineManagerMock, never()).createScriptEngine("js", ScriptFileReference.getScriptIdentifier(p2));
    public void testSortsAllFilesInNewDirectory() {
        Path p20 = getFile("dir/script.sl20.js");
        Path p10 = getFile("dir/script2.sl10.js");
        Path d = p10.getParent();
                p10.toString());
                p20.toString());
    public void testFileRemoved() {
        scriptFileWatcher.processWatchEvent(DELETE, p1);
        scriptFileWatcher.processWatchEvent(DELETE, p2);
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).removeEngine(p1.toString());
        verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS)).removeEngine(p2.toString());
    public void testScriptEngineRemovedOnFailedLoad() {
        when(scriptEngineManagerMock.loadScript(any(), any())).thenReturn(false);
        when(scriptEngineContainer.getIdentifier()).thenReturn(ScriptFileReference.getScriptIdentifier(p));
                ScriptFileReference.getScriptIdentifier(p));
        inOrder.verify(scriptEngineManagerMock, timeout(DEFAULT_TEST_TIMEOUT_MS))
                .loadScript(eq(ScriptFileReference.getScriptIdentifier(p)), any());
                .removeEngine(ScriptFileReference.getScriptIdentifier(p));
    public void testIfInitializedForEarlyInitialization() {
        CompletableFuture<?> initialized = scriptFileWatcher.ifInitialized();
        assertThat(initialized.isDone(), is(false));
        updateStartLevel(StartLevelService.STARTLEVEL_STATES);
        waitForAssert(() -> assertThat(initialized.isDone(), is(true)));
    public void testIfInitializedForLateInitialization() {
        when(startLevelServiceMock.getStartLevel()).thenReturn(StartLevelService.STARTLEVEL_RULEENGINE);
        AbstractScriptFileWatcher watcher = createScriptFileWatcher();
        watcher.activate();
        watcher.onReadyMarkerAdded(new ReadyMarker(StartLevelService.STARTLEVEL_MARKER_TYPE,
                Integer.toString(StartLevelService.STARTLEVEL_STATES)));
        waitForAssert(() -> assertThat(watcher.ifInitialized().isDone(), is(true)));
        watcher.deactivate();
    private Path getFile(String name) {
        Path tempFile = tempScriptDir.resolve(name);
            File parent = tempFile.getParent().toFile();
            if (!parent.exists() && !parent.mkdirs()) {
                fail("Failed to create parent directories");
            if (!tempFile.toFile().createNewFile()) {
                fail("Failed to create temp script file");
            throw new AssertionFailedError("Failed to create temp script file: " + e.getMessage());
        return Path.of(tempFile.toUri());
     * Increase the start level in steps of 10
     * @param level the target start-level
    private void updateStartLevel(int level) {
        while (currentStartLevel < level) {
            currentStartLevel += 10;
            scriptFileWatcher.onReadyMarkerAdded(
                    new ReadyMarker(StartLevelService.STARTLEVEL_MARKER_TYPE, Integer.toString(currentStartLevel)));
    private AbstractScriptFileWatcher createScriptFileWatcher() {
        return createScriptFileWatcher(false);
    private AbstractScriptFileWatcher createScriptFileWatcher(boolean watchSubDirectories) {
        return new AbstractScriptFileWatcher(watchServiceMock, scriptEngineManagerMock, readyServiceMock,
                startLevelServiceMock, "", watchSubDirectories) {
                return new CountingScheduledExecutor(atomicInteger);
    private void awaitEmptyQueue() {
        waitForAssert(() -> assertThat(atomicInteger.get(), is(0)));
    private static class CountingScheduledExecutor extends ScheduledThreadPoolExecutor {
        private final AtomicInteger counter;
        public CountingScheduledExecutor(AtomicInteger counter) {
            super(1);
            this.counter = counter;
        public Future<?> submit(@NonNullByDefault({}) Runnable runnable) {
            counter.getAndIncrement();
            Runnable wrappedRunnable = () -> {
                runnable.run();
                counter.getAndDecrement();
            return super.submit(wrappedRunnable);
