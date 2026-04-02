 * Params that need to be persisted.
 * @author Hilbrand Bouwkamp - Moved class to it's own file and added hashCode and equals methods
 * @author Gaël L'hopital - Added deserializerClassName
class PersistedParams {
    String handle;
    String tokenUrl;
    String authorizationUrl;
    String clientId;
    String clientSecret;
    String scope;
    Boolean supportsBasicAuth;
    String state;
    String redirectUri;
    int tokenExpiresInSeconds = 60;
     * Default constructor needed for json serialization.
    public PersistedParams() {
     * Constructor.
     * @param handle the handle to the oauth service
     * @param tokenUrl the token url of the oauth provider. This is used for getting access token.
     * @param authorizationUrl the authorization url of the oauth provider. This is used purely for generating
     *            authorization code/ url.
     * @param clientId the client id
     * @param clientSecret the client secret (optional)
     * @param scope the desired scope
     * @param supportsBasicAuth whether the OAuth provider supports basic authorization or the client id and client
     *            secret should be passed as form params. true - use http basic authentication, false - do not use http
     *            basic authentication, null - unknown (default to do not use)
    public PersistedParams(String handle, String tokenUrl, String authorizationUrl, String clientId,
            String clientSecret, String scope, Boolean supportsBasicAuth, int tokenExpiresInSeconds,
            @Nullable String deserializerClassName) {
        this.tokenUrl = tokenUrl;
        this.scope = scope;
        this.supportsBasicAuth = supportsBasicAuth;
        result = prime * result + ((authorizationUrl == null) ? 0 : authorizationUrl.hashCode());
        result = prime * result + ((clientId == null) ? 0 : clientId.hashCode());
        result = prime * result + ((clientSecret == null) ? 0 : clientSecret.hashCode());
        result = prime * result + ((handle == null) ? 0 : handle.hashCode());
        result = prime * result + ((redirectUri == null) ? 0 : redirectUri.hashCode());
        result = prime * result + ((scope == null) ? 0 : scope.hashCode());
        result = prime * result + ((state == null) ? 0 : state.hashCode());
        result = prime * result + ((supportsBasicAuth == null) ? 0 : supportsBasicAuth.hashCode());
        result = prime * result + tokenExpiresInSeconds;
        result = prime * result + ((tokenUrl == null) ? 0 : tokenUrl.hashCode());
        if (obj == null || getClass() != obj.getClass()) {
        PersistedParams other = (PersistedParams) obj;
            if (other.authorizationUrl != null) {
        } else if (!authorizationUrl.equals(other.authorizationUrl)) {
            if (other.clientId != null) {
        } else if (!clientId.equals(other.clientId)) {
        if (clientSecret == null) {
            if (other.clientSecret != null) {
        } else if (!clientSecret.equals(other.clientSecret)) {
        if (handle == null) {
            if (other.handle != null) {
        } else if (!handle.equals(other.handle)) {
        if (redirectUri == null) {
            if (other.redirectUri != null) {
        } else if (!redirectUri.equals(other.redirectUri)) {
        if (scope == null) {
            if (other.scope != null) {
        } else if (!scope.equals(other.scope)) {
            if (other.state != null) {
        } else if (!state.equals(other.state)) {
        if (supportsBasicAuth == null) {
            if (other.supportsBasicAuth != null) {
        } else if (!supportsBasicAuth.equals(other.supportsBasicAuth)) {
        if (tokenExpiresInSeconds != other.tokenExpiresInSeconds) {
            if (other.tokenUrl != null) {
        } else if (!tokenUrl.equals(other.tokenUrl)) {
