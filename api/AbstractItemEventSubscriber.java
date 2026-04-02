package org.openhab.core.items.events;
 * The {@link AbstractItemEventSubscriber} defines an abstract implementation of the {@link EventSubscriber} interface
 * for receiving {@link ItemStateEvent}s and {@link ItemCommandEvent}s from the openHAB event bus.
 * A subclass can implement the methods {@link #receiveUpdate(ItemStateEvent)} and
 * {@link #receiveCommand(ItemCommandEvent)} in order to receive and handle such events.
public abstract class AbstractItemEventSubscriber implements EventSubscriber {
    private final Set<String> subscribedEventTypes = Set.of(ItemStateEvent.TYPE, ItemCommandEvent.TYPE,
            ItemTimeSeriesEvent.TYPE);
        if (event instanceof ItemStateEvent stateEvent) {
            receiveUpdate(stateEvent);
        } else if (event instanceof ItemTimeSeriesEvent timeSeriesEvent) {
            receiveTimeSeries(timeSeriesEvent);
     * Callback method for receiving item command events from the openHAB event bus.
     * @param commandEvent the item command event
        // Default implementation: do nothing.
        // Can be implemented by subclass in order to handle item commands.
     * Callback method for receiving item update events from the openHAB event bus.
     * @param updateEvent the item update event
        // Can be implemented by subclass in order to handle item updates.
     * Callback method for receiving item timeseries events from the openHAB event bus.
     * @param timeSeriesEvent the timeseries event
        // Can be implemented by subclass in order to handle timeseries updates.
