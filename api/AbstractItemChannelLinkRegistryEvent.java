package org.openhab.core.thing.link.events;
 * {@link AbstractItemChannelLinkRegistryEvent} is an abstract class for item channel link events.
public abstract class AbstractItemChannelLinkRegistryEvent extends AbstractEvent {
    private final ItemChannelLinkDTO link;
    protected AbstractItemChannelLinkRegistryEvent(String topic, String payload, ItemChannelLinkDTO link) {
    public ItemChannelLinkDTO getLink() {
