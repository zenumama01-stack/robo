 * ThingHandler that randomly sends numbers and strings to channels based on a configured interval
public class MagicChattyThingHandler extends BaseThingHandler {
    private static Logger logger = LoggerFactory.getLogger(MagicChattyThingHandler.class);
    private static final String PARAM_INTERVAL = "interval";
    private static final int START_DELAY = 3;
    private static final List<String> RANDOM_TEXTS = List.of("OPEN", "CLOSED", "ON", "OFF", "Hello",
            "This is a sentence");
    private final Set<ChannelUID> numberChannelUIDs = new HashSet<>();
    private final Set<ChannelUID> textChannelUIDs = new HashSet<>();
    private BigDecimal interval = new BigDecimal(0);
    private final Runnable chatRunnable;
    private @Nullable ScheduledFuture<?> backgroundJob = null;
        Configuration config = getConfig();
        interval = (BigDecimal) config.get(PARAM_INTERVAL);
        if (interval == null) {
            updateStatus(ThingStatus.OFFLINE, ThingStatusDetail.CONFIGURATION_ERROR, "Interval not set");
        // do not start the chatting job if interval is 0, just set the thing to ONLINE
        if (interval.intValue() > 0) {
            backgroundJob = scheduler.scheduleWithFixedDelay(chatRunnable, START_DELAY, interval.intValue(),
        if (backgroundJob != null && !backgroundJob.isCancelled()) {
            backgroundJob.cancel(true);
    public MagicChattyThingHandler(Thing thing) {
        chatRunnable = new Runnable() {
                for (ChannelUID channelUID : numberChannelUIDs) {
                    double randomValue = ThreadLocalRandom.current().nextDouble() * 100;
                    int intValue = (int) randomValue;
                    State cmd;
                    if (intValue % 2 == 0) {
                        cmd = new QuantityType<>(randomValue + "°C");
                        cmd = new DecimalType(randomValue);
                    updateState(channelUID, cmd);
                for (ChannelUID channelUID : textChannelUIDs) {
                    int pos = (int) (ThreadLocalRandom.current().nextDouble() * (RANDOM_TEXTS.size() - 1));
                    String randomValue = RANDOM_TEXTS.get(pos);
                    StringType cmd = new StringType(randomValue);
        super.channelLinked(channelUID);
        if ("number".equals(channelUID.getId())) {
            numberChannelUIDs.add(channelUID);
        } else if ("text".equals(channelUID.getId())) {
            textChannelUIDs.add(channelUID);
        super.channelUnlinked(channelUID);
            numberChannelUIDs.remove(channelUID);
            textChannelUIDs.remove(channelUID);
        logger.debug("Received command {} on channel {}", command, channelUID);
    protected void updateState(ChannelUID channelUID, State state) {
        logger.debug("Got state {} from device on channel {}", state, channelUID);
        super.updateState(channelUID, state);
