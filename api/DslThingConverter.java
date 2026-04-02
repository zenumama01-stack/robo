package org.openhab.core.model.thing.internal.fileconverter;
import org.openhab.core.model.thing.internal.GenericItemChannelLinkProvider;
import org.openhab.core.model.thing.internal.GenericThingProvider;
import org.openhab.core.model.thing.thing.ModelChannel;
import org.openhab.core.model.thing.thing.ModelProperty;
import org.openhab.core.model.thing.thing.ThingFactory;
import org.openhab.core.thing.fileconverter.AbstractThingSerializer;
 * {@link DslThingConverter} is the DSL file converter for {@link Thing} objects
@Component(immediate = true, service = { ThingSerializer.class, ThingParser.class })
public class DslThingConverter extends AbstractThingSerializer implements ThingParser {
    private final Logger logger = LoggerFactory.getLogger(DslThingConverter.class);
    private final GenericThingProvider thingProvider;
    private final GenericItemChannelLinkProvider itemChannelLinkProvider;
    private final Map<String, ThingModel> elementsToGenerate = new ConcurrentHashMap<>();
    public DslThingConverter(final @Reference ModelRepository modelRepository,
            final @Reference GenericThingProvider thingProvider,
            final @Reference GenericItemChannelLinkProvider itemChannelLinkProvider,
        super(thingTypeRegistry, channelTypeRegistry, configDescRegistry);
    public void setThingsToBeSerialized(String id, List<Thing> things, boolean hideDefaultChannels,
            boolean hideDefaultParameters) {
        ThingModel model = ThingFactory.eINSTANCE.createThingModel();
        Set<Thing> handledThings = new HashSet<>();
        for (Thing thing : things) {
            if (handledThings.contains(thing)) {
            model.getThings().add(buildModelThing(thing, hideDefaultChannels, hideDefaultParameters, things.size() > 1,
                    true, things, handledThings));
        ThingModel model = elementsToGenerate.remove(id);
            // Double quotes are unexpectedly generated in thing UID when the segment contains a -.
            // Fix that by removing these double quotes. Requires to first build the generated syntax as a String
            modelRepository.generateFileFormat(outputStream, "things", model);
            String syntax = new String(outputStream.toByteArray()).replaceAll(":\"([a-zA-Z0-9_][a-zA-Z0-9_-]*)\"",
                    ":$1");
                out.write(syntax.getBytes());
                logger.warn("Exception when writing the generated syntax {}", e.getMessage());
    private ModelThing buildModelThing(Thing thing, boolean hideDefaultChannels, boolean hideDefaultParameters,
            boolean preferPresentationAsTree, boolean topLevel, List<Thing> onlyThings, Set<Thing> handledThings) {
        ModelThing model;
        ModelBridge modelBridge;
        List<Thing> childThings = getChildThings(thing, onlyThings);
        if (preferPresentationAsTree && thing instanceof Bridge && !childThings.isEmpty()) {
            modelBridge = ThingFactory.eINSTANCE.createModelBridge();
            modelBridge.setBridge(true);
            model = modelBridge;
            modelBridge = null;
            model = ThingFactory.eINSTANCE.createModelThing();
        if (!preferPresentationAsTree || topLevel) {
            model.setId(thing.getUID().getAsString());
            ThingUID bridgeUID = thing.getBridgeUID();
            if (bridgeUID != null && modelBridge == null) {
                model.setBridgeUID(bridgeUID.getAsString());
            model.setThingTypeId(thing.getThingTypeUID().getId());
            model.setThingId(thing.getUID().getId());
        ThingType thingType = thingTypeRegistry.getThingType(thing.getThingTypeUID(), localeProvider.getLocale());
        String label = thingType != null && thingType.getLabel().equals(thing.getLabel()) ? null : thing.getLabel();
        if (thing.getLocation() != null) {
            model.setLocation(thing.getLocation());
        for (ConfigParameter param : getConfigurationParameters(thing, hideDefaultParameters)) {
                model.getProperties().add(property);
        if (preferPresentationAsTree && modelBridge != null) {
            modelBridge.setThingsHeader(false);
            for (Thing child : childThings) {
                if (!handledThings.contains(child)) {
                    modelBridge.getThings().add(buildModelThing(child, hideDefaultChannels, hideDefaultParameters, true,
                            false, onlyThings, handledThings));
        List<Channel> channels = hideDefaultChannels ? getNonDefaultChannels(thing) : thing.getChannels();
        model.setChannelsHeader(!channels.isEmpty());
        for (Channel channel : channels) {
            model.getChannels().add(buildModelChannel(channel, hideDefaultParameters));
        handledThings.add(thing);
    private ModelChannel buildModelChannel(Channel channel, boolean hideDefaultParameters) {
        ModelChannel modelChannel = ThingFactory.eINSTANCE.createModelChannel();
            modelChannel.setChannelType(channelTypeUID.getId());
            modelChannel.setChannelKind(channel.getKind() == ChannelKind.STATE ? "State" : "Trigger");
            modelChannel.setType(channel.getAcceptedItemType());
        modelChannel.setId(channel.getUID().getId());
        if (channel.getLabel() != null) {
            modelChannel.setLabel(channel.getLabel());
        for (ConfigParameter param : getConfigurationParameters(channel, hideDefaultParameters)) {
                modelChannel.getProperties().add(property);
        return modelChannel;
        ModelProperty property = ThingFactory.eINSTANCE.createModelProperty();
                property.getValue().addAll(list);
            // DSL thing syntax does not like a configuration parameter value provided as Double type.
            // By security, we apply a conversion to a BigDecimal in case this would happen.
            logger.debug("Configuration parameter {} with value {} is provided unexpectedly as Double type", key,
        return modelRepository.createIsolatedModel("things", inputStream, errors, warnings);
    public Collection<Thing> getParsedObjects(String modelName) {
        return thingProvider.getAllFromModel(modelName);
    public Collection<ItemChannelLink> getParsedChannelLinks(String modelName) {
        return itemChannelLinkProvider.getAllFromContext(modelName);
