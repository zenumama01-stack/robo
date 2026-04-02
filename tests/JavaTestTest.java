 * Testing the test. Test suite for the JavaTest base test class.
 * @author Henning Treu - Initial contribution.
public class JavaTestTest {
    private @NonNullByDefault({}) JavaTest javaTest;
        javaTest = new JavaTest();
    public void waitForAssertShouldRunAfterLastCallWhenAssertionSucceeds() {
        Runnable afterLastCall = mock(Runnable.class);
        javaTest.waitForAssert(() -> assertTrue(true), null, afterLastCall, 100, 50);
        verify(afterLastCall, times(1)).run();
    public void waitForAssertShouldRunAfterLastCallWhenAssertionFails() {
            javaTest.waitForAssert(() -> fail(), null, afterLastCall, 100, 50);
        } catch (final AssertionError ex) {
    public void waitForAssertShouldNotCatchNPE() {
        assertThrows(NullPointerException.class, () -> {
            javaTest.waitForAssert(() -> {
                Map.of().get("key").toString();
    public void interceptedLoggerShouldNotLogBelowAboveMinLevel() {
        javaTest.setupInterceptedLogger(LogTest.class, JavaTest.LogLevel.INFO);
        LogTest logTest = new LogTest();
        logTest.logDebug("debug message");
        javaTest.stopInterceptedLogger(LogTest.class);
        Assertions.assertThrows(AssertionError.class,
                () -> javaTest.assertLogMessage(LogTest.class, JavaTest.LogLevel.DEBUG, "debug message"));
    public void interceptedLoggerShouldLogAboveMinLevel() {
        logTest.logError("error message");
        javaTest.assertLogMessage(LogTest.class, JavaTest.LogLevel.ERROR, "error message");
    private static class LogTest {
        private final Logger logger = LoggerFactory.getLogger(LogTest.class);
        public LogTest() {
        public void logDebug(String message) {
            logger.debug(message);
        public void logError(String message) {
            logger.error(message);
