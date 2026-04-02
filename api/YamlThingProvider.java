import org.openhab.core.thing.type.AutoUpdatePolicy;
 * {@link YamlThingProvider} is an OSGi service, that allows to define things in YAML configuration files.
 * These things are automatically exposed to the {@link org.openhab.core.thing.ThingRegistry}.
@Component(immediate = true, service = { ThingProvider.class, YamlThingProvider.class, YamlModelListener.class })
public class YamlThingProvider extends AbstractProvider<Thing>
        implements ThingProvider, YamlModelListener<YamlThingDTO>, ReadyService.ReadyTracker {
    private static final String XML_THING_TYPE = "openhab.xmlThingTypes";
    private final Logger logger = LoggerFactory.getLogger(YamlThingProvider.class);
    private final Set<String> loadedXmlThingTypes = new CopyOnWriteArraySet<>();
    private final Map<String, Collection<Thing>> thingsMap = new ConcurrentHashMap<>();
    private final List<QueueContent> queue = new CopyOnWriteArrayList<>();
    private final Runnable lazyRetryRunnable = new Runnable() {
            logger.debug("Starting lazy retry thread");
            while (!queue.isEmpty()) {
                for (QueueContent qc : queue) {
                    if (retryCreateThing(qc.thingHandlerFactory, qc.thingTypeUID, qc.configuration, qc.thingUID,
                            qc.bridgeUID)) {
                        queue.remove(qc);
                if (!queue.isEmpty()) {
            logger.debug("Lazy retry thread ran out of work. Good bye.");
    private @Nullable Thread lazyRetryThread;
    private record QueueContent(ThingHandlerFactory thingHandlerFactory, ThingTypeUID thingTypeUID,
            Configuration configuration, ThingUID thingUID, @Nullable ThingUID bridgeUID) {
    public YamlThingProvider(final @Reference BundleResolver bundleResolver,
        thingsMap.clear();
        loadedXmlThingTypes.clear();
        return thingsMap.keySet().stream().filter(name -> !isIsolatedModel(name))
                .map(name -> thingsMap.getOrDefault(name, List.of())).flatMap(list -> list.stream()).toList();
    public Collection<Thing> getAllFromModel(String modelName) {
        return thingsMap.getOrDefault(modelName, List.of());
    public Class<YamlThingDTO> getElementClass() {
        return YamlThingDTO.class;
    public void addedModel(String modelName, Collection<YamlThingDTO> elements) {
        boolean isolated = isIsolatedModel(modelName);
        List<Thing> added = elements.stream().map(t -> mapThing(t, isolated)).filter(Objects::nonNull).toList();
        Collection<Thing> modelThings = Objects
                .requireNonNull(thingsMap.computeIfAbsent(modelName, k -> new ArrayList<>()));
        modelThings.addAll(added);
            logger.debug("model {} added thing {}", modelName, t.getUID());
            if (!isolated) {
    public void updatedModel(String modelName, Collection<YamlThingDTO> elements) {
        List<Thing> updated = elements.stream().map(t -> mapThing(t, isolated)).filter(Objects::nonNull).toList();
            modelThings.stream().filter(th -> th.getUID().equals(t.getUID())).findFirst().ifPresentOrElse(oldThing -> {
                modelThings.remove(oldThing);
                modelThings.add(t);
                logger.debug("model {} updated thing {}", modelName, t.getUID());
                    notifyListenersAboutUpdatedElement(oldThing, t);
    public void removedModel(String modelName, Collection<YamlThingDTO> elements) {
        Collection<Thing> modelThings = thingsMap.getOrDefault(modelName, List.of());
        elements.stream().map(this::buildThingUID).filter(Objects::nonNull).forEach(uid -> {
            modelThings.stream().filter(th -> th.getUID().equals(uid)).findFirst().ifPresentOrElse(oldThing -> {
                logger.debug("model {} removed thing {}", modelName, uid);
                    notifyListenersAboutRemovedElement(oldThing);
            }, () -> logger.debug("model {} thing {} not found", modelName, uid));
        if (modelThings.isEmpty()) {
            thingsMap.remove(modelName);
    public void addThingHandlerFactory(final ThingHandlerFactory thingHandlerFactory) {
        logger.debug("addThingHandlerFactory {}", thingHandlerFactory.getClass().getSimpleName());
        thingHandlerFactories.add(thingHandlerFactory);
        thingHandlerFactoryAdded(thingHandlerFactory);
    public void removeThingHandlerFactory(final ThingHandlerFactory thingHandlerFactory) {
        thingHandlerFactories.remove(thingHandlerFactory);
    public void setReadyService(final ReadyService readyService) {
        readyService.registerTracker(this);
    public void unsetReadyService(final ReadyService readyService) {
        if (XML_THING_TYPE.equals(readyMarker.getType())) {
            String bsn = readyMarker.getIdentifier();
            loadedXmlThingTypes.add(bsn);
            thingHandlerFactories.stream().filter(factory -> bsn.equals(getBundleName(factory))).forEach(factory -> {
                thingHandlerFactoryAdded(factory);
        loadedXmlThingTypes.remove(readyMarker.getIdentifier());
    private void thingHandlerFactoryAdded(ThingHandlerFactory handlerFactory) {
        logger.debug("thingHandlerFactoryAdded {} isThingHandlerFactoryReady={}",
                handlerFactory.getClass().getSimpleName(), isThingHandlerFactoryReady(handlerFactory));
        if (isThingHandlerFactoryReady(handlerFactory)) {
            if (!thingsMap.isEmpty()) {
                logger.debug("Refreshing models due to new thing handler factory {}",
                        handlerFactory.getClass().getSimpleName());
                thingsMap.keySet().stream().filter(name -> !isIsolatedModel(name)).forEach(modelName -> {
                    List<Thing> things = thingsMap.getOrDefault(modelName, List.of()).stream()
                            .filter(th -> handlerFactory.supportsThingType(th.getThingTypeUID())).toList();
                        logger.info("Refreshing YAML model {} ({} things with {})", modelName, things.size(),
                            if (!retryCreateThing(handlerFactory, thing.getThingTypeUID(), thing.getConfiguration(),
                                    thing.getUID(), thing.getBridgeUID())) {
                                // Possible cause: Asynchronous loading of the XML files
                                // Add the data to the queue in order to retry it later
                                        "ThingHandlerFactory \'{}\' claimed it can handle \'{}\' type but actually did not. Queued for later refresh.",
                                        handlerFactory.getClass().getSimpleName(), thing.getThingTypeUID());
                                queueRetryThingCreation(handlerFactory, thing.getThingTypeUID(),
                                        thing.getConfiguration(), thing.getUID(), thing.getBridgeUID());
                        logger.debug("No refresh needed from YAML model {}", modelName);
                logger.debug("No things yet loaded; no need to trigger a refresh due to new thing handler factory");
    private boolean retryCreateThing(ThingHandlerFactory handlerFactory, ThingTypeUID thingTypeUID,
        logger.trace("Retry creating thing {}", thingUID);
        Thing newThing = handlerFactory.createThing(thingTypeUID, new Configuration(configuration), thingUID,
        if (newThing != null) {
            logger.debug("Successfully loaded thing \'{}\' during retry", thingUID);
            Thing oldThing = null;
            for (Map.Entry<String, Collection<Thing>> entry : thingsMap.entrySet()) {
                Collection<Thing> modelThings = entry.getValue();
                oldThing = modelThings.stream().filter(t -> t.getUID().equals(newThing.getUID())).findFirst()
                    mergeThing(newThing, oldThing, false);
                    modelThings.add(newThing);
                    logger.debug("Refreshing thing \'{}\' after successful retry", newThing.getUID());
                    if (!ThingHelper.equals(oldThing, newThing) && !isIsolatedModel(entry.getKey())) {
                        notifyListenersAboutUpdatedElement(oldThing, newThing);
            if (oldThing == null) {
                logger.debug("Refreshing thing \'{}\' after retry failed because thing is not found",
                        newThing.getUID());
        return newThing != null;
    private boolean isThingHandlerFactoryReady(ThingHandlerFactory thingHandlerFactory) {
        String bundleName = getBundleName(thingHandlerFactory);
        return bundleName != null && loadedXmlThingTypes.contains(bundleName);
    private @Nullable String getBundleName(ThingHandlerFactory thingHandlerFactory) {
        Bundle bundle = bundleResolver.resolveBundle(thingHandlerFactory.getClass());
        return bundle == null ? null : bundle.getSymbolicName();
    private @Nullable ThingUID buildThingUID(YamlThingDTO thingDto) {
            return new ThingUID(thingDto.uid);
    private @Nullable Thing mapThing(YamlThingDTO thingDto, boolean isolatedModel) {
            ThingUID thingUID = new ThingUID(thingDto.uid);
            ThingTypeUID thingTypeUID = new ThingTypeUID(thingUID.getBindingId(), segments[1]);
            ThingType thingType = thingTypeRegistry.getThingType(thingTypeUID, localeProvider.getLocale());
            ThingUID bridgeUID = thingDto.bridge != null ? new ThingUID(thingDto.bridge) : null;
            Configuration configuration = new Configuration(thingDto.config);
            ThingBuilder thingBuilder = thingDto.isBridge() ? BridgeBuilder.create(thingTypeUID, thingUID)
                    : ThingBuilder.create(thingTypeUID, thingUID);
            thingBuilder.withLabel(
                    thingDto.label != null ? thingDto.label : (thingType != null ? thingType.getLabel() : null));
            thingBuilder.withLocation(thingDto.location);
            thingBuilder.withBridge(bridgeUID);
            thingBuilder.withConfiguration(configuration);
            List<Channel> channels = createChannels(!isolatedModel, thingTypeUID, thingUID,
                    thingDto.channels != null ? thingDto.channels : Map.of(),
                    thingType != null ? thingType.getChannelDefinitions() : List.of());
            thingBuilder.withChannels(channels);
            Thing thing = thingBuilder.build();
            Thing thingFromHandler = null;
            ThingHandlerFactory handlerFactory = thingHandlerFactories.stream()
                    .filter(thf -> isThingHandlerFactoryReady(thf) && thf.supportsThingType(thingTypeUID)).findFirst()
            if (handlerFactory != null) {
                thingFromHandler = handlerFactory.createThing(thingTypeUID, new Configuration(thingDto.config),
                        thingUID, bridgeUID);
                if (thingFromHandler != null) {
                    mergeThing(thingFromHandler, thing, isolatedModel);
                    logger.debug("Successfully loaded thing \'{}\'", thingUID);
                } else if (!isolatedModel) {
                            handlerFactory.getClass().getSimpleName(), thingTypeUID);
                    queueRetryThingCreation(handlerFactory, thingTypeUID, configuration, thingUID, bridgeUID);
            return thingFromHandler != null ? thingFromHandler : thing;
            logger.warn("Error creating thing '{}', thing will be ignored: {}", thingDto.uid, e.getMessage());
    private List<Channel> createChannels(boolean applyDefaultConfig, ThingTypeUID thingTypeUID, ThingUID thingUID,
            Map<String, YamlChannelDTO> channelsDto, List<ChannelDefinition> channelDefinitions) {
        Set<String> addedChannelIds = new HashSet<>();
        channelsDto.forEach((channelId, channelDto) -> {
            ChannelTypeUID channelTypeUID = channelDto.type == null ? null
                    : new ChannelTypeUID(thingUID.getBindingId(), channelDto.type);
            Channel channel = createChannel(applyDefaultConfig, thingUID, channelId, channelTypeUID,
                    channelDto.getKind(), channelDto.getItemType(), channelDto.label, channelDto.description, null,
                    new Configuration(channelDto.config), true);
            channels.add(channel);
            addedChannelIds.add(channelId);
        channelDefinitions.forEach(channelDef -> {
            String id = channelDef.getId();
            if (addedChannelIds.add(id)) {
                ChannelType channelType = channelTypeRegistry.getChannelType(channelDef.getChannelTypeUID(),
                        localeProvider.getLocale());
                    Channel channel = ChannelBuilder.create(new ChannelUID(thingUID, id), channelType.getItemType())
                            .withType(channelDef.getChannelTypeUID())
                            .withAutoUpdatePolicy(channelType.getAutoUpdatePolicy()).build();
                            "Could not create channel '{}' for thing '{}', because channel type '{}' could not be found.",
                            id, thingUID, channelDef.getChannelTypeUID());
    private Channel createChannel(boolean applyDefaultConfig, ThingUID thingUID, String channelId,
            @Nullable ChannelTypeUID channelTypeUID, ChannelKind channelKind, @Nullable String channelItemType,
            @Nullable String channelLabel, @Nullable String channelDescription,
            @Nullable AutoUpdatePolicy channelAutoUpdatePolicy, Configuration channelConfiguration,
            boolean ignoreMissingChannelType) {
        ChannelKind kind = channelKind;
        String itemType = channelItemType;
        String label = channelLabel;
        String description = channelDescription;
        AutoUpdatePolicy autoUpdatePolicy = channelAutoUpdatePolicy;
        Configuration configuration = new Configuration(channelConfiguration);
            ChannelType channelType = channelTypeRegistry.getChannelType(channelTypeUID, localeProvider.getLocale());
                kind = channelType.getKind();
                itemType = channelType.getItemType();
                autoUpdatePolicy = channelType.getAutoUpdatePolicy();
                URI descUriO = channelType.getConfigDescriptionURI();
                if (applyDefaultConfig && descUriO != null) {
                    ConfigUtil.applyDefaultConfiguration(configuration,
                            configDescriptionRegistry.getConfigDescription(descUriO));
            } else if (!ignoreMissingChannelType) {
                logger.warn("Channel type {} could not be found for thing '{}'.", channelTypeUID, thingUID);
        ChannelBuilder builder = ChannelBuilder.create(new ChannelUID(thingUID, channelId), itemType).withKind(kind)
                .withConfiguration(configuration).withType(channelTypeUID).withAutoUpdatePolicy(autoUpdatePolicy);
            builder.withLabel(label);
            builder.withDescription(description);
    private void mergeThing(Thing target, Thing source, boolean keepSourceConfig) {
        String label = source.getLabel();
            ThingType thingType = thingTypeRegistry.getThingType(target.getThingTypeUID(), localeProvider.getLocale());
            label = thingType != null ? thingType.getLabel() : null;
        target.setLabel(label);
        target.setLocation(source.getLocation());
        target.setBridgeUID(source.getBridgeUID());
        if (keepSourceConfig) {
            target.getConfiguration().setProperties(Map.of());
        Configuration thingConfig = processThingConfiguration(target.getThingTypeUID(), target.getUID(),
                source.getConfiguration());
        thingConfig.keySet().forEach(paramName -> {
            target.getConfiguration().put(paramName, thingConfig.get(paramName));
        List<Channel> channelsToAdd = new ArrayList<>();
        source.getChannels().forEach(channel -> {
            Channel targetChannel = target.getChannels().stream().filter(c -> c.getUID().equals(channel.getUID()))
            if (targetChannel != null) {
                    targetChannel.getConfiguration().setProperties(Map.of());
                Configuration channelConfig = processChannelConfiguration(targetChannel.getChannelTypeUID(),
                        targetChannel.getUID(), channel.getConfiguration());
                channelConfig.keySet().forEach(paramName -> {
                    targetChannel.getConfiguration().put(paramName, channelConfig.get(paramName));
                Channel newChannel = channel;
                if (channel.getChannelTypeUID() != null) {
                    // We create again the user defined channel because channel type was potentially not yet
                    // in the registry when the channel was initially created
                    Configuration channelConfig = processChannelConfiguration(channel.getChannelTypeUID(),
                            channel.getUID(), channel.getConfiguration());
                    newChannel = createChannel(!keepSourceConfig, target.getUID(), channel.getUID().getId(),
                            channel.getChannelTypeUID(), channel.getKind(), channel.getAcceptedItemType(),
                            channel.getLabel(), channel.getDescription(), channel.getAutoUpdatePolicy(), channelConfig,
                channelsToAdd.add(newChannel);
        // add the channels only defined in source list to the target list
        ThingHelper.addChannelsToThing(target, channelsToAdd);
    private void queueRetryThingCreation(ThingHandlerFactory handlerFactory, ThingTypeUID thingTypeUID,
        queue.add(new QueueContent(handlerFactory, thingTypeUID, configuration, thingUID, bridgeUID));
        Thread thread = lazyRetryThread;
        if (thread == null || !thread.isAlive()) {
            thread = new Thread(lazyRetryRunnable);
            lazyRetryThread = thread;
            thread.start();
    private Configuration processThingConfiguration(ThingTypeUID thingTypeUID, ThingUID thingUID,
            Configuration configuration) {
        Set<String> thingStringParams = !configuration.keySet().isEmpty()
                ? getThingConfigStringParameters(thingTypeUID, thingUID)
                : Set.of();
        return processConfiguration(thingUID, configuration, thingStringParams);
    private Configuration processChannelConfiguration(@Nullable ChannelTypeUID channelTypeUID, ChannelUID channelUID,
        Set<String> channelStringParams = !configuration.keySet().isEmpty()
                ? getChannelConfigStringParameters(channelTypeUID, channelUID)
        return processConfiguration(channelUID, configuration, channelStringParams);
    private Configuration processConfiguration(UID uid, Configuration configuration, Set<String> stringParameters) {
        Map<String, Object> params = new HashMap<>();
        configuration.keySet().forEach(name -> {
            Object valueIn = configuration.get(name);
            Object valueOut = valueIn;
            // For configuration parameter of type text only
            if (stringParameters.contains(name)) {
                if (valueIn != null && !(valueIn instanceof String)) {
                            "\"{}\": the value of the configuration TEXT parameter \"{}\" is not interpreted as a string and will be automatically converted. Enclose your value in double quotes to prevent conversion.",
                            uid, name);
                // if the value in YAML is an unquoted number, the value resulting of the parsing can then be
                // of type BigDecimal or BigInteger.
                // If the value is of type BigDecimal, we convert it into a String. If there is no decimal,
                // we convert it to an integer and return a String from that integer.
                // - Value 1 in YAML is converted into String "1"
                // - Value 1.0 in YAML is converted into String "1"
                // - Value 1.5 in YAML is converted into String "1.5"
                // If the value is not of type BigDecimal, it is kept unchanged. Conversion to a String will
                // be applied at a next step during configuration normalization.
                if (valueIn instanceof BigDecimal bigDecimalValue) {
                        valueOut = bigDecimalValue.stripTrailingZeros().scale() <= 0
                                ? String.valueOf(bigDecimalValue.toBigIntegerExact().longValue())
                                : bigDecimalValue.toString();
                        logger.trace("config param {}: {} ({}) converted into {} ({})", name, valueIn,
                                valueIn.getClass().getSimpleName(), valueOut, valueOut.getClass().getSimpleName());
                    } catch (ArithmeticException e) {
                        // Ignore error and return the original value
            params.put(name, valueOut);
        return new Configuration(params);
    private Set<String> getThingConfigStringParameters(ThingTypeUID thingTypeUID, ThingUID thingUID) {
        Set<String> params = new HashSet<>();
            params.addAll(getStringParameters(descURI));
            params.addAll(getStringParameters(new URI("thing:" + thingUID)));
            // Ignore exception, this will never happen with a valid thing UID
    private Set<String> getChannelConfigStringParameters(@Nullable ChannelTypeUID channelTypeUID,
            ChannelUID channelUID) {
        ChannelType channelType = channelTypeUID == null ? null : channelTypeRegistry.getChannelType(channelTypeUID);
            params.addAll(getStringParameters(new URI("channel:" + channelUID)));
            // Ignore exception, this will never happen with a valid channel UID
    private Set<String> getStringParameters(URI uri) {
        ConfigDescription configDescription = configDescriptionRegistry.getConfigDescription(uri);
            for (Entry<String, ConfigDescriptionParameter> param : configDescription.toParametersMap().entrySet()) {
                if (param.getValue().getType() == ConfigDescriptionParameter.Type.TEXT) {
                    params.add(param.getKey());
