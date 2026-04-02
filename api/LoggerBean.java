 * This is a java bean that is used to define logger settings for the REST interface.
public class LoggerBean {
    public final List<LoggerInfo> loggers;
    public static class LoggerInfo {
        public final String loggerName;
        public final String level;
        public LoggerInfo(String loggerName, String level) {
    public LoggerBean(Map<String, String> logLevels) {
        loggers = logLevels.entrySet().stream().map(l -> new LoggerInfo(l.getKey(), l.getValue())).toList();
