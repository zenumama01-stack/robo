 * The {@link MagicOnlineOfflineHandler} is responsible for handling commands, which are
public class MagicOnlineOfflineHandler extends BaseThingHandler {
    private static final String TOGGLE_TIME = "toggleTime";
    private @NonNullByDefault({}) ScheduledFuture<?> toggleJob;
    public MagicOnlineOfflineHandler(Thing thing) {
        int toggleTime = ((BigDecimal) getConfig().get(TOGGLE_TIME)).intValue();
        toggleJob = scheduler.scheduleWithFixedDelay(() -> {
            if (getThing().getStatus() == ThingStatus.ONLINE) {
                updateStatus(ThingStatus.OFFLINE);
            } else if (getThing().getStatus() != ThingStatus.ONLINE) {
        }, 0, toggleTime, TimeUnit.SECONDS);
        if (toggleJob != null) {
            toggleJob.cancel(true);
            toggleJob = null;
