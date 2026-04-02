package org.openhab.core.cache.lru;
 * Each cache result instance can handle several {@link InputStream}s.
 * This class is a wrapper for such functionality and can
 * ask the cached entry for data, allowing concurrent access to
 * the source even if it is currently actively read from the supplier service.
 * This class implements the two main read methods (byte by byte, and with an array)
public class InputStreamCacheWrapper extends InputStream {
    private final Logger logger = LoggerFactory.getLogger(InputStreamCacheWrapper.class);
    private LRUMediaCacheEntry<?> cacheEntry;
    private int offset = 0;
     * Construct a transparent InputStream wrapper around data from the cache.
     * @param cacheEntry The parent cached {@link LRUMediaCacheEntry}
    public InputStreamCacheWrapper(LRUMediaCacheEntry<?> cacheEntry) {
        this.cacheEntry = cacheEntry;
        return cacheEntry.availableFrom(offset);
        byte[] bytesRead = cacheEntry.read(offset, 1);
        if (bytesRead.length == 0) {
            return bytesRead[0] & 0xff;
            throw new IOException("Array to write is null");
        Objects.checkFromIndexSize(off, len, b.length);
        if (len == 0) {
        byte[] bytesRead = cacheEntry.read(offset, len);
        offset += bytesRead.length;
        for (; i < len && i < bytesRead.length; i++) {
            b[off + i] = bytesRead[i];
        if (n > Integer.MAX_VALUE || n < Integer.MIN_VALUE) {
            throw new IOException("Invalid offset, exceeds Integer range");
        offset += (int) n;
            cacheEntry.closeStreamClient();
        long totalSize = cacheEntry.getTotalSize();
        if (totalSize > 0L) {
            return totalSize;
        logger.debug("Cannot get the length of the stream");
    public InputStream getClonedStream() throws IOException {
        return cacheEntry.getInputStream();
        markedOffset = offset;
        offset = markedOffset;
