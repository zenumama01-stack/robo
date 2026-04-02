import org.osgi.service.log.LogEntry;
import org.osgi.service.log.LogListener;
 * The {@link LogWebSocket} is the WebSocket implementation for logs.
 * This supports sending of history, and provides a method of managing message cadence.
 * When a client connects, it must send a filter request before the server will send any logs. This triggers the sending
 * of history.
 * Live logs are sent as individual messages if they are received with sufficient spacing. When logs come in very
 * quickly, they are clustered together and sent as an array after up to 100mS.
 * @author Chris Jackson - Add history and improve performance using arrays
public class LogWebSocket implements LogListener, WriteCallback {
    private static final TypeToken<List<String>> STRING_LIST_TYPE = (TypeToken<List<String>>) TypeToken
            .getParameterized(List.class, String.class);
    private static final int SEND_PERIOD = 100; // Minimum allowable time between log packets (in milliseconds)
    private static final long FIRST_SEQUENCE = 0;
    private final Logger logger = LoggerFactory.getLogger(LogWebSocket.class);
    private final LogWebSocketAdapter wsAdapter;
    private @Nullable ScheduledExecutorService scheduledExecutorService;
    private @Nullable ScheduledFuture<?> commitScheduledFuture;
    private volatile long lastSentTime;
    /** Indicates that sending of live logs is suspended */
    private volatile boolean suspended;
    private List<LogDTO> deferredLogs = new ArrayList<>();
    private long lastSequence = FIRST_SEQUENCE;
    private final List<Pattern> loggerPatterns = new CopyOnWriteArrayList<>();
    public LogWebSocket(Gson gson, LogWebSocketAdapter wsAdapter) {
        stopDeferredScheduledFuture();
                enabled = false;
            this.deferredLogs.clear();
            if (this.scheduledExecutorService != null) {
                this.scheduledExecutorService.shutdownNow();
            this.scheduledExecutorService = null;
    public synchronized void onConnect(Session session) {
        this.remoteEndpoint = session.getRemote();
        InetSocketAddress isa = session.getRemoteAddress();
        String name = isa == null ? "websocket-logger"
                : "websocket-logger-" + isa.getHostString() + ':' + isa.getPort();
        this.scheduledExecutorService = Executors.newSingleThreadScheduledExecutor(ThreadFactoryBuilder.create()
                .withNamePrefix("OH").withName(name).withUncaughtExceptionHandler((t, e) -> {
        // Detect empty message (keepalive) and ignore
        if ("{}".equals(message)) {
            // Defer sending live logs while we process the history
            suspended = true;
            // Enable log messages
                enabled = true;
            suspended = false;
        LogFilterDTO logFilterDto;
            logFilterDto = gson.fromJson(message, LogFilterDTO.class);
            logger.warn("Failed to parse '{}' to a valid log filter object", message);
            flush();
        if (!loggerPatterns.isEmpty()) {
            loggerPatterns.clear();
        List<String> loggerNames;
        if (logFilterDto != null && (loggerNames = logFilterDto.loggerNames) != null) {
            List<Pattern> filters = loggerNames.stream().map(Pattern::compile).toList();
            if (!filters.isEmpty()) {
                loggerPatterns.addAll(filters);
        Long timeStart;
        Long timeStop;
        if (logFilterDto != null && logFilterDto.timeStart != null) {
            timeStart = logFilterDto.timeStart;
            timeStart = Long.MIN_VALUE;
        if (logFilterDto != null && logFilterDto.timeStop != null) {
            timeStop = logFilterDto.timeStop;
            timeStop = Long.MAX_VALUE;
        Long sequenceStart;
        if (logFilterDto != null && logFilterDto.sequenceStart != null) {
            sequenceStart = logFilterDto.sequenceStart;
                sequenceStart = lastSequence;
        List<LogEntry> logs = new ArrayList<>();
        for (Enumeration<LogEntry> history = wsAdapter.getLog(); history.hasMoreElements();) {
            logs.add(history.nextElement());
        if (logs.isEmpty()) {
        Predicate<LogEntry> withinTimeRange = log -> (log.getTime() >= timeStart) && (log.getTime() <= timeStop);
        Predicate<LogEntry> withinSequence = log -> log.getSequence() > sequenceStart;
        Predicate<LogEntry> nameMatchesAnyPattern = log -> loggerPatterns.stream()
                .anyMatch(pattern -> pattern.matcher(log.getLoggerName()).matches());
        List<LogEntry> filteredEvents = logs.stream().filter(withinTimeRange.and(withinSequence))
                .collect(Collectors.toList());
        // List<LogEntry> filteredEvents = logs.stream().filter(withinTimeRange.and(nameMatchesAnyPattern))
        // .collect(Collectors.toList());
        List<LogDTO> dtoList = filteredEvents.stream().map(this::map).collect(Collectors.toList());
        Collections.sort(dtoList);
        sendMessage(gson.toJson(dtoList), remoteEndpoint);
        // Remove any duplicates from the live log buffer
        long newestSequence = logs.getFirst().getSequence();
            Iterator<LogDTO> iterator = deferredLogs.iterator();
            while (iterator.hasNext()) {
                LogDTO value = iterator.next();
                if (value.sequence <= newestSequence) {
                    iterator.remove();
        // Continue with live logs...
     * @implNote Under no circumstances must this method result in something being logged, since that
     *           causes an "endless circle".
    public void logged(@NonNullByDefault({}) LogEntry logEntry) {
        if (!loggerPatterns.isEmpty() && loggerPatterns.stream().noneMatch(logPatternMatch(logEntry))) {
        LogDTO logDTO = map(logEntry);
        boolean bufferEmpty;
        ScheduledExecutorService executor;
        RemoteEndpoint remote;
            if ((executor = scheduledExecutorService) == null || (remote = remoteEndpoint) == null) {
            lastSequence = logEntry.getSequence();
            // If the buffer isn't empty or the last message was sent less than SEND_PERIOD ago, then we just buffer
            long sentTime = lastSentTime;
            boolean suspended = this.suspended;
            if (!(bufferEmpty = deferredLogs.isEmpty()) || suspended
                    || sentTime > System.currentTimeMillis() - SEND_PERIOD) {
                if (bufferEmpty) {
                    if (!suspended) {
                        commitScheduledFuture = executor.schedule(this::flush,
                                sentTime + SEND_PERIOD - System.currentTimeMillis(), TimeUnit.MILLISECONDS);
                deferredLogs.add(logDTO);
                executor.submit(() -> {
                    sendMessage(gson.toJson(logDTO), remote);
    private static Predicate<Pattern> logPatternMatch(LogEntry logEntry) {
        return pattern -> pattern.matcher(logEntry.getLoggerName()).matches();
    private LogDTO map(LogEntry logEntry) {
        String stackTrace;
        if (logEntry.getException() != null) {
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            logEntry.getException().printStackTrace(pw);
            stackTrace = sw.toString();
            stackTrace = "";
        return new LogDTO(logEntry.getSequence(), logEntry.getLoggerName(), logEntry.getLogLevel(), logEntry.getTime(),
                logEntry.getMessage(), stackTrace);
    private void stopDeferredScheduledFuture() {
        // Stop any existing scheduled commit
        ScheduledFuture<?> commitScheduledFuture;
            commitScheduledFuture = this.commitScheduledFuture;
            this.commitScheduledFuture = null;
        if (commitScheduledFuture != null && !commitScheduledFuture.isDone()) {
            commitScheduledFuture.cancel(false);
    private void flush() {
        List<LogDTO> logs;
            if (deferredLogs.isEmpty()) {
                logs = null;
                remoteEndpoint = null;
                logs = List.copyOf(deferredLogs);
                deferredLogs.clear();
        if (logs != null && remoteEndpoint != null) {
            sendMessage(gson.toJson(logs), remoteEndpoint);
        lastSentTime = System.currentTimeMillis();
        // Can't log anything from this class, so nothing to do
