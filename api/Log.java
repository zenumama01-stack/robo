 * This allows a script to log to the SLF4J-Log.
public class Log {
    private static final String LOGGER_NAME_PREFIX = "org.openhab.core.model.script.";
     * Creates the Log-Entry <code>format</code> with level <code>DEBUG</code> and logs under the loggers name
     * <code>org.openhab.core.model.script.&lt;loggerName&gt;</code>
     * @param loggerName the name of the Logger which is prefixed with <code>org.openhab.core.model.script.</code>
     * @param format the Log-Statement which can contain placeholders '<code>{}</code>'
     * @param args the arguments to replace the placeholders contained in <code>format</code>
     * @see Logger
    public static void logDebug(String loggerName, String format, Object... args) {
        LoggerFactory.getLogger(LOGGER_NAME_PREFIX.concat(loggerName)).debug(format, args);
     * Creates the Log-Entry <code>format</code> with level <code>INFO</code> and logs under the loggers name
    public static void logInfo(String loggerName, String format, Object... args) {
        LoggerFactory.getLogger(LOGGER_NAME_PREFIX.concat(loggerName)).info(format, args);
     * Creates the Log-Entry <code>format</code> with level <code>WARN</code> and logs under the loggers name
    public static void logWarn(String loggerName, String format, Object... args) {
        LoggerFactory.getLogger(LOGGER_NAME_PREFIX.concat(loggerName)).warn(format, args);
     * Creates the Log-Entry <code>format</code> with level <code>ERROR</code> and logs under the loggers name
    public static void logError(String loggerName, String format, Object... args) {
        LoggerFactory.getLogger(LOGGER_NAME_PREFIX.concat(loggerName)).error(format, args);
