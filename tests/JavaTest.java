import java.util.function.BooleanSupplier;
import ch.qos.logback.classic.Logger;
import ch.qos.logback.classic.spi.ILoggingEvent;
import ch.qos.logback.core.read.ListAppender;
 * {@link JavaTest} is an abstract base class for tests which are not necessarily based on OSGi.
public class JavaTest {
    protected static final int DFL_TIMEOUT = 10000;
    protected static final int DFL_SLEEP_TIME = 50;
    private final Map<Class<?>, Logger> interceptedLoggers = new HashMap<>();
    private final Map<Class<?>, ListAppender<ILoggingEvent>> interceptedLoggerAppenders = new HashMap<>();
    public enum LogLevel {
        TRACE(ch.qos.logback.classic.Level.TRACE),
        DEBUG(ch.qos.logback.classic.Level.DEBUG),
        INFO(ch.qos.logback.classic.Level.INFO),
        WARN(ch.qos.logback.classic.Level.WARN),
        ERROR(ch.qos.logback.classic.Level.ERROR);
        private final ch.qos.logback.classic.Level level;
        LogLevel(ch.qos.logback.classic.Level level) {
        public ch.qos.logback.classic.Level getLevel() {
     * Set up an intercepting logger for a class with a log level threshold
     * @param clazz The {@link Class} to intercept the logs for
     * @param minLogLevel The minimum log level that should be recorded
    protected void setupInterceptedLogger(Class<?> clazz, LogLevel minLogLevel) {
        Logger logger = (Logger) LoggerFactory.getLogger(clazz);
        logger.setLevel(minLogLevel.getLevel());
        ListAppender<ILoggingEvent> appender = new ListAppender<>();
        appender.start();
        logger.addAppender(appender);
        interceptedLoggers.put(clazz, logger);
        interceptedLoggerAppenders.put(clazz, appender);
     * Stop the logging interception
     * @param clazz The {@link Class} to stop intercepting logs for
    protected void stopInterceptedLogger(Class<?> clazz) {
        Logger logger = interceptedLoggers.get(clazz);
        ListAppender<ILoggingEvent> appender = interceptedLoggerAppenders.get(clazz);
        logger.detachAppender(appender);
     * Assert that not message was logged for a class
     * @param clazz The {@link Class} to check
    protected void assertNoLogMessage(Class<?> clazz) {
        if (appender == null) {
            Assertions.fail("Logger for class '" + clazz + "' not found.");
        if (!appender.list.isEmpty()) {
            Assertions.fail("Expected no log message for class '" + clazz + "', but found '" + appender.list + "'.");
     * Assert that a message was logged for a class with a given log level and message
     * @param logLevel The expected log level
     * @param message The expected message
    protected void assertLogMessage(Class<?> clazz, LogLevel logLevel, String message) {
        boolean isPresent = appender.list.stream()
                .anyMatch(e -> logLevel.getLevel().equals(e.getLevel()) && message.equals(e.getFormattedMessage()));
        if (!isPresent) {
            Assertions.fail("Expected '" + message + "' at level '" + logLevel + "' for class '" + clazz
                    + "', but found '" + appender.list + "'.");
     * Wait until the condition is fulfilled or the timeout is reached.
     * This method uses the default timing parameters.
     * @param condition the condition to check
     * @return true on success, false on timeout
    protected boolean waitFor(BooleanSupplier condition) {
        return waitFor(condition, DFL_TIMEOUT, DFL_SLEEP_TIME);
     * @param timeout timeout
     * @param sleepTime interval for checking the condition
    protected boolean waitFor(BooleanSupplier condition, long timeout, long sleepTime) {
        long waitingTime = 0;
        boolean rv;
        while (!(rv = condition.getAsBoolean()) && waitingTime < timeout) {
            waitingTime += sleepTime;
            internalSleep(sleepTime);
     * Wait until the assertion is fulfilled or the timeout is reached.
     * @param assertion closure that must not have an argument
    protected void waitForAssert(Runnable assertion) {
        waitForAssert(assertion, null, DFL_TIMEOUT, DFL_SLEEP_TIME);
     * @param assertion the logic to execute
    protected void waitForAssert(Runnable assertion, long timeout, long sleepTime) {
        waitForAssert(assertion, null, timeout, sleepTime);
     * @return the return value of the supplied assertion object's function on success
    protected <T> T waitForAssert(Supplier<T> assertion) {
        return waitForAssert(assertion, null, DFL_TIMEOUT, DFL_SLEEP_TIME);
    protected <T> T waitForAssert(Supplier<T> assertion, long timeout, long sleepTime) {
        return waitForAssert(assertion, null, timeout, sleepTime);
     * @param beforeLastCall logic to execute in front of the last call to ${code assertion}
    protected void waitForAssert(Runnable assertion, @Nullable Runnable beforeLastCall, long timeout, long sleepTime) {
        waitForAssert(assertion, beforeLastCall, null, timeout, sleepTime);
     * @param afterLastCall logic to execute after the last call to ${code assertion}
    protected void waitForAssert(Runnable assertion, @Nullable Runnable beforeLastCall,
            @Nullable Runnable afterLastCall, long timeout, long sleepTime) {
        while (waitingTime < timeout) {
                assertion.run();
                if (afterLastCall != null) {
                    afterLastCall.run();
            } catch (final Error error) {
        if (beforeLastCall != null) {
            beforeLastCall.run();
     * Useful for testing @NonNull annotated parameters
     * This method can be used if you want to check the behavior of a method if you supply null to a non-null marked
     * argument.
     * If you use null directly the compiler will raise an error. Using this method allows you to work around that
     * compiler check by using a null value that is marked as non-null.
     * @return null for testing purpose
    protected static <T> T giveNull() {
    @SuppressWarnings("PMD.AvoidCatchingNPE")
    private <T> T waitForAssert(Supplier<T> assertion, @Nullable Runnable beforeLastCall, long timeout,
            long sleepTime) {
        final long timeoutNs = TimeUnit.MILLISECONDS.toNanos(timeout);
        final long startingTime = System.nanoTime();
        while (System.nanoTime() - startingTime < timeoutNs) {
                return assertion.get();
            } catch (final Error | NullPointerException error) {
    private void internalSleep(long millis) {
            Thread.sleep(millis);
            throw new IllegalStateException("We shouldn't be interrupted while testing");
