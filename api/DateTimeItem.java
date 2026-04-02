 * A DateTimeItem stores a timestamp including a valid time zone.
public class DateTimeItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(DateTimeType.class,
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(DateTimeType.class,
            RefreshType.class);
    public DateTimeItem(String name) {
        super(CoreItemFactory.DATETIME, name);
     * Send a DateTimeType command to the item.
    public void send(DateTimeType command) {
    public void send(DateTimeType command, @Nullable String source) {
            State convertedState = state.as(DateTimeType.class);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof DateTimeType)) {
