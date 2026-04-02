 * Interface for a service that provides access to a {@link ThingHandler}.
public interface ThingHandlerService {
     * Sets the ThingHandler on which the actions (methods) should be called
     * @param handler the {@link ThingHandler}
    void setThingHandler(ThingHandler handler);
     * Gets the ThingHandler on which the actions (methods) should be called
     * @return the {@link ThingHandler}
    ThingHandler getThingHandler();
     * This method is used by the framework during activation of the OSGi component.
     * It is called BEFORE the thing handler is set.
     * See {@link #initialize()}, {@link #deactivate()}
    default void activate() {
     * This method is used by the framework during de-activation of the OSGi component.
     * It is NOT guaranteed that the thing handler is still valid.
     * See {@link #dispose()}, {@link #activate()}
    default void deactivate() {
     * This method is used by the framework during activation of the service.
     * It is called AFTER the component is fully activated and thing handler has been set.
     * Implementations should override this method to add additional initialization code. This method should call
     * <code>super.initialize()</code> to ensure background discovery is properly handled.
     * See {@link #activate()}, {@link #dispose()}
    default void initialize() {
     * This method is used by the framework during de-activation of the service.
     * It is called while the component is still activated.
     * Code depending on an activated service should go here. This method should call <code>super.dispose()</code> to
     * ensure background discovery is properly handled.
     * See {@link #deactivate()}, {@link #initialize()}
    default void dispose() {
