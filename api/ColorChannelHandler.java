package org.openhab.core.thing.binding.generic.converter;
import org.openhab.core.thing.binding.generic.ChannelTransformation;
import org.openhab.core.thing.binding.generic.ChannelValueConverterConfig;
import org.openhab.core.thing.internal.binding.generic.converter.AbstractTransformingChannelHandler;
 * The {@link ColorChannelHandler} implements {@link org.openhab.core.library.items.ColorItem} conversions
public class ColorChannelHandler extends AbstractTransformingChannelHandler {
    private static final BigDecimal BYTE_FACTOR = BigDecimal.valueOf(2.55);
    private static final Pattern TRIPLE_MATCHER = Pattern.compile("(?<r>\\d+),(?<g>\\d+),(?<b>\\d+)");
    private State state = UnDefType.UNDEF;
    public ColorChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
            ChannelTransformation commandTransformations, ChannelValueConverterConfig channelConfig) {
        super(updateState, postCommand, sendValue, stateTransformations, commandTransformations, channelConfig);
    protected @Nullable Command toCommand(String value) {
    public String toString(Command command) {
        String string = channelConfig.commandToFixedValue(command);
        if (command instanceof HSBType newState) {
            state = newState;
            return hsbToString(newState);
        } else if (command instanceof PercentType percentCommand && state instanceof HSBType colorState) {
            HSBType newState = new HSBType(colorState.getHue(), colorState.getSaturation(), percentCommand);
        throw new IllegalArgumentException("Command type '" + command.toString() + "' not supported");
    public Optional<State> toState(String string) {
        State newState = UnDefType.UNDEF;
        if (string.equals(channelConfig.onValue)) {
            if (state instanceof HSBType hsbState) {
                newState = new HSBType(hsbState.getHue(), hsbState.getSaturation(), PercentType.HUNDRED);
                newState = HSBType.WHITE;
        } else if (string.equals(channelConfig.offValue)) {
                newState = new HSBType(hsbState.getHue(), hsbState.getSaturation(), PercentType.ZERO);
                newState = HSBType.BLACK;
        } else if (string.equals(channelConfig.increaseValue) && state instanceof HSBType hsbState) {
            BigDecimal newBrightness = hsbState.getBrightness().toBigDecimal().add(channelConfig.step);
            if (HUNDRED.compareTo(newBrightness) < 0) {
                newBrightness = HUNDRED;
            newState = new HSBType(hsbState.getHue(), hsbState.getSaturation(), new PercentType(newBrightness));
        } else if (string.equals(channelConfig.decreaseValue) && state instanceof HSBType hsbState) {
            BigDecimal newBrightness = hsbState.getBrightness().toBigDecimal().subtract(channelConfig.step);
            if (BigDecimal.ZERO.compareTo(newBrightness) > 0) {
                newBrightness = BigDecimal.ZERO;
            Matcher matcher = TRIPLE_MATCHER.matcher(string);
                switch (channelConfig.colorMode) {
                    case RGB -> {
                        int r = Integer.parseInt(matcher.group("r"));
                        int g = Integer.parseInt(matcher.group("g"));
                        int b = Integer.parseInt(matcher.group("b"));
                        newState = HSBType.fromRGB(r, g, b);
                    case HSB -> newState = new HSBType(string);
        return Optional.of(newState);
    private String hsbToString(HSBType state) {
            case RGB:
                PercentType[] rgb = ColorUtil.hsbToRgbPercent(state);
                return String.format("%1$d,%2$d,%3$d", rgb[0].toBigDecimal().multiply(BYTE_FACTOR).intValue(),
                        rgb[1].toBigDecimal().multiply(BYTE_FACTOR).intValue(),
                        rgb[2].toBigDecimal().multiply(BYTE_FACTOR).intValue());
            case HSB:
                return state.toString();
        throw new IllegalStateException("Invalid colorMode setting");
    public enum ColorMode {
        RGB,
        HSB
