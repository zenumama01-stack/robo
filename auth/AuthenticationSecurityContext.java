package org.openhab.core.io.rest.auth.internal;
 * A {@link SecurityContext} holding an instance of {@link Authentication}
public interface AuthenticationSecurityContext extends SecurityContext {
     * Retrieves the {@link Authentication} associated with this context
     * @return the authentication instance
    Authentication getAuthentication();
