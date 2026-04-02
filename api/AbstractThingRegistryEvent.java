package org.openhab.core.thing.events;
 * Abstract implementation of a thing registry event which will be posted by a {@link ThingRegistry} for added, removed
 * and updated items.
public abstract class AbstractThingRegistryEvent extends AbstractEvent {
    private final ThingDTO thing;
     * Must be called in subclass constructor to create a new thing registry event.
     * @param source the source
     * @param thing the thing data transfer object
    protected AbstractThingRegistryEvent(String topic, String payload, @Nullable String source, ThingDTO thing) {
     * Gets the thing data transfer object.
     * @return the thing data transfer object
    public ThingDTO getThing() {
