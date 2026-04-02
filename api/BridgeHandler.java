 * A {@link BridgeHandler} handles the communication between the openHAB framework and
 * a <i>bridge</i> (a device that acts as a gateway to enable the communication with other devices)
 * represented by a {@link Bridge} instance.
 * A {@link BridgeHandler} is a {@link ThingHandler} as well.
public interface BridgeHandler extends ThingHandler {
     * Informs the bridge handler that a child handler has been initialized.
     * @param childHandler the initialized child handler
     * @param childThing the thing of the initialized child handler
    void childHandlerInitialized(ThingHandler childHandler, Thing childThing);
     * Informs the bridge handler that a child handler has been disposed.
     * @param childHandler the disposed child handler
     * @param childThing the thing of the disposed child handler
    void childHandlerDisposed(ThingHandler childHandler, Thing childThing);
