package org.openhab.core.io.websocket.log;
import org.osgi.service.log.LogLevel;
 * The {@link LogDTO} is used for serialization and deserialization of log messages
 * @author Chris Jackson - Add sequence and make Comparable based on sequence
public class LogDTO implements Comparable<LogDTO> {
    public String loggerName;
    public LogLevel level;
    public Date timestamp;
    public long unixtime;
    public String message;
    public String stackTrace;
    public long sequence;
    public LogDTO(long sequence, String loggerName, LogLevel level, long unixtime, String message, String stackTrace) {
        this.sequence = sequence;
        this.loggerName = loggerName;
        this.level = level;
        this.timestamp = new Date(unixtime);
        this.unixtime = unixtime;
        this.stackTrace = stackTrace;
    public int compareTo(LogDTO o) {
        return Long.compare(sequence, o.sequence);
