import javax.security.auth.login.AppConfigurationEntry;
import javax.security.auth.login.AppConfigurationEntry.LoginModuleControlFlag;
import javax.security.auth.login.Configuration;
 * Describes a JAAS configuration with the {@link ManagedUserLoginModule} as a sufficient login module.
 * @author Yannick Schaus - initial contribution
public class ManagedUserLoginConfiguration extends Configuration {
    public AppConfigurationEntry[] getAppConfigurationEntry(String name) {
        return new AppConfigurationEntry[] { new AppConfigurationEntry(ManagedUserLoginModule.class.getCanonicalName(),
                LoginModuleControlFlag.SUFFICIENT, new HashMap<>()) };
