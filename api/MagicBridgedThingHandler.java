 * The {@link MagicBridgedThingHandler} is responsible for handling commands, which are
 * sent to one of the channels.
public class MagicBridgedThingHandler extends BaseThingHandler {
    public MagicBridgedThingHandler(Thing thing) {
        Bridge bridge = getBridge();
        if (bridge == null || bridge.getStatus() == ThingStatus.UNINITIALIZED) {
            updateStatus(ThingStatus.UNKNOWN, ThingStatusDetail.BRIDGE_UNINITIALIZED);
        } else if (bridge.getStatus() == ThingStatus.OFFLINE) {
            updateStatus(ThingStatus.OFFLINE, ThingStatusDetail.BRIDGE_OFFLINE);
        } else if (bridge.getStatus() == ThingStatus.ONLINE) {
            updateStatus(ThingStatus.UNKNOWN);
