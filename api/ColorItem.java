 * A ColorItem can be used for color values, e.g. for LED lights
public class ColorItem extends DimmerItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(HSBType.class, PercentType.class,
            OnOffType.class, UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(HSBType.class,
            PercentType.class, OnOffType.class, IncreaseDecreaseType.class, RefreshType.class);
    public ColorItem(String name) {
        super(CoreItemFactory.COLOR, name);
     * Send an HSBType command to the item.
    public void send(HSBType command) {
    public void send(HSBType command, @Nullable String source) {
            State currentState = this.state;
            if (currentState instanceof HSBType hsbType) {
                DecimalType hue = hsbType.getHue();
                PercentType saturation = hsbType.getSaturation();
                // we map ON/OFF values to dark/bright, so that the hue and saturation values are not changed
                if (state == OnOffType.OFF) {
                    applyState(new HSBType(hue, saturation, PercentType.ZERO), source);
                } else if (state == OnOffType.ON) {
                    applyState(new HSBType(hue, saturation, PercentType.HUNDRED), source);
                } else if (state instanceof PercentType percentType && !(state instanceof HSBType)) {
                    applyState(new HSBType(hue, saturation, percentType), source);
                } else if (state instanceof DecimalType decimalType && !(state instanceof HSBType)) {
                    applyState(
                            new HSBType(hue, saturation,
                                    new PercentType(decimalType.toBigDecimal().multiply(BigDecimal.valueOf(100)))),
                // try conversion
                State convertedState = state.as(HSBType.class);
                    applyState(convertedState, source);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof HSBType)) {
