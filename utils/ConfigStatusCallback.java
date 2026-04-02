package org.openhab.core.config.core.status;
 * The {@link ConfigStatusCallback} interface is a callback interface to propagate a new configuration status for an
 * entity.
public interface ConfigStatusCallback {
     * Based on the given {@link ConfigStatusSource} this operation propagates a new configuration status for an entity
     * after its configuration has been updated.
     * @param configStatusSource the source of the configuration status
    void configUpdated(ConfigStatusSource configStatusSource);
