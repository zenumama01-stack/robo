import static org.hamcrest.Matchers.not;
import static org.hamcrest.Matchers.nullValue;
import static org.hamcrest.Matchers.sameInstance;
import static org.mockito.Mockito.timeout;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyNoMoreInteractions;
import java.util.Timer;
 * The {@link CacheScriptExtensionTest} contains tests for {@link CacheScriptExtension}
public class CacheScriptExtensionTest {
    private static final String SCRIPT1 = "script1";
    private static final String SCRIPT2 = "script2";
    private static final String KEY1 = "key1";
    private static final String KEY2 = "key2";
    private static final String VALUE1 = "value1";
    private static final String VALUE2 = "value2";
    public void sharedCacheBasicFunction() {
        CacheScriptExtension se = new CacheScriptExtension();
        ValueCache cache = getCache(se, SCRIPT1, CacheScriptExtension.SHARED_CACHE_NAME);
        testCacheBasicFunctions(cache);
    public void privateCacheBasicFunction() {
        ValueCache cache = getCache(se, SCRIPT1, CacheScriptExtension.PRIVATE_CACHE_NAME);
    public void sharedCacheIsSharedBetweenTwoRuns() {
        ValueCache cache1 = getCache(se, SCRIPT1, CacheScriptExtension.SHARED_CACHE_NAME);
        Objects.requireNonNull(cache1);
        cache1.put(KEY1, VALUE1);
        assertThat(cache1.get(KEY1), is(VALUE1));
        ValueCache cache2 = getCache(se, SCRIPT1, CacheScriptExtension.SHARED_CACHE_NAME);
        assertThat(cache2, not(sameInstance(cache1)));
        assertThat(cache2.get(KEY1), is(VALUE1));
    public void sharedCacheIsClearedIfScriptUnloaded() {
        se.unload(SCRIPT1);
        ValueCache cache1new = getCache(se, SCRIPT2, CacheScriptExtension.SHARED_CACHE_NAME);
        assertThat(cache1new.get(KEY1), nullValue());
    public void sharedCachesIsSharedBetweenTwoScripts() {
        ValueCache cache2 = getCache(se, SCRIPT2, CacheScriptExtension.SHARED_CACHE_NAME);
        assertThat(cache1, not(is(cache2)));
        cache2.remove(KEY1);
        assertThat(cache2.get(KEY1), nullValue());
        assertThat(cache1.get(KEY1), nullValue());
    public void privateCacheIsSharedBetweenTwoRuns() {
        ValueCache cache1 = getCache(se, SCRIPT1, CacheScriptExtension.PRIVATE_CACHE_NAME);
        ValueCache cache2 = getCache(se, SCRIPT1, CacheScriptExtension.PRIVATE_CACHE_NAME);
        assertThat(cache2, sameInstance(cache1));
    public void privateCacheIsClearedIfScriptUnloaded() {
        ValueCache cache1new = getCache(se, SCRIPT2, CacheScriptExtension.PRIVATE_CACHE_NAME);
        assertThat(cache1new, not(sameInstance(cache1)));
    public void privateCachesIsNotSharedBetweenTwoScripts() {
        ValueCache cache2 = getCache(se, SCRIPT2, CacheScriptExtension.PRIVATE_CACHE_NAME);
    public void jobsInSharedCacheAreCancelledOnUnload() {
        testJobCancellation(CacheScriptExtension.SHARED_CACHE_NAME);
    public void jobsInPrivateCacheAreCancelledOnUnload() {
        testJobCancellation(CacheScriptExtension.PRIVATE_CACHE_NAME);
    public void testJobCancellation(String cacheType) {
        ValueCache cache = getCache(se, SCRIPT1, cacheType);
        Timer timerMock = mock(Timer.class);
        ScheduledFuture<?> futureMock = mock(ScheduledFuture.class);
        cache.put(KEY1, timerMock);
        cache.put(KEY2, futureMock);
        // ensure jobs are not cancelled on removal
        cache.remove(KEY1);
        cache.remove(KEY2);
        verifyNoMoreInteractions(timerMock, futureMock);
        verify(timerMock, timeout(1000)).cancel();
        verify(futureMock, timeout(1000)).cancel(true);
    public void testCacheBasicFunctions(ValueCache cache) {
        // cache is initially empty
        assertThat(cache.get(KEY1), nullValue());
        // return value is null if no value before and new value can be retrieved
        assertThat(cache.put(KEY1, VALUE1), nullValue());
        assertThat(cache.get(KEY1), is(VALUE1));
        // value returns old value on update and updated value can be retrieved
        assertThat(cache.put(KEY1, VALUE2), is(VALUE1));
        assertThat(cache.get(KEY1), is(VALUE2));
        // old value is returned on removal and cache empty afterwards
        assertThat(cache.remove(KEY1), is(VALUE2));
        // new value is inserted from supplier
        assertThat(cache.get(KEY1, () -> VALUE1), is(VALUE1));
        // different keys return different values
        cache.put(KEY2, VALUE2);
        assertThat(cache.get(KEY2), is(VALUE2));
        // computed value is returned
        assertThat(cache.compute(KEY1, (k, v) -> VALUE1), is(VALUE1));
        assertThat(cache.compute(KEY1, (k, v) -> VALUE2), is(VALUE2));
        assertThat(cache.compute(KEY1, (k, v) -> null), nullValue());
        // remappingFunction is called with key and old value
        cache.compute(KEY1, (k, v) -> {
            assertThat(k, is(KEY1));
            assertThat(v, nullValue());
            return VALUE2;
        cache.put(KEY1, VALUE1);
            assertThat(v, is(VALUE1));
    private ValueCache getCache(CacheScriptExtension se, String scriptIdentifier, String type) {
        ValueCache cache = (ValueCache) se.get(scriptIdentifier, type);
        Objects.requireNonNull(cache);
        return cache;
