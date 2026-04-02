import static org.openhab.core.io.rest.auth.internal.ExpiringUserSecurityContextCache.*;
 * Tests {@link ExpiringUserSecurityContextCache}.
public class ExpiringUserSecurityContextCacheTest {
    private static final Duration ONE_HOUR = Duration.ofHours(1);
    private ExpiringUserSecurityContextCache createCache(Duration expirationDuration) {
        return new ExpiringUserSecurityContextCache(expirationDuration.toMillis());
    private Map<String, UserSecurityContext> createValues(int count) {
        Map<String, UserSecurityContext> map = new LinkedHashMap<>();
        for (int i = 0; i < count; i++) {
            String userName = "user" + i;
            UserSecurityContext userSecurityContext = new UserSecurityContext(new GenericUser(userName),
                    new Authentication(userName), userName + " token");
            map.put("key" + i, userSecurityContext);
    private void addValues(ExpiringUserSecurityContextCache cache, Map<String, UserSecurityContext> values) {
        values.entrySet().stream().forEach(entry -> cache.put(entry.getKey(), entry.getValue()));
    private void assertValuesAreCached(Map<String, UserSecurityContext> values,
            ExpiringUserSecurityContextCache cache) {
        for (Entry<String, UserSecurityContext> entry : values.entrySet()) {
            assertThat(cache.get(entry.getKey()), is(entry.getValue()));
    private void assertValuesAreNotCached(Map<String, UserSecurityContext> values,
            assertThat(cache.get(entry.getKey()), is(nullValue()));
    public void cachedValuesAreReturned() {
        ExpiringUserSecurityContextCache cache = createCache(ONE_HOUR);
        Map<String, UserSecurityContext> values = createValues(MAX_SIZE);
        addValues(cache, values);
        assertValuesAreCached(values, cache);
    public void nonCachedValuesAreNotReturned() {
        assertValuesAreNotCached(values, cache);
    public void clearedValuesAreNotReturned() {
    public void eldestEntriesAreRemovedWhenMaxSizeIsExceeded() {
        int removed = 20;
        Map<String, UserSecurityContext> values = createValues(MAX_SIZE + removed);
        Map<String, UserSecurityContext> removedValues = new LinkedHashMap<>();
        Map<String, UserSecurityContext> cachedValues = new LinkedHashMap<>();
            if (i < removed) {
                removedValues.put(entry.getKey(), entry.getValue());
                cachedValues.put(entry.getKey(), entry.getValue());
        assertValuesAreNotCached(removedValues, cache);
        assertValuesAreCached(cachedValues, cache);
    public void expiredEntriesAreRemoved() {
        ExpiringUserSecurityContextCache cache = createCache(Duration.ZERO);
        for (int i = 0; i < CLEANUP_FREQUENCY; i++) {
            cache.get("key");
