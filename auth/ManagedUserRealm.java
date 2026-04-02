import org.apache.karaf.jaas.boot.ProxyLoginModule;
import org.apache.karaf.jaas.config.JaasRealm;
 * A JAAS realm description for the {@link UserRegistry} based login module.
@Component(service = JaasRealm.class)
public class ManagedUserRealm implements JaasRealm {
    public static final String REALM_NAME = "openhab";
    public static final String MODULE_CLASS = "org.openhab.core.auth.jaas.internal.ManagedUserLoginModule";
        return REALM_NAME;
    public int getRank() {
    public AppConfigurationEntry[] getEntries() {
        Map<String, Object> options = new HashMap<>();
        options.put(ProxyLoginModule.PROPERTY_MODULE, MODULE_CLASS);
        return new AppConfigurationEntry[] {
                new AppConfigurationEntry(MODULE_CLASS, LoginModuleControlFlag.SUFFICIENT, options) };
