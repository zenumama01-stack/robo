package org.openhab.core.model.item.runtime.internal;
import org.openhab.core.model.ItemsStandaloneSetup;
public class ItemRuntimeActivator implements ModelParser {
    private final Logger logger = LoggerFactory.getLogger(ItemRuntimeActivator.class);
    public void activate() throws Exception {
        ItemsStandaloneSetup.doSetup();
        logger.debug("Registered 'item' configuration parser");
    public void deactivate() throws Exception {
        ItemsStandaloneSetup.unregister();
    public String getExtension() {
        return "items";
