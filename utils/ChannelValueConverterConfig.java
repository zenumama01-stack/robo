import org.openhab.core.thing.binding.generic.converter.ColorChannelHandler;
 * The {@link ChannelValueConverterConfig} is a base class for the channel configuration of things
 * using the {@link ChannelHandler}s
public class ChannelValueConverterConfig {
    private final Map<String, State> stringStateMap = new HashMap<>();
    private final Map<Command, @Nullable String> commandStringMap = new HashMap<>();
    public ChannelMode mode = ChannelMode.READWRITE;
    // number
    public @Nullable String unit;
    // switch, dimmer, color
    public @Nullable String onValue;
    public @Nullable String offValue;
    // dimmer, color
    public BigDecimal step = BigDecimal.ONE;
    public @Nullable String increaseValue;
    public @Nullable String decreaseValue;
    // color
    public ColorChannelHandler.ColorMode colorMode = ColorChannelHandler.ColorMode.RGB;
    // contact
    public @Nullable String openValue;
    public @Nullable String closedValue;
    // rollershutter
    public @Nullable String upValue;
    public @Nullable String downValue;
    public @Nullable String stopValue;
    public @Nullable String moveValue;
    // player
    public @Nullable String playValue;
    public @Nullable String pauseValue;
    public @Nullable String nextValue;
    public @Nullable String previousValue;
    public @Nullable String rewindValue;
    public @Nullable String fastforwardValue;
     * maps a command to a user-defined string
     * @param command the command to map
     * @return a string or null if no mapping found
    public @Nullable String commandToFixedValue(Command command) {
            createMaps();
        return commandStringMap.get(command);
     * maps a user-defined string to a state
     * @param string the string to map
     * @return the state or null if no mapping found
    public @Nullable State fixedValueToState(String string) {
        return stringStateMap.get(string);
    private void createMaps() {
        addToMaps(this.onValue, OnOffType.ON);
        addToMaps(this.offValue, OnOffType.OFF);
        addToMaps(this.openValue, OpenClosedType.OPEN);
        addToMaps(this.closedValue, OpenClosedType.CLOSED);
        addToMaps(this.upValue, UpDownType.UP);
        addToMaps(this.downValue, UpDownType.DOWN);
        commandStringMap.put(IncreaseDecreaseType.INCREASE, increaseValue);
        commandStringMap.put(IncreaseDecreaseType.DECREASE, decreaseValue);
        commandStringMap.put(StopMoveType.STOP, stopValue);
        commandStringMap.put(StopMoveType.MOVE, moveValue);
        commandStringMap.put(PlayPauseType.PLAY, playValue);
        commandStringMap.put(PlayPauseType.PAUSE, pauseValue);
        commandStringMap.put(NextPreviousType.NEXT, nextValue);
        commandStringMap.put(NextPreviousType.PREVIOUS, previousValue);
        commandStringMap.put(RewindFastforwardType.REWIND, rewindValue);
        commandStringMap.put(RewindFastforwardType.FASTFORWARD, fastforwardValue);
    private void addToMaps(@Nullable String value, State state) {
            commandStringMap.put((Command) state, value);
            stringStateMap.put(value, state);
