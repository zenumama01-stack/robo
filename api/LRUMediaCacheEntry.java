import java.nio.channels.FileChannel;
 * A cached media entry resulting from a call to a supplier or a load from disk
 * This class also adds the capability to serve multiple InputStream concurrently
 * without asking already retrieved data to the wrapped stream.
public class LRUMediaCacheEntry<V> {
    private final Logger logger = LoggerFactory.getLogger(LRUMediaCacheEntry.class);
     * Arbitrary chunk size. Small is less latency but more small calls and CPU load.
    private static final int CHUNK_SIZE = 10000;
     * Take count of the number of {@link InputStreamCacheWrapper} currently using this {@link LRUMediaCacheEntry}
    private int countStreamClient = 0;
     * A unique key to identify the result
     * (used to build the filename)
    // The inner InputStream
    private @Nullable V metadata;
    // The data file where the media is stored:
    // optional metadata is stored here:
    private @Nullable Storage<V> storage;
    protected long currentSize = 0;
    private boolean completed;
    private boolean faulty = false;
    private @Nullable FileChannel fileChannel;
    private final Lock fileOperationLock = new ReentrantLock();
     * This constructor is used when the file is fully cached on disk.
     * The file on disk will provide the data, and the storage will
     * provide metadata.
     * @param key A unique key to identify the produced data
    public LRUMediaCacheEntry(String key) {
        this.completed = true;
     * This constructor is used when the file is not yet cached on disk.
     * Data is provided by the arguments
     * @param inputStream The data stream
     * @param metadata optional metadata to store along the stream
    public LRUMediaCacheEntry(String key, InputStream inputStream, @Nullable V metadata) {
        this.completed = false;
     * Link this cache entry to the underlying storage facility (disk for data, storage service for metadata)
     * @param cacheDirectory
     * @param storage
    protected void setCacheContext(Path cacheDirectory, Storage<V> storage) {
        File fileLocal = cacheDirectory.resolve(key).toFile();
        this.file = fileLocal;
        V actualDataInStorage = storage.get(key);
        if (actualDataInStorage == null) {
            storage.put(key, metadata);
            this.metadata = actualDataInStorage;
        this.currentSize = fileLocal.length();
     * Get total size of the underlying stream.
     * If not already completed, will query the stream inside,
     * or get all the data.
    protected long getTotalSize() {
        if (completed) { // we already know the total size of the sound
            return currentSize;
            // we must force-read all the stream to get the real size
                read(0, Integer.MAX_VALUE);
                logger.debug("Cannot read the total size of the cache result. Using 0", e);
     * Get the current size
    protected long getCurrentSize() {
     * Get the key identifying this cache entry
    protected String getKey() {
     * Open an InputStream wrapped around the file
     * There could be several clients InputStream on the same cache result
     * @return A new InputStream with data from the cache
    public InputStream getInputStream() throws IOException {
        File localFile = file;
        if (localFile == null) { // the cache entry is not tied to the disk. The cache is not ready or not to be used.
            InputStream inputStreamLocal = inputStream;
            if (inputStreamLocal != null) {
                return inputStreamLocal;
                        "Shouldn't happen. This cache entry is not tied to a file on disk and the inner input stream is null.");
        logger.debug("Trying to open a cache inputstream for {}", localFile.getName());
        fileOperationLock.lock();
            countStreamClient++;
            // we could have to open the fileChannel
            FileChannel fileChannelLocal = fileChannel;
            if (fileChannelLocal == null || !localFile.exists()) {
                fileChannelLocal = FileChannel.open(localFile.toPath(),
                        EnumSet.of(StandardOpenOption.CREATE, StandardOpenOption.READ, StandardOpenOption.WRITE));
                fileChannel = fileChannelLocal;
                // if the file size is 0 but the completed boolean is true, THEN it means the file have
                // been deleted. We must mark the file as to be recreated :
                if (completed && fileChannelLocal.size() == 0) {
                    logger.debug("The cached file {} is not present anymore. We will have to recreate it",
                            localFile.getName());
                    this.faulty = true;
            fileOperationLock.unlock();
        return new InputStreamCacheWrapper(this);
     * This method is called by a wrapper when it has been closed by a client
     * The file and the inner stream could then be closed, if and only if no other client are accessing it.
    protected void closeStreamClient() throws IOException {
        File fileLocal = file;
        if (fileLocal == null) {
            logger.debug("Trying to close a non existent-file. Is there a bug");
        logger.debug("Trying to close a cached inputstream client for {}", fileLocal.getName());
            countStreamClient--;
            if (countStreamClient <= 0) {// no more client reading or writing : closing the filechannel
                    if (fileChannelLocal != null) {
                            logger.debug("Effectively close the cache filechannel for {}", fileLocal.getName());
                            fileChannelLocal.close();
                            fileChannel = null;
                        inputStreamLocal.close();
                    if (inputStreamLocal instanceof Disposable disposableStream) {
                        disposableStream.dispose();
     * Get metadata for this cache result.
     * @return metadata
    public @Nullable V getMetadata() {
        return this.metadata;
     * Read from the cached file. If there is not enough bytes to read in the file, the supplier will be queried.
     * @param start The offset to read the file from
     * @param sizeToRead the number of byte to read
     * @return A byte array from the file. The size may or may not be the sizeToRead requested
    protected byte[] read(int start, int sizeToRead) throws IOException {
        if (fileChannelLocal == null || isFaulty()) {
            throw new IOException("Cannot read cache from null file channel or deleted file.");
        // check if we need to get data from the inner stream.
        if (start + sizeToRead > fileChannelLocal.size() && !completed) {
            logger.trace("Maybe need to get data from inner stream");
            // try to get new bytes from the inner stream
            InputStream streamLocal = inputStream;
            if (streamLocal != null) {
                logger.trace("Trying to synchronize for reading inner inputstream");
                synchronized (streamLocal) {
                    // now that we really have the lock, test again if we really need data from the stream
                    while (start + sizeToRead > fileChannelLocal.size() && !completed) {
                        logger.trace("Really need to get data from inner stream");
                        byte[] readFromSupplierStream = streamLocal.readNBytes(CHUNK_SIZE);
                        if (readFromSupplierStream.length == 0) { // we read all the stream
                            logger.trace("End of the stream reached");
                            completed = true;
                            fileChannelLocal.write(ByteBuffer.wrap(readFromSupplierStream), currentSize);
                            logger.trace("writing {} bytes to {}", readFromSupplierStream.length, key);
                            currentSize += readFromSupplierStream.length;
                faulty = true;
                logger.warn("Shouldn't happen : trying to get data from upstream for {} but original stream is null",
        // the cache file is now filled, get bytes from it.
        long maxToRead = Math.min(fileChannelLocal.size(), sizeToRead);
        ByteBuffer byteBufferFromChannelFile = ByteBuffer.allocate((int) maxToRead);
        int byteReadNumber = fileChannelLocal.read(byteBufferFromChannelFile, Integer.valueOf(start).longValue());
        logger.trace("Read {} bytes from the filechannel", byteReadNumber);
        if (byteReadNumber > 0) {
            byte[] resultByteArray = new byte[byteReadNumber];
            byteBufferFromChannelFile.rewind();
            byteBufferFromChannelFile.get(resultByteArray);
            return resultByteArray;
            return new byte[0];
     * Return the number of bytes that we can actually read without calling
     * the underlying stream
     * @param offset
    protected int availableFrom(int offset) {
        if (fileChannelLocal == null) {
            long nBytes = Math.min(Integer.MAX_VALUE, Math.max(0, fileChannelLocal.size() - offset));
            // nBytes is for sure in integer range, safe to cast
            return (int) nBytes;
            logger.debug("Cannot get file length for cache file {}", key);
     * Delete the cache file linked to this entry
    protected void deleteFile() {
        logger.debug("Receiving call to delete the cache file {}", key);
            // check if a client is actually reading the file
            if (countStreamClient <= 0) {
                logger.debug("Effectively deleting the cached file {}", key);
                // delete the file :
                if (fileLocal != null) {
                    fileLocal.delete();
                // and the associated info
                Storage<V> storageLocal = storage;
                if (storageLocal != null) {
                    storageLocal.remove(key);
    public boolean isFaulty() {
        return faulty;
