import static org.openhab.core.magic.binding.MagicBindingConstants.CHANNEL_RAWBUTTON;
import org.openhab.core.thing.CommonTriggerEvents;
 * The {@link MagicButtonHandler} is capable of triggering different events. Triggers a PRESSED event every 5 seconds on
 * the rawbutton trigger channel.
public class MagicButtonHandler extends BaseThingHandler {
    private @Nullable ScheduledFuture<?> scheduledJob;
    public MagicButtonHandler(Thing thing) {
        startScheduledJob();
        stopScheduledJob();
    private void startScheduledJob() {
        ScheduledFuture<?> localScheduledJob = scheduledJob;
        if (localScheduledJob == null || localScheduledJob.isCancelled()) {
            scheduledJob = scheduler.scheduleWithFixedDelay(() -> {
                triggerChannel(CHANNEL_RAWBUTTON, CommonTriggerEvents.PRESSED);
            }, 5, 5, TimeUnit.SECONDS);
    private void stopScheduledJob() {
        if (localScheduledJob != null && !localScheduledJob.isCancelled()) {
            localScheduledJob.cancel(true);
            scheduledJob = null;
