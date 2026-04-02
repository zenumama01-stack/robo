package org.openhab.core.internal.auth;
import org.openhab.core.auth.UnsupportedCredentialsException;
 * Default implementation of authentication manager.
public class AuthenticationManagerImpl implements AuthenticationManager {
    private final Logger logger = LoggerFactory.getLogger(AuthenticationManagerImpl.class);
    private final List<AuthenticationProvider> providers = new CopyOnWriteArrayList<>();
    public Authentication authenticate(Credentials credentials) throws AuthenticationException {
        boolean unmatched = true;
        Class<? extends Credentials> credentialsClazz = credentials.getClass();
        for (AuthenticationProvider provider : providers) {
            if (provider.supports(credentialsClazz)) {
                unmatched = false;
                    Authentication authentication = provider.authenticate(credentials);
                    logger.info("Failed to authenticate credentials {} with provider {}", credentialsClazz, provider);
        if (unmatched) {
            throw new UnsupportedCredentialsException("Unsupported credentials specified " + credentialsClazz);
        throw new AuthenticationException("Could not authenticate credentials " + credentialsClazz);
    public void addAuthenticationProvider(AuthenticationProvider provider) {
    public void removeAuthenticationProvider(AuthenticationProvider provider) {
