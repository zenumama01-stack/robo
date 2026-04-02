package org.openhab.core.magic.service;
 * Testing service for multi-context configurations.
@Component(immediate = true, configurationPolicy = ConfigurationPolicy.REQUIRE, service = MagicMultiInstanceService.class, configurationPid = "org.openhab.magicmultiinstance")
public class MagicMultiInstanceService {
    private final Logger logger = LoggerFactory.getLogger(MagicMultiInstanceService.class);
        logger.debug("activate");
        for (Entry<String, Object> e : properties.entrySet()) {
            logger.debug("{}: {}", e.getKey(), e.getValue());
        logger.debug("modified");
