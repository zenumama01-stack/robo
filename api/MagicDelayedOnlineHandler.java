 * ThingHandler for a thing that goes online after 15 seconds
public class MagicDelayedOnlineHandler extends BaseThingHandler {
    private static final int DELAY = 15;
    public MagicDelayedOnlineHandler(Thing thing) {
        // schedule delayed job to set the thing to ONLINE
        scheduler.schedule(() -> updateStatus(ThingStatus.ONLINE), DELAY, TimeUnit.SECONDS);
            if (command instanceof DecimalType cmd) {
                int cmdInt = cmd.intValue();
                ThingStatus status = cmdInt > 0 ? ThingStatus.ONLINE : ThingStatus.OFFLINE;
                int waitTime = Math.abs(cmd.intValue());
                scheduler.schedule(() -> updateStatus(status), waitTime, TimeUnit.SECONDS);
