package org.openhab.core.io.transport.mdns.internal;
 * Addon of the default OSGi bundle activator
public final class MDNSActivator implements BundleActivator {
    private final Logger logger = LoggerFactory.getLogger(MDNSActivator.class);
     * Called whenever the OSGi framework starts our bundle
        logger.debug("mDNS service has been started.");
     * Called whenever the OSGi framework stops our bundle
        logger.debug("mDNS service has been stopped.");
