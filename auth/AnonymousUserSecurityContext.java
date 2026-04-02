package org.openhab.core.io.rest.auth;
 * This {@link SecurityContext} can be used to give anonymous users (i.e. unauthenticated requests) the "user" role.
public class AnonymousUserSecurityContext implements SecurityContext {
    public @Nullable Principal getUserPrincipal() {
    public boolean isUserInRole(@Nullable String role) {
        return role == null || Role.USER.equals(role);
    public boolean isSecure() {
    public @Nullable String getAuthenticationScheme() {
