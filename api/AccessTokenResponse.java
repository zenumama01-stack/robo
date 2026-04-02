import java.io.Serializable;
 * This is the Access Token Response, a simple value-object that holds the result of the
 * from an Access Token Request, as listed in RFC 6749:
 * 4.1.4 - Authorization Code grant - Access Token Response,
 * 4.2.2 - Implicit Grant - Access Token Response,
 * 4.3.3 - Resource Owner Password Credentials Grant - Access Token Response
 * 4.4.3 - Client Credentials Grant - Access Token Response
 * @author Gary Tse - Adaptation for Eclipse SmartHome
public final class AccessTokenResponse implements Serializable, Cloneable {
     * For Serializable
    private static final long serialVersionUID = 4837164195629364014L;
     * The access token issued by the authorization server. It is used
     * by the client to gain access to a resource.
     * This token must be confidential in transit and storage.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-1.4">rfc6749 section-1.4</a>
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-10.3">rfc6749 section-10.3</a>
    private @Nullable String accessToken;
     * Token type. e.g. Bearer, MAC
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-7.1">rfc6749 section-7.1</a>
    private @Nullable String tokenType;
     * Number of seconds that this OAuthToken is valid for since the time it was created.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-4.2.2">rfc6749 section-4.2.2</a>
    private long expiresIn;
     * Refresh token is a string representing the authorization granted to
     * the client by the resource owner. Unlike access tokens, refresh tokens are
     * intended for use only with authorization servers and are never sent
     * to resource servers.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-1.5">rfc6749 section-1.5</a>
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-10.4">rfc6749 section-10.4</a>
    private @Nullable String refreshToken;
     * A space-delimited case-sensitive un-ordered string. This may be used
     * by the authorization server to inform the client of the scope of the access token
     * issued.
     * @see <a href="https://tools.ietf.org/html/rfc6749#section-3.3">rfc6749 section-3.3</a>
     * If the {@code state} parameter was present in the access token request.
     * The exact value should be returned as-is from the authorization provider.
     * <a href="https://tools.ietf.org/html/rfc6749#section-4.2.2">rfc6749 section-4.2.2</a>
    private @Nullable String state;
     * Created datetime of this access token. This is generated locally
     * by the OAUTH client as at the time the access token is received.
     * This should be slightly later than the actual time the access token
     * is produced at the server.
    private @Nullable Instant createdOn;
     * Extra elements that may be passed in the token response by a specific OAuth implementation.
     * These fields are provider-specific and are not restricted to standard OAuth fields. As such,
     * they may contain sensitive information (for example, identifiers, metadata or other values
     * that should not be logged or exposed).
     * Consumers of this value MUST treat all entries as potentially sensitive and avoid logging or
     * otherwise exposing them unless they have explicitly verified that the data is safe to do so.
    private Map<String, String> extraFields = Collections.emptyMap();
     * Returns the additional provider-specific fields from the token response.
     * Note: the returned map may contain sensitive information in non-standard fields. Callers
     * MUST take care not to log, persist, or expose these values without first ensuring that
     * doing so is appropriate in their security context.
     * @return a map of additional fields as provided by the authorization server
    public Map<String, String> getExtraFields() {
        return Collections.unmodifiableMap(extraFields);
    public void setExtraFields(Map<String, String> extraFields) {
        this.extraFields = (extraFields.isEmpty()) ? Collections.emptyMap() : Map.copyOf(extraFields);
     * Calculate if the token is expired against the given time.
     * It also returns true even if the token is not initialized (i.e. object newly created).
     * @param givenTime To calculate if the token is expired against the givenTime.
     * @param tokenExpiresInBuffer A positive integer in seconds to act as additional buffer to the calculation.
     *            This causes the OAuthToken to expire earlier then the stated expiry-time given
     *            by the authorization server.
     * @return true if object is not-initialized, or expired, or expired early due to buffer
    public boolean isExpired(Instant givenTime, int tokenExpiresInBuffer) {
        return createdOn == null
                || createdOn.plusSeconds(expiresIn).minusSeconds(tokenExpiresInBuffer).isBefore(givenTime);
    public @Nullable String getAccessToken() {
        return accessToken;
    public void setAccessToken(@Nullable String accessToken) {
    public @Nullable String getTokenType() {
        return tokenType;
    public void setTokenType(@Nullable String tokenType) {
        this.tokenType = tokenType;
    public long getExpiresIn() {
        return expiresIn;
    public void setExpiresIn(long expiresIn) {
        this.expiresIn = expiresIn;
    public @Nullable String getRefreshToken() {
    public void setRefreshToken(@Nullable String refreshToken) {
    public void setScope(@Nullable String scope) {
    public @Nullable String getState() {
    public void setState(@Nullable String state) {
    public @Nullable Instant getCreatedOn() {
        return createdOn;
    public void setCreatedOn(@Nullable Instant createdOn) {
        this.createdOn = createdOn;
    public Object clone() {
            return super.clone();
            throw new IllegalStateException("not possible", e);
        return Objects.hash(accessToken, tokenType, expiresIn, refreshToken, scope, state, createdOn, extraFields);
    public boolean equals(@Nullable Object thatAuthTokenObj) {
        if (this == thatAuthTokenObj) {
        // Exact match since class is final
        if (thatAuthTokenObj == null || !this.getClass().equals(thatAuthTokenObj.getClass())) {
        AccessTokenResponse that = (AccessTokenResponse) thatAuthTokenObj;
        return Objects.equals(this.accessToken, that.accessToken) && Objects.equals(this.tokenType, that.tokenType)
                && Objects.equals(this.expiresIn, that.expiresIn)
                && Objects.equals(this.refreshToken, that.refreshToken) && Objects.equals(this.scope, that.scope)
                && Objects.equals(this.state, that.state) && Objects.equals(this.createdOn, that.createdOn)
                && Objects.equals(this.extraFields, that.extraFields);
     * Warning: the toString() function may returns sensitive information that should not go into the log.
        return "AccessTokenResponse [accessToken=" + accessToken + ", tokenType=" + tokenType + ", expiresIn="
                + expiresIn + ", refreshToken=" + refreshToken + ", scope=" + scope + ", state=" + state
                + ", createdOn=" + createdOn + ", extraFields= " + extraFields + "]";
