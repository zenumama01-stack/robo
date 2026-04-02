 * A handler for a thermostat thing.
public class MagicThermostatThingHandler extends BaseThingHandler {
    public MagicThermostatThingHandler(Thing thing) {
        if (CHANNEL_SET_TEMPERATURE.equals(channelUID.getId())) {
            if (command instanceof DecimalType || command instanceof QuantityType) {
                String state = command.toFullString() + (command instanceof DecimalType ? " °C" : "");
                    updateState(CHANNEL_TEMPERATURE, new QuantityType<>(state));
                }, 2, TimeUnit.SECONDS);
        updateState(CHANNEL_TEMPERATURE, new QuantityType<>(21, SIUnits.CELSIUS));
        updateState(CHANNEL_SET_TEMPERATURE, new QuantityType<>(21, SIUnits.CELSIUS));
        updateState(CHANNEL_BATTERY_LEVEL, new DecimalType(23));
