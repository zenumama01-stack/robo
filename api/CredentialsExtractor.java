package org.openhab.core.io.http.auth;
 * Provider of credentials which works in given context and can provide credentials out of it.
 * @author Łukasz Dywicki - Initial contribution.
 * @param <C> Context type.
public interface CredentialsExtractor<C> {
    Optional<Credentials> retrieveCredentials(C requestContext);
