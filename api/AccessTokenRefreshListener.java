package org.openhab.core.auth.client.oauth2;
 * Handler to act up on changes of the access token.
 * @author Hilbrand Bouwkamp - Initial contribution
public interface AccessTokenRefreshListener {
     * Notifies of a successful token response from {@link OAuthClientService#refreshToken()}.
     * @param tokenResponse token response
    void onAccessTokenResponse(AccessTokenResponse tokenResponse);
