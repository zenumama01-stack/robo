 * A DimmerItem can be used as a switch (ON/OFF), but it also accepts percent values
 * to reflect the dimmed state.
 * @author Markus Rathgeb - Support more types for getStateAs
public class DimmerItem extends SwitchItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(PercentType.class, OnOffType.class,
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(PercentType.class,
            OnOffType.class, IncreaseDecreaseType.class, RefreshType.class);
    public DimmerItem(String name) {
        super(CoreItemFactory.DIMMER, name);
    /* package */ DimmerItem(String type, String name) {
        super(type, name);
     * Send a PercentType command to the item.
    public void send(PercentType command) {
    public void send(PercentType command, @Nullable String source) {
     * Send an INCREASE/DECREASE command to the item.
    public void send(IncreaseDecreaseType command) {
    public void send(IncreaseDecreaseType command, @Nullable String source) {
            State convertedState = state.as(PercentType.class);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof PercentType)) {
            super.applyTimeSeries(timeSeries);
