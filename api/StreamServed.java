import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;
 * Streams served by the AudioHTTPServer.
public record StreamServed(String url, AudioStream audioStream, AtomicInteger currentlyServedStream, AtomicLong timeout,
        boolean multiTimeStream, CompletableFuture<@Nullable Void> playEnd) {
