package org.openhab.core.automation.module.script.action;
import java.time.ZonedDateTime;
import org.openhab.core.types.Command;
import org.openhab.core.types.State;
import org.openhab.core.types.TimeSeries;
 * The {@link BusEvent} allows write access to the openHAB event bus from within scripts.
 * Items should not be updated directly (setting the state property), but updates should
 * be sent to the bus, so that all interested bundles are notified.
public interface BusEvent {
     * Sends a command for a specified item to the event bus.
     * @param item the item to send the command to
     * @param commandString the command to send
    void sendCommand(Item item, String commandString);
     * @param source the source of the command
    void sendCommand(Item item, String commandString, @Nullable String source);
     * Sends a number as a command for a specified item to the event bus.
     * @param command the number to send as a command
    void sendCommand(Item item, Number command);
    void sendCommand(Item item, Number command, @Nullable String source);
     * @param itemName the name of the item to send the command to
    void sendCommand(String itemName, String commandString);
    void sendCommand(String itemName, String commandString, @Nullable String source);
     * @param command the command to send
    void sendCommand(Item item, Command command);
    void sendCommand(Item item, Command command, @Nullable String source);
     * Posts a status update for a specified item to the event bus.
     * @param item the item to send the status update for
     * @param stateString the new state of the item
    void postUpdate(Item item, String stateString);
     * @param source the source of the status update
    void postUpdate(Item item, String stateString, @Nullable String source);
     * @param state the new state of the item as a number
    void postUpdate(Item item, Number state);
    void postUpdate(Item item, Number state, @Nullable String source);
     * @param itemName the name of the item to send the status update for
    void postUpdate(String itemName, String stateString);
    void postUpdate(String itemName, String stateString, @Nullable String source);
     * @param state the new state of the item
    void postUpdate(Item item, State state);
    void postUpdate(Item item, State state, @Nullable String source);
     * Sends a time series to the event bus
     * @param item the item to send the time series for
     * @param timeSeries a {@link TimeSeries} containing policy and values
    void sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries);
     * @param source the source of the time series
    void sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries, @Nullable String source);
     * @param values a {@link Map} containing the timeseries, composed of pairs of {@link ZonedDateTime} and
     *            {@link State}
     * @param policy either <code>ADD</code> or <code>REPLACE</code>
    void sendTimeSeries(@Nullable String itemName, @Nullable Map<ZonedDateTime, State> values, String policy);
    void sendTimeSeries(@Nullable String itemName, @Nullable Map<ZonedDateTime, State> values, String policy,
            @Nullable String source);
     * Stores the current states for a list of items in a map.
     * A group item is not itself put into the map, but instead all its members.
     * @param items the items for which the state should be stored
     * @return the map of items with their states
    Map<Item, State> storeStates(Item... items);
     * Restores item states from a map.
     * If the saved state can be interpreted as a command, a command is sent for the item
     * (and the physical device can send a status update if occurred). If it is no valid
     * command, the item state is directly updated to the saved value.
     * @param statesMap a map with ({@link Item}, {@link State}) entries
    void restoreStates(Map<Item, State> statesMap);
import org.openhab.core.model.script.internal.engine.action.BusEventActionService;
 * The {@link BusEvent} is a wrapper for the BusEvent actions.
public class BusEvent {
    public static Object sendCommand(Item item, String commandString) {
        BusEventActionService.getBusEvent().sendCommand(item, commandString);
    public static Object sendCommand(Item item, String commandString, String source) {
        BusEventActionService.getBusEvent().sendCommand(item, commandString, source);
    public static Object sendCommand(Item item, Number number) {
        BusEventActionService.getBusEvent().sendCommand(item, number);
    public static Object sendCommand(Item item, Number number, String source) {
        BusEventActionService.getBusEvent().sendCommand(item, number, source);
    public static Object sendCommand(String itemName, String commandString) {
        BusEventActionService.getBusEvent().sendCommand(itemName, commandString);
    public static Object sendCommand(String itemName, String commandString, String source) {
        BusEventActionService.getBusEvent().sendCommand(itemName, commandString, source);
    public static Object sendCommand(Item item, Command command) {
        BusEventActionService.getBusEvent().sendCommand(item, command);
    public static Object sendCommand(Item item, Command command, String source) {
        BusEventActionService.getBusEvent().sendCommand(item, command, source);
    public static Object postUpdate(Item item, Number state) {
        BusEventActionService.getBusEvent().postUpdate(item, state);
    public static Object postUpdate(Item item, Number state, String source) {
        BusEventActionService.getBusEvent().postUpdate(item, state, source);
    public static Object postUpdate(Item item, String stateAsString) {
        BusEventActionService.getBusEvent().postUpdate(item, stateAsString);
    public static Object postUpdate(Item item, String stateAsString, String source) {
        BusEventActionService.getBusEvent().postUpdate(item, stateAsString, source);
    public static Object postUpdate(String itemName, String stateString) {
        BusEventActionService.getBusEvent().postUpdate(itemName, stateString);
    public static Object postUpdate(String itemName, String stateString, String source) {
        BusEventActionService.getBusEvent().postUpdate(itemName, stateString, source);
    public static Object sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries) {
        BusEventActionService.getBusEvent().sendTimeSeries(item, timeSeries);
    public static Object sendTimeSeries(@Nullable Item item, @Nullable TimeSeries timeSeries, String source) {
        BusEventActionService.getBusEvent().sendTimeSeries(item, timeSeries, source);
    public static Object sendTimeSeries(@Nullable String itemName, @Nullable Map<ZonedDateTime, State> values,
            String policy) {
        BusEventActionService.getBusEvent().sendTimeSeries(itemName, values, policy);
            String policy, String source) {
        BusEventActionService.getBusEvent().sendTimeSeries(itemName, values, policy, source);
    public static Object postUpdate(Item item, State state) {
    public static Map<Item, State> storeStates(Item... items) {
        return BusEventActionService.getBusEvent().storeStates(items);
    public static Object restoreStates(Map<Item, State> statesMap) {
        BusEventActionService.getBusEvent().restoreStates(statesMap);
