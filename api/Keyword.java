package org.openhab.core.auth.oauth2client.internal;
 * Just a place to store all the important, reused keywords.
 * @author Gary Tse - Initial contribution
public interface Keyword {
    String CLIENT_ID = "client_id";
    String CLIENT_SECRET = "client_secret";
    String GRANT_TYPE = "grant_type";
    String USERNAME = "username";
    String PASSWORD = "password";
    String CLIENT_CREDENTIALS = "client_credentials";
    String AUTHORIZATION_CODE = "authorization_code";
    String SCOPE = "scope";
    String REFRESH_TOKEN = "refresh_token";
    String REDIRECT_URI = "redirect_uri";
    String CODE = "code"; // https://tools.ietf.org/html/rfc6749#section-4.1
    String STATE = "state"; // https://tools.ietf.org/html/rfc6749#section-4.1
