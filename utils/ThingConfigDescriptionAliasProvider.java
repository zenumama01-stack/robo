import org.openhab.core.config.core.ConfigDescriptionAliasProvider;
 * Provides a proxy for thing & channel configuration descriptions.
 * If a thing config description is requested, the provider will look up the thing/channel type
 * to get the configURI and the config description for it. If there is a corresponding {@link ConfigOptionProvider}, it
 * will be used to get updated options.
 * @author Chris Jackson - Updated to separate thing type from thing name
 * @author Simon Kaufmann - Added support for channel config descriptions, turned into alias handler
public class ThingConfigDescriptionAliasProvider implements ConfigDescriptionAliasProvider {
    public ThingConfigDescriptionAliasProvider(final @Reference ThingRegistry thingRegistry,
    public @Nullable URI getAlias(URI uri) {
        // If this is not a concrete thing, then return
        if (uri.getScheme() == null) {
        return switch (uri.getScheme()) {
            case "thing" -> getThingConfigDescriptionURI(uri);
            case "channel" -> getChannelConfigDescriptionURI(uri);
    private @Nullable URI getThingConfigDescriptionURI(URI uri) {
        // First, get the thing type so we get the generic config descriptions
        ThingUID thingUID = new ThingUID(uri.getSchemeSpecificPart());
        // Get the config description URI for this thing type
        return thingType.getConfigDescriptionURI();
    private @Nullable URI getChannelConfigDescriptionURI(URI uri) {
        String stringUID = uri.getSchemeSpecificPart();
        if (uri.getFragment() != null) {
            stringUID = stringUID + "#" + uri.getFragment();
        ChannelUID channelUID = new ChannelUID(stringUID);
        // First, get the thing so we get access to the channel type via the channel
        // Get the config description URI for this channel type
        return channelType.getConfigDescriptionURI();
