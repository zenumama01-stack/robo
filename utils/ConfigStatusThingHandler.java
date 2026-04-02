 * The {@link ConfigStatusThingHandler} is an extension of {@link BaseThingHandler} that implements the
 * {@link BaseThingHandler#updateConfiguration(Configuration)} to initiate a propagation of a new
 * @author Chris Jackson - Add updateConfiguration override to handle status updates
public abstract class ConfigStatusThingHandler extends BaseThingHandler implements ConfigStatusProvider {
     * Creates a new instance of this class for the given {@link Thing}.
     * @param thing the thing for this handler
    public ConfigStatusThingHandler(Thing thing) {
