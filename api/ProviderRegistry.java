package org.openhab.core.automation.module.script.providersupport.internal;
 * Interface to be implemented by all {@link org.openhab.core.common.registry.Registry} (delegates) that are used to
 * provide openHAB entities from scripts.
 * @author Florian Hotze - Initial contribution
public interface ProviderRegistry {
     * Removes all elements that are provided by the script the {@link ProviderRegistry} instance is bound to.
     * To be called when the script is unloaded or reloaded.
    void removeAllAddedByScript();
