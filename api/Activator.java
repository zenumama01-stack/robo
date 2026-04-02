package org.openhab.core;
import org.osgi.annotation.bundle.Header;
@Header(name = Constants.BUNDLE_ACTIVATOR, value = "${@class}")
public final class Activator implements BundleActivator {
    private final Logger logger = LoggerFactory.getLogger(Activator.class);
        logger.info("Starting openHAB {} ({})", OpenHAB.getVersion(), OpenHAB.buildString());
        ServiceReference<ConfigurationAdmin> ref;
        if (bc != null && (ref = bc.getServiceReference(ConfigurationAdmin.class)) != null) {
            ConfigurationAdmin ca = bc.getService(ref);
            Configuration conf = ca.getConfiguration(OpenHAB.ADDONS_SERVICE_PID);
            conf.setBundleLocation("?openhab");
            bc.ungetService(ref);
            logger.warn("Could not acquire ConfigurationAdmin instance, configuration \"{}\" might not work correctly",
                    OpenHAB.ADDONS_SERVICE_PID);
    public void stop(@Nullable BundleContext context) throws Exception {
