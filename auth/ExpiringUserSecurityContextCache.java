 * This class provides a cache for up to 10 UserSecurityContexts.
 * Entries have a lifetime and are removed from the cache upon the next
 * get call.
public class ExpiringUserSecurityContextCache {
    static final int MAX_SIZE = 10;
    static final int CLEANUP_FREQUENCY = 10;
    private final long keepPeriod;
    private final Map<String, MyEntry> entryMap;
    private int calls = 0;
    public ExpiringUserSecurityContextCache(long expirationTime) {
        this.keepPeriod = expirationTime;
        entryMap = new LinkedHashMap<>() {
            private static final long serialVersionUID = -1220310861591070462L;
            protected boolean removeEldestEntry(Map.@Nullable Entry<String, MyEntry> eldest) {
                return size() > MAX_SIZE;
    public synchronized @Nullable UserSecurityContext get(String key) {
        if (calls >= CLEANUP_FREQUENCY) {
            new HashSet<>(entryMap.keySet()).forEach(k -> getEntry(k));
            calls = 0;
        MyEntry entry = getEntry(key);
        if (entry != null) {
    public synchronized void put(String key, UserSecurityContext value) {
        entryMap.put(key, new MyEntry(System.currentTimeMillis(), value));
    public synchronized void clear() {
        entryMap.clear();
    private @Nullable MyEntry getEntry(String key) {
        MyEntry entry = entryMap.get(key);
            final long curTimeMillis = System.currentTimeMillis();
            long entryAge = curTimeMillis - entry.timestamp;
            if (entryAge < 0 || entryAge >= keepPeriod) {
                entryMap.remove(key);
                entry = null;
                entry.timestamp = curTimeMillis;
    static class MyEntry {
        public long timestamp;
        public final UserSecurityContext value;
        MyEntry(long timestamp, UserSecurityContext value) {
            this.timestamp = timestamp;
