 * The {@link BaseBridgeHandler} implements the {@link BridgeHandler} interface and adds some convenience methods for
 * bridges to the {@link BaseThingHandler}.
 * It is recommended to extend this abstract base class.
 * @author Stefan Bußweiler - Added implementation of BridgeHandler interface
public abstract class BaseBridgeHandler extends BaseThingHandler implements BridgeHandler {
     * @see BaseThingHandler
    public BaseBridgeHandler(Bridge bridge) {
        super(bridge);
    public Bridge getThing() {
        return (Bridge) super.getThing();
     * Creates a bridge builder, which allows to modify the bridge. The method
     * {@link BaseThingHandler#updateThing(Thing)} must be called to persist the changes.
     * @return {@link BridgeBuilder} which builds an exact copy of the bridge
    protected BridgeBuilder editThing() {
        return BridgeBuilder.create(thing.getThingTypeUID(), thing.getUID()).withBridge(thing.getBridgeUID())
                .withChannels(thing.getChannels()).withConfiguration(thing.getConfiguration())
                .withLabel(thing.getLabel()).withLocation(thing.getLocation()).withProperties(thing.getProperties())
                .withSemanticEquipmentTag(thing.getSemanticEquipmentTag());
    public void childHandlerInitialized(ThingHandler childHandler, Thing childThing) {
        // do nothing by default, can be overridden by subclasses
    public void childHandlerDisposed(ThingHandler childHandler, Thing childThing) {
