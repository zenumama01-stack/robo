package org.openhab.core.model.thing.runtime.internal;
import org.openhab.core.model.thing.ThingStandaloneSetup;
public class ThingRuntimeActivator implements ModelParser {
    private final Logger logger = LoggerFactory.getLogger(ThingRuntimeActivator.class);
        ThingStandaloneSetup.doSetup();
        logger.debug("Registered 'thing' configuration parser");
        ThingStandaloneSetup.unregister();
        return "things";
