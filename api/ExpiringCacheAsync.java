 * Complementary class to {@link org.openhab.core.cache.ExpiringCache}, implementing an asynchronous variant
 * of an expiring cache. An instance returns the cached value immediately to the callback if not expired yet, otherwise
 * issue a fetch in another thread and notify callback implementors asynchronously.
 * @param <V> the type of the cached value
public class ExpiringCacheAsync<V> {
    protected final long expiry;
    protected long expiresAt = 0;
    protected @Nullable CompletableFuture<V> currentNewValueRequest = null;
    protected @Nullable V value;
     * @param expiry the duration in milliseconds for how long the value stays valid. Must be positive.
    public ExpiringCacheAsync(Duration expiry) {
     * @param expiry the duration in milliseconds for how long the value stays valid. Must be greater than 0.
    public ExpiringCacheAsync(long expiry) {
        this(Duration.ofMillis(expiry));
     * @param requestNewValueFuture If the value is expired, this supplier is called to supply the cache with a future
     *            that on completion will update the cached value
     * @return the value in form of a CompletableFuture. You can for instance use it this way:
     *         {@code getValue().thenAccept(value->useYourValueHere(value));}. If you need the value synchronously you
     *         can use {@code getValue().get()}.
    public CompletableFuture<V> getValue(Supplier<CompletableFuture<V>> requestNewValueFuture) {
        if (isExpired()) {
            return refreshValue(requestNewValueFuture);
            return CompletableFuture.completedFuture(value);
    public void invalidateValue() {
     * Returns an arbitrary time reference in nanoseconds.
     * This is used for the cache to determine if a value has expired.
    protected long getCurrentNanoTime() {
        return System.nanoTime();
     * Refreshes and returns the value asynchronously. Use the return value like with getValue() to get the refreshed
     * @param requestNewValueFuture This supplier is called to supply the cache with a future
     *            that on completion will update the cached value. The supplier will not be used,
     *            if there is already an ongoing refresh.
     * @return the new value in form of a CompletableFuture.
    public synchronized CompletableFuture<V> refreshValue(Supplier<CompletableFuture<V>> requestNewValueFuture) {
        CompletableFuture<V> currentNewValueRequest = this.currentNewValueRequest;
        // There is already an ongoing refresh, just return that future
        if (currentNewValueRequest != null) {
            return currentNewValueRequest;
        // We request a value update from the supplier now
        currentNewValueRequest = this.currentNewValueRequest = requestNewValueFuture.get();
        if (currentNewValueRequest == null) {
            throw new IllegalArgumentException("We expect a CompletableFuture for refreshValue() to work!");
        CompletableFuture<V> t = currentNewValueRequest.thenApply(newValue -> {
            // No request is ongoing anymore, update the value and expire time
            this.currentNewValueRequest = null;
            expiresAt = getCurrentNanoTime() + expiry;
        // The @NonNullbyDefault annotation forces us to check the return value of thenApply.
        return expiresAt < getCurrentNanoTime();
     * Return the raw value, no matter if it is already
     * expired or still valid.
    public @Nullable V getLastKnownValue() {
