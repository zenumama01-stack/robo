import static org.openhab.core.auth.oauth2client.internal.StorageRecordType.*;
import java.time.temporal.ChronoUnit;
import java.util.LinkedHashSet;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import org.openhab.core.auth.client.oauth2.StorageCipher;
import org.openhab.core.auth.oauth2client.internal.cipher.SymmetricKeyCipher;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializer;
 * This class handles the storage directly. It is internal to the OAuthClientService and there is
 * little need to study this.
 * The first role of this handler storing and caching the access token response, and persisted parameters.
 * The storage contains these:
 * 1. INDEX_HANDLES = json string-set of all handles
 * 2. <handle>.LastUsed = system-time-milliseconds
 * 3. <handle>.AccessTokenResponse = Json of AccessTokenResponse
 * 4. <handle>.ServiceConfiguration = Json of PersistedParameters
 * If at any time, the storage is not available, it is still possible to read existing access tokens from store.
 * The last-used statistics for this access token is broken. It is a measured risk to take.
 * If at any time, the storage is not available, it is not able to write any new access tokens into store.
 * All entries are subject to removal if they have not been used for 183 days or more (half year).
 * The recycle is performed when then instance is deactivated
@Component(property = "CIPHER_TARGET=SymmetricKeyCipher")
public class OAuthStoreHandlerImpl implements OAuthStoreHandler {
    // easy mocking with protected access
    protected static final int EXPIRE_DAYS = 183;
    protected static final int ACCESS_TOKEN_CACHE_SIZE = 50;
    private static final String STORE_NAME = "StorageHandler.For.OAuthClientService";
    private static final String STORE_KEY_INDEX_OF_HANDLES = "INDEX_HANDLES";
    private final Set<String> allHandles = new HashSet<>(); // must be initialized
    private final StorageFacade storageFacade;
    private final Set<StorageCipher> allAvailableStorageCiphers = new LinkedHashSet<>();
    private Optional<StorageCipher> storageCipher = Optional.empty();
    private final Logger logger = LoggerFactory.getLogger(OAuthStoreHandlerImpl.class);
    public OAuthStoreHandlerImpl(final @Reference StorageService storageService) {
        storageFacade = new StorageFacade(storageService.getStorage(STORE_NAME));
    public void activate(Map<String, Object> properties) throws GeneralSecurityException {
        // this allows future implementations to change cipher by just setting the CIPHER_TARGET
        String cipherTarget = (String) properties.getOrDefault("CIPHER_TARGET", SymmetricKeyCipher.CIPHER_ID);
        // choose the cipher by the cipherTarget
        storageCipher = allAvailableStorageCiphers.stream()
                .filter(cipher -> cipher.getUniqueCipherId().equals(cipherTarget)).findFirst();
        logger.debug("Using Cipher: {}", storageCipher
                .orElseThrow(() -> new GeneralSecurityException("No StorageCipher with target=" + cipherTarget)));
     * Deactivate and free resources.
        storageFacade.close(); // this removes old entries
        // DS will take care of other references
    public @Nullable AccessTokenResponse loadAccessTokenResponse(String handle) throws GeneralSecurityException {
        AccessTokenResponse accessTokenResponseFromStore = (AccessTokenResponse) storageFacade.get(handle,
                ACCESS_TOKEN_RESPONSE);
        if (accessTokenResponseFromStore == null) {
            // token does not exist
        return decryptToken(accessTokenResponseFromStore);
    public void saveAccessTokenResponse(String handle, @Nullable AccessTokenResponse pAccessTokenResponse) {
        AccessTokenResponse accessTokenResponse = pAccessTokenResponse;
        if (accessTokenResponse == null) {
            accessTokenResponse = new AccessTokenResponse(); // put empty
        AccessTokenResponse encryptedToken;
            encryptedToken = encryptToken(accessTokenResponse);
            logger.warn("Unable to encrypt token, storing as-is", e);
            encryptedToken = accessTokenResponse;
        storageFacade.put(handle, encryptedToken);
    public @Nullable DeviceCodeResponseDTO loadDeviceCodeResponse(String handle) throws GeneralSecurityException {
        DeviceCodeResponseDTO dcr = (DeviceCodeResponseDTO) storageFacade.get(handle, DEVICE_CODE_RESPONSE);
            // device code response does not exist
        return decryptDeviceCodeResponse(dcr);
    public void saveDeviceCodeResponse(String handle, @Nullable DeviceCodeResponseDTO dcrArg) {
        DeviceCodeResponseDTO dcr = dcrArg;
            dcr = new DeviceCodeResponseDTO(); // put empty
        DeviceCodeResponseDTO dcrEncrypted;
            dcrEncrypted = encryptDeviceCodeResponse(dcr);
            dcrEncrypted = dcr;
        storageFacade.put(handle, dcrEncrypted);
    public void remove(String handle) {
        storageFacade.removeByHandle(handle);
    public void removeAll() {
        storageFacade.removeAll();
        allHandles.clear();
    public void savePersistedParams(String handle, @Nullable PersistedParams persistedParams) {
        storageFacade.put(handle, persistedParams);
    public @Nullable PersistedParams loadPersistedParams(String handle) {
        return (PersistedParams) storageFacade.get(handle, SERVICE_CONFIGURATION);
    private AccessTokenResponse encryptToken(AccessTokenResponse accessTokenResponse) throws GeneralSecurityException {
        AccessTokenResponse encryptedAccessToken = (AccessTokenResponse) accessTokenResponse.clone();
        if (accessTokenResponse.getAccessToken() != null) {
            encryptedAccessToken.setAccessToken(encrypt(accessTokenResponse.getAccessToken()));
        if (accessTokenResponse.getRefreshToken() != null) {
            encryptedAccessToken.setRefreshToken(encrypt(accessTokenResponse.getRefreshToken()));
        return encryptedAccessToken;
    private DeviceCodeResponseDTO encryptDeviceCodeResponse(DeviceCodeResponseDTO dcr) throws GeneralSecurityException {
        DeviceCodeResponseDTO dcrEncrypted = (DeviceCodeResponseDTO) dcr.clone();
        if (dcr.getDeviceCode() != null) {
            dcrEncrypted.setDeviceCode(encrypt(dcr.getDeviceCode()));
        if (dcr.getUserCode() != null) {
            dcrEncrypted.setUserCode(encrypt(dcr.getUserCode()));
        if (dcr.getVerificationUri() != null) {
            dcrEncrypted.setVerificationUri(encrypt(dcr.getVerificationUri()));
        if (dcr.getVerificationUriComplete() != null) {
            dcrEncrypted.setVerificationUriComplete(encrypt(dcr.getVerificationUriComplete()));
        return dcrEncrypted;
    private AccessTokenResponse decryptToken(AccessTokenResponse accessTokenResponse) throws GeneralSecurityException {
        AccessTokenResponse decryptedToken = (AccessTokenResponse) accessTokenResponse.clone();
        if (storageCipher.isEmpty()) {
            return decryptedToken; // do nothing if no cipher
        logger.debug("Decrypting: {}", accessTokenResponse);
        decryptedToken.setAccessToken(storageCipher.get().decrypt(accessTokenResponse.getAccessToken()));
        decryptedToken.setRefreshToken(storageCipher.get().decrypt(accessTokenResponse.getRefreshToken()));
        return decryptedToken;
    private DeviceCodeResponseDTO decryptDeviceCodeResponse(DeviceCodeResponseDTO dcr) throws GeneralSecurityException {
        DeviceCodeResponseDTO dcrDecrypted = (DeviceCodeResponseDTO) dcr.clone();
            return dcrDecrypted; // do nothing if no cipher
        logger.debug("Decrypting: {}", dcr);
        dcrDecrypted.setDeviceCode(storageCipher.get().decrypt(dcr.getDeviceCode()));
        dcrDecrypted.setUserCode(storageCipher.get().decrypt(dcr.getUserCode()));
        dcrDecrypted.setVerificationUri(storageCipher.get().decrypt(dcr.getVerificationUri()));
        dcrDecrypted.setVerificationUriComplete(storageCipher.get().decrypt(dcr.getVerificationUriComplete()));
        return dcrDecrypted;
    private @Nullable String encrypt(@Nullable String token) throws GeneralSecurityException {
        if (token == null) {
            return token; // do nothing if no cipher
            StorageCipher cipher = storageCipher.get();
            return cipher.encrypt(token);
     * Static policy -- don't want to change cipher on the fly!
     * There may be multiple storage ciphers, choose the one that matches the target (done at activate)
    @Reference(cardinality = ReferenceCardinality.AT_LEAST_ONE)
    protected synchronized void setStorageCipher(StorageCipher storageCipher) {
        // keep all ciphers
        allAvailableStorageCiphers.add(storageCipher);
    @SuppressWarnings("PMD.CompareObjectsWithEquals")
    protected synchronized void unsetStorageCipher(StorageCipher storageCipher) {
        allAvailableStorageCiphers.remove(storageCipher);
        if (this.storageCipher.isPresent() && this.storageCipher.get() == storageCipher) {
            this.storageCipher = Optional.empty();
    private boolean isExpired(@Nullable Instant lastUsed) {
        if (lastUsed == null) {
        // (last used + 183 days < now) then it is expired
        return lastUsed.plus(EXPIRE_DAYS, ChronoUnit.DAYS).isBefore(Instant.now());
     * This is designed to simplify all the locking required for the store.
    private class StorageFacade implements AutoCloseable {
        private final Storage<String> storage;
        private final Lock storageLock = new ReentrantLock(); // for all operations on the storage
        private final Gson gson;
        public StorageFacade(Storage<String> storage) {
            this.storage = storage;
            // Add adapters for Instant
            gson = new GsonBuilder().setDateFormat(DateTimeType.DATE_PATTERN_JSON_COMPAT)
                    .registerTypeAdapter(Instant.class,
                            (JsonSerializer<Instant>) (date, type,
                                    jsonSerializationContext) -> new JsonPrimitive(date.toString()))
                    .setPrettyPrinting().create();
        public Set<String> getAllHandlesFromIndex() {
                String allHandlesStr = get(STORE_KEY_INDEX_OF_HANDLES);
                logger.debug("All available handles: {}", allHandlesStr);
                if (allHandlesStr == null) {
                    return Set.of();
                return Objects.requireNonNullElse(gson.fromJson(allHandlesStr, HashSet.class), Set.of());
            } catch (RuntimeException storeNotAvailable) {
        public @Nullable String get(String key) {
            storageLock.lock();
                return storage.get(key);
                storageLock.unlock();
        public @Nullable Object get(String handle, StorageRecordType recordType) {
                String value = storage.get(recordType.getKey(handle));
                // update last used when it is an access token
                if (ACCESS_TOKEN_RESPONSE.equals(recordType)) {
                        return gson.fromJson(value, AccessTokenResponse.class);
                                "Unable to deserialize json, discarding AccessTokenResponse.  "
                                        + "Please check json against standard or with oauth provider. json:\n{}",
                                value, e);
                } else if (SERVICE_CONFIGURATION.equals(recordType)) {
                        return gson.fromJson(value, PersistedParams.class);
                        logger.error("Unable to deserialize json, discarding PersistedParams. json:\n{}", value, e);
                } else if (LAST_USED.equals(recordType)) {
                        return gson.fromJson(value, Instant.class);
                        logger.info("Unable to deserialize json, reset LAST_USED to now. json:\n{}", value);
                        return Instant.now();
                } else if (DEVICE_CODE_RESPONSE.equals(recordType)) {
                        return gson.fromJson(value, DeviceCodeResponseDTO.class);
                                "Unable to deserialize json, discarding DeviceCodeResponse. "
        public void put(String handle, @Nullable AccessTokenResponse accessTokenResponse) {
                    storage.put(ACCESS_TOKEN_RESPONSE.getKey(handle), (String) null);
                    String gsonAccessTokenStr = gson.toJson(accessTokenResponse);
                    storage.put(ACCESS_TOKEN_RESPONSE.getKey(handle), gsonAccessTokenStr);
                    String gsonDateStr = gson.toJson(Instant.now());
                    storage.put(LAST_USED.getKey(handle), gsonDateStr);
                    if (!allHandles.contains(handle)) {
                        // update all handles index
                        allHandles.add(handle);
                        storage.put(STORE_KEY_INDEX_OF_HANDLES, gson.toJson(allHandles));
        public void put(String handle, @Nullable DeviceCodeResponseDTO dcr) {
                    storage.put(DEVICE_CODE_RESPONSE.getKey(handle), (String) null);
                    String gsonDcrString = gson.toJson(dcr);
                    storage.put(DEVICE_CODE_RESPONSE.getKey(handle), gsonDcrString);
        public void put(String handle, @Nullable PersistedParams persistedParams) {
                if (persistedParams == null) {
                    storage.put(SERVICE_CONFIGURATION.getKey(handle), (String) null);
                    String gsonPersistedParamsStr = gson.toJson(persistedParams);
                    storage.put(SERVICE_CONFIGURATION.getKey(handle), gsonPersistedParamsStr);
        public void removeByHandle(String handle) {
            logger.debug("Removing handle {} from storage", handle);
                if (allHandles.remove(handle)) { // entry exists and successfully removed
                    storage.remove(ACCESS_TOKEN_RESPONSE.getKey(handle));
                    storage.remove(DEVICE_CODE_RESPONSE.getKey(handle));
                    storage.remove(LAST_USED.getKey(handle));
                    storage.remove(SERVICE_CONFIGURATION.getKey(handle));
                    storage.put(STORE_KEY_INDEX_OF_HANDLES, gson.toJson(allHandles)); // update all handles
            // no need any locks, the other methods will take care of this
            Set<String> allHandlesFromStore = getAllHandlesFromIndex();
            for (String handle : allHandlesFromStore) {
                removeByHandle(handle);
            boolean lockGained = false;
                // dont want to wait too long during shutdown or update
                lockGained = storageLock.tryLock(15, TimeUnit.SECONDS);
                // if lockGained within timeout, then try to remove old entries
                if (lockGained) {
                    String handlesSSV = this.storage.get(STORE_KEY_INDEX_OF_HANDLES);
                    if (handlesSSV != null) {
                        String[] handles = handlesSSV.trim().split(" ");
                        for (String handle : handles) {
                            Instant lastUsed = (Instant) get(handle, LAST_USED);
                            if (isExpired(lastUsed)) {
                // if lock is not acquired within the timeout or thread is interruted
                // then forget about the old entries, do not try to delete them.
                // re-setting thread state to interrupted
                Thread.currentThread().interrupt();
                    } catch (IllegalMonitorStateException e) {
                        // never reach here normally
                        logger.error("Unexpected attempt to unlock without lock", e);
