import javax.security.auth.spi.LoginModule;
import org.openhab.core.auth.UserRegistry;
import org.osgi.framework.ServiceReference;
 * This {@link LoginModule} delegates the authentication to a {@link UserRegistry}
public class ManagedUserLoginModule implements LoginModule {
    private final Logger logger = LoggerFactory.getLogger(ManagedUserLoginModule.class);
    private UserRegistry userRegistry;
    private Subject subject;
    public void initialize(Subject subject, CallbackHandler callbackHandler, Map<String, ?> sharedState,
            Map<String, ?> options) {
    public boolean login() throws LoginException {
            // try to get the UserRegistry instance
            BundleContext bundleContext = FrameworkUtil.getBundle(UserRegistry.class).getBundleContext();
            ServiceReference<UserRegistry> serviceReference = bundleContext.getServiceReference(UserRegistry.class);
            userRegistry = bundleContext.getService(serviceReference);
            logger.error("Cannot initialize the ManagedLoginModule", e);
            throw new LoginException("Authorization failed");
            Credentials credentials = (Credentials) this.subject.getPrivateCredentials().iterator().next();
            userRegistry.authenticate(credentials);
        } catch (AuthenticationException e) {
            throw new LoginException(e.getMessage());
    public boolean commit() throws LoginException {
    public boolean abort() throws LoginException {
    public boolean logout() throws LoginException {
