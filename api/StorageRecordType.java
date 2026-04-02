 * Enum of types being used in the store
public enum StorageRecordType {
    LAST_USED(".LastUsed"),
    ACCESS_TOKEN_RESPONSE(".AccessTokenResponse"),
    SERVICE_CONFIGURATION(".ServiceConfiguration"),
    DEVICE_CODE_RESPONSE(".DeviceCodeResponse");
    private String suffix;
    StorageRecordType(String suffix) {
        this.suffix = suffix;
    public String getKey(@Nullable String handle) {
        return (handle == null) ? this.suffix : (handle + this.suffix);
