import org.osgi.service.log.LogReaderService;
 * The {@link LogWebSocketAdapter} allows subscription to log events over WebSocket
@Component(immediate = true, service = { WebSocketAdapter.class })
public class LogWebSocketAdapter implements WebSocketAdapter {
    public static final String ADAPTER_ID = "logs";
    private final Set<LogWebSocket> webSockets = new CopyOnWriteArraySet<>();
    private final LogReaderService logReaderService;
    public LogWebSocketAdapter(@Reference LogReaderService logReaderService) {
        this.logReaderService = logReaderService;
        webSockets.forEach(logReaderService::removeLogListener);
    public void registerListener(LogWebSocket logWebSocket) {
        webSockets.add(logWebSocket);
        logReaderService.addLogListener(logWebSocket);
    public void unregisterListener(LogWebSocket logWebSocket) {
        logReaderService.removeLogListener(logWebSocket);
        webSockets.remove(logWebSocket);
        return new LogWebSocket(gson, LogWebSocketAdapter.this);
    public Enumeration<LogEntry> getLog() {
        return logReaderService.getLog();
