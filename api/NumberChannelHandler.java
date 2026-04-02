import javax.measure.format.MeasurementParseException;
 * The {@link NumberChannelHandler} implements {@link org.openhab.core.library.items.NumberItem} conversions
public class NumberChannelHandler extends AbstractTransformingChannelHandler {
    public NumberChannelHandler(Consumer<State> updateState, Consumer<Command> postCommand,
        String trimmedValue = value.trim();
        if (!trimmedValue.isEmpty()) {
                if (channelConfig.unit != null) {
                    // we have a given unit - use that
                    newState = new QuantityType<>(trimmedValue + " " + channelConfig.unit);
                        // try if we have a simple number
                        newState = new DecimalType(trimmedValue);
                    } catch (IllegalArgumentException e1) {
                        // not a plain number, maybe with unit?
                        newState = new QuantityType<>(trimmedValue);
            } catch (IllegalArgumentException | MeasurementParseException e) {
                // finally failed
