package org.openhab.core.model.persistence.runtime.internal;
import org.openhab.core.model.persistence.PersistenceStandaloneSetup;
public class PersistenceRuntimeActivator implements ModelParser {
    private final Logger logger = LoggerFactory.getLogger(PersistenceRuntimeActivator.class);
        PersistenceStandaloneSetup.doSetup();
        logger.debug("Registered 'persistence' configuration parser");
        PersistenceStandaloneSetup.unregister();
        return "persist";
