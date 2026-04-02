import static org.openhab.core.thing.profiles.SystemProfiles.RAWROCKER_DIMMER;
 * The {@link RawRockerDimmerProfile} transforms rocker switch channel events into dimmer commands.
 * @author Jan Kemmler - Initial contribution
public class RawRockerDimmerProfile implements TriggerProfile {
    private final ProfileContext context;
    private @Nullable ScheduledFuture<?> dimmFuture;
    private @Nullable ScheduledFuture<?> timeoutFuture;
    private long pressedTime = 0;
    RawRockerDimmerProfile(ProfileCallback callback, ProfileContext context) {
        return RAWROCKER_DIMMER;
        if (CommonTriggerEvents.DIR1_PRESSED.equals(event)) {
            buttonPressed(IncreaseDecreaseType.INCREASE);
        } else if (CommonTriggerEvents.DIR1_RELEASED.equals(event)) {
            buttonReleased(OnOffType.ON);
        } else if (CommonTriggerEvents.DIR2_PRESSED.equals(event)) {
            buttonPressed(IncreaseDecreaseType.DECREASE);
        } else if (CommonTriggerEvents.DIR2_RELEASED.equals(event)) {
            buttonReleased(OnOffType.OFF);
    private synchronized void buttonPressed(Command commandToSend) {
        if (null != timeoutFuture) {
        this.cancelDimmFuture();
        dimmFuture = context.getExecutorService().scheduleWithFixedDelay(() -> callback.sendCommand(commandToSend), 550,
                200, TimeUnit.MILLISECONDS);
        timeoutFuture = context.getExecutorService().schedule(() -> this.cancelDimmFuture(), 10, TimeUnit.SECONDS);
        pressedTime = System.currentTimeMillis();
    private synchronized void buttonReleased(Command commandToSend) {
        if (System.currentTimeMillis() - pressedTime <= 500) {
            callback.sendCommand(commandToSend);
    private synchronized void cancelDimmFuture() {
        if (null != dimmFuture) {
            dimmFuture.cancel(false);
