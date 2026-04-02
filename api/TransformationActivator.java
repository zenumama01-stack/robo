package org.openhab.core.transform.internal;
 * Extension of the default OSGi bundle activator
public final class TransformationActivator implements BundleActivator {
    private final Logger logger = LoggerFactory.getLogger(TransformationActivator.class);
    public void start(@Nullable BundleContext bc) throws Exception {
        context = bc;
        logger.debug("Transformation Service has been started.");
    public void stop(@Nullable BundleContext bc) throws Exception {
        logger.debug("Transformation Service has been stopped.");
     * Returns the bundle context of this bundle
    public static @Nullable BundleContext getContext() {
