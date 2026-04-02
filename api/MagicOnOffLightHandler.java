 * The {@link MagicOnOffLightHandler} is responsible for handling commands, which are
public class MagicOnOffLightHandler extends BaseThingHandler {
    public MagicOnOffLightHandler(Thing thing) {
        triggerChannel("trigger", command.toString());
        updateState("timestamp", new DateTimeType());
