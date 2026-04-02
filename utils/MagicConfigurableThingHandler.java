 * Handler for thing with a configuration parameter
public class MagicConfigurableThingHandler extends BaseThingHandler {
    public MagicConfigurableThingHandler(Thing thing) {
        super.handleConfigurationUpdate(configurationParameters);
