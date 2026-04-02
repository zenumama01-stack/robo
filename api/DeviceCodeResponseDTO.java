 * This {@link DeviceCodeResponseDTO} is a DTO with fields that encapsulate the data from RFC-8628 device code
 * responses.
 * Note: RFC-8628 says 'verificationUriComplete' and 'interval' are OPTIONAL fields.
 * See {@link AccessTokenResponse} for reference.
public final class DeviceCodeResponseDTO implements Serializable, Cloneable {
    private static final long serialVersionUID = 4261783375996959200L;
    private String deviceCode;
    private Long interval; // optional
    private String userCode;
    private String verificationUri;
    private String verificationUriComplete; // optional
    private Instant createdOn;
    public Instant getCreatedOn() {
    public String getDeviceCode() {
        return deviceCode;
    public Long getInterval() {
        return interval;
    public String getUserCode() {
        return userCode;
    public String getVerificationUri() {
        return verificationUri;
    public String getVerificationUriComplete() {
        return verificationUriComplete;
     * Calculate if the device code response is expired against the given time.
     * It also returns true even if the response is not initialized (i.e. newly created).
     * @param givenTime To calculate if the response is expired against the givenTime.
     *            This causes the response to expire earlier then the stated expiry-time given.
     * @return true if object is not-initialized, or expired, or expired early due to buffer.
    public void setCreatedOn(Instant createdOn) {
    public void setDeviceCode(String deviceCode) {
        this.deviceCode = deviceCode;
    public void setInterval(Long interval) {
        this.interval = interval;
    public void setUserCode(String userCode) {
        this.userCode = userCode;
    public void setVerificationUri(String verificationUri) {
        this.verificationUri = verificationUri;
    public void setVerificationUriComplete(String verificationUriComplete) {
        this.verificationUriComplete = verificationUriComplete;
        DeviceCodeResponseDTO other = (DeviceCodeResponseDTO) obj;
        return Objects.equals(createdOn, other.createdOn) && Objects.equals(deviceCode, other.deviceCode)
                && expiresIn == other.expiresIn && Objects.equals(interval, other.interval)
                && Objects.equals(userCode, other.userCode) && Objects.equals(verificationUri, other.verificationUri)
                && Objects.equals(verificationUriComplete, other.verificationUriComplete);
        return Objects.hash(createdOn, deviceCode, expiresIn, interval, userCode, verificationUri,
                verificationUriComplete);
        return "DeviceCodeResponseDTO [deviceCode=" + deviceCode + ", expiresIn=" + expiresIn + ", interval=" + interval
                + ", userCode=" + userCode + ", verificationUri=" + verificationUri + ", verificationUriComplete="
                + verificationUriComplete + ", createdOn=" + createdOn + "]";
