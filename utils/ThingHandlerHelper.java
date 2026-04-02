package org.openhab.core.thing.util;
 * This class provides utility methods related to the {@link ThingHandler} class.
 * @author Simon Kaufmann - added UNKNOWN
public class ThingHandlerHelper {
    private ThingHandlerHelper() {
     * Checks if the given state indicates that a thing handler has been initialized.
     * @return true if the thing handler has been initialized, otherwise false.
    public static boolean isHandlerInitialized(final ThingStatus thingStatus) {
        return thingStatus == ThingStatus.OFFLINE || thingStatus == ThingStatus.ONLINE
                || thingStatus == ThingStatus.UNKNOWN;
     * Checks if the thing handler has been initialized.
    public static boolean isHandlerInitialized(final Thing thing) {
        return isHandlerInitialized(thing.getStatus());
    public static boolean isHandlerInitialized(final ThingHandler handler) {
        return isHandlerInitialized(handler.getThing());
