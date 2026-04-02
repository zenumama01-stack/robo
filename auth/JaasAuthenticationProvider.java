package org.openhab.core.auth.jaas.internal;
import java.security.Principal;
import javax.security.auth.Subject;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.callback.NameCallback;
import javax.security.auth.callback.PasswordCallback;
import javax.security.auth.callback.UnsupportedCallbackException;
import javax.security.auth.login.LoginContext;
import javax.security.auth.login.LoginException;
import org.openhab.core.auth.Authentication;
import org.openhab.core.auth.AuthenticationException;
import org.openhab.core.auth.AuthenticationProvider;
import org.openhab.core.auth.Credentials;
import org.openhab.core.auth.GenericUser;
import org.openhab.core.auth.UsernamePasswordCredentials;
 * Implementation of authentication provider which is backed by JAAS realm.
 * Real authentication logic is embedded in login modules implemented by 3rd party, this code is just for bridging it.
 * @author Łukasz Dywicki - Initial contribution
 * @author Kai Kreuzer - Removed ManagedService and used DS configuration instead
 * @author Yannick Schaus - provides a configuration with the ManagedUserLoginModule as a sufficient login module
@Component(configurationPid = "org.openhab.jaas")
public class JaasAuthenticationProvider implements AuthenticationProvider {
    private static final String DEFAULT_REALM = "openhab";
    private @Nullable String realmName;
    public Authentication authenticate(final Credentials credentials) throws AuthenticationException {
        if (realmName == null) { // configuration is not yet ready or set
            realmName = DEFAULT_REALM;
        if (!(credentials instanceof UsernamePasswordCredentials)) {
            throw new AuthenticationException("Unsupported credentials passed to provider.");
        UsernamePasswordCredentials userCredentials = (UsernamePasswordCredentials) credentials;
        final String name = userCredentials.getUsername();
        final char[] password = userCredentials.getPassword().toCharArray();
        final ClassLoader contextClassLoader = Thread.currentThread().getContextClassLoader();
            Principal userPrincipal = new GenericUser(name);
            Subject subject = new Subject(DEFAULT_REALM.equals(realmName), Set.of(userPrincipal), Set.of(),
                    Set.of(userCredentials));
            CallbackHandler callbackHandler = new CallbackHandler() {
                public void handle(@NonNullByDefault({}) Callback[] callbacks)
                        throws IOException, UnsupportedCallbackException {
                    for (Callback callback : callbacks) {
                        if (callback instanceof PasswordCallback passwordCallback) {
                            passwordCallback.setPassword(password);
                        } else if (callback instanceof NameCallback nameCallback) {
                            nameCallback.setName(name);
                            throw new UnsupportedCallbackException(callback);
            LoginContext loginContext;
            if (DEFAULT_REALM.equals(realmName)) {
                Thread.currentThread().setContextClassLoader(ManagedUserLoginModule.class.getClassLoader());
                loginContext = new LoginContext(realmName, subject, callbackHandler,
                        new ManagedUserLoginConfiguration());
                loginContext = new LoginContext(realmName, subject, callbackHandler);
            loginContext.login();
            return getAuthentication(name, loginContext.getSubject());
        } catch (LoginException e) {
            String message = e.getMessage();
            throw new AuthenticationException(message != null ? message : "An unexpected LoginException occurred");
            Thread.currentThread().setContextClassLoader(contextClassLoader);
    private Authentication getAuthentication(String name, Subject subject) {
        return new Authentication(name, getRoles(subject.getPrincipals()));
    private String[] getRoles(Set<Principal> principals) {
        return principals.stream().map(Principal::getName).distinct().toArray(String[]::new);
    protected void activate(Map<String, Object> properties) {
        modified(properties);
    protected void deactivate(Map<String, Object> properties) {
    protected void modified(Map<String, Object> properties) {
        Object propertyValue = properties.get("realmName");
        if (propertyValue != null) {
            if (propertyValue instanceof String string) {
                realmName = string;
                realmName = propertyValue.toString();
            // value could be unset, we should reset its value
    public boolean supports(Class<? extends Credentials> type) {
        return UsernamePasswordCredentials.class.isAssignableFrom(type);
