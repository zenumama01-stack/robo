package org.openhab.core.model.sitemap.runtime.internal;
import org.openhab.core.model.sitemap.SitemapStandaloneSetup;
public class SitemapRuntimeActivator implements ModelParser {
    private final Logger logger = LoggerFactory.getLogger(SitemapRuntimeActivator.class);
        SitemapStandaloneSetup.doSetup();
        logger.debug("Registered 'sitemap' configuration parser");
        SitemapStandaloneSetup.unregister();
        return "sitemap";
