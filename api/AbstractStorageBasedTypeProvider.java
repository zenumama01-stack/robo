import org.openhab.core.thing.internal.type.TriggerChannelTypeBuilderImpl;
import org.openhab.core.thing.type.ChannelGroupTypeBuilder;
import org.openhab.core.thing.type.ChannelGroupTypeProvider;
import org.openhab.core.thing.type.ChannelGroupTypeUID;
import org.openhab.core.thing.type.StateChannelTypeBuilder;
 * The {@link AbstractStorageBasedTypeProvider} is the base class for the implementation of a {@link Storage} based
 * {@link ThingTypeProvider}, {@link ChannelTypeProvider} and {@link ChannelGroupTypeProvider}
 * It can be subclassed by bindings that create {@link ThingType}s and {@link ChannelType}s on-the-fly and need to
 * persist those for future thing initializations
public abstract class AbstractStorageBasedTypeProvider
        implements ThingTypeProvider, ChannelTypeProvider, ChannelGroupTypeProvider {
    private final Storage<ThingTypeEntity> thingTypeEntityStorage;
    private final Storage<ChannelTypeEntity> channelTypeEntityStorage;
    private final Storage<ChannelGroupTypeEntity> channelGroupTypeEntityStorage;
     * Instantiate a new storage based type provider. The subclass needs to be a
     * {@link org.osgi.service.component.annotations.Component} and declare itself as {@link ThingTypeProvider} and/or
     * {@link ChannelTypeProvider} and/or {@link ChannelGroupTypeProvider}.
     * @param storageService a persistent {@link StorageService}
    protected AbstractStorageBasedTypeProvider(StorageService storageService) {
        String thingTypeStorageName = getClass().getName() + "-ThingType";
        String channelTypeStorageName = getClass().getName() + "-ChannelType";
        String channelGroupTypeStorageName = getClass().getName() + "-ChannelGroupType";
        ClassLoader classLoader = getClass().getClassLoader();
        thingTypeEntityStorage = storageService.getStorage(thingTypeStorageName, classLoader);
        channelTypeEntityStorage = storageService.getStorage(channelTypeStorageName, classLoader);
        channelGroupTypeEntityStorage = storageService.getStorage(channelGroupTypeStorageName, classLoader);
     * Add or update a {@link ThingType} to the storage
     * @param thingType the {@link ThingType} that needs to be stored
    public void putThingType(ThingType thingType) {
        thingTypeEntityStorage.put(thingType.getUID().toString(), mapToEntity(thingType));
     * Remove a {@link ThingType} from the storage
     * @param thingTypeUID the {@link ThingTypeUID} of the thing type
    public void removeThingType(ThingTypeUID thingTypeUID) {
        thingTypeEntityStorage.remove(thingTypeUID.toString());
     * Add or update a {@link ChannelType} to the storage
     * @param channelType the {@link ChannelType} that needs to be stored
    public void putChannelType(ChannelType channelType) {
        channelTypeEntityStorage.put(channelType.getUID().toString(), mapToEntity(channelType));
     * Remove a {@link ChannelType} from the storage
     * @param channelTypeUID the {@link ChannelTypeUID} of the channel type
    public void removeChannelType(ChannelTypeUID channelTypeUID) {
        channelTypeEntityStorage.remove(channelTypeUID.toString());
     * Add or update a {@link ChannelGroupType} to the storage
     * @param channelGroupType the {@link ChannelType} that needs to be stored
    public void putChannelGroupType(ChannelGroupType channelGroupType) {
        channelGroupTypeEntityStorage.put(channelGroupType.getUID().toString(), mapToEntity(channelGroupType));
     * Remove a {@link ChannelGroupType} from the storage
     * @param channelGroupTypeUID the {@link ChannelGroupTypeUID} of the channel type
    public void removeChannelGroupType(ChannelGroupTypeUID channelGroupTypeUID) {
        channelGroupTypeEntityStorage.remove(channelGroupTypeUID.toString());
    public Collection<ThingType> getThingTypes(@Nullable Locale locale) {
        return thingTypeEntityStorage.stream().map(Map.Entry::getValue).filter(Objects::nonNull)
                .map(Objects::requireNonNull).map(AbstractStorageBasedTypeProvider::mapFromEntity).toList();
    public @Nullable ThingType getThingType(ThingTypeUID thingTypeUID, @Nullable Locale locale) {
        ThingTypeEntity entity = thingTypeEntityStorage.get(thingTypeUID.toString());
            return mapFromEntity(entity);
        return channelTypeEntityStorage.stream().map(Map.Entry::getValue).filter(Objects::nonNull)
        ChannelTypeEntity entity = channelTypeEntityStorage.get(channelTypeUID.toString());
    public Collection<ChannelGroupType> getChannelGroupTypes(@Nullable Locale locale) {
        return channelGroupTypeEntityStorage.stream().map(Map.Entry::getValue).filter(Objects::nonNull)
    public @Nullable ChannelGroupType getChannelGroupType(ChannelGroupTypeUID channelGroupTypeUID,
        ChannelGroupTypeEntity entity = channelGroupTypeEntityStorage.get(channelGroupTypeUID.toString());
    static ThingTypeEntity mapToEntity(ThingType thingType) {
        ThingTypeEntity entity = new ThingTypeEntity();
        entity.uid = thingType.getUID();
        entity.label = thingType.getLabel();
        entity.description = thingType.getDescription();
        entity.supportedBridgeTypeRefs = thingType.getSupportedBridgeTypeUIDs();
        entity.configDescriptionUri = thingType.getConfigDescriptionURI();
        entity.category = thingType.getCategory();
        entity.channelGroupDefinitions = thingType.getChannelGroupDefinitions().stream()
                .map(AbstractStorageBasedTypeProvider::mapToEntity).toList();
        entity.channelDefinitions = thingType.getChannelDefinitions().stream()
        entity.representationProperty = thingType.getRepresentationProperty();
        entity.properties = thingType.getProperties();
        entity.isListed = thingType.isListed();
        entity.extensibleChannelTypeIds = thingType.getExtensibleChannelTypeIds();
        entity.isBridge = thingType instanceof BridgeType;
    static ChannelDefinitionEntity mapToEntity(ChannelDefinition channelDefinition) {
        ChannelDefinitionEntity entity = new ChannelDefinitionEntity();
        entity.id = channelDefinition.getId();
        entity.uid = channelDefinition.getChannelTypeUID();
        entity.label = channelDefinition.getLabel();
        entity.description = channelDefinition.getDescription();
        entity.properties = channelDefinition.getProperties();
        entity.autoUpdatePolicy = channelDefinition.getAutoUpdatePolicy();
    static ChannelGroupDefinitionEntity mapToEntity(ChannelGroupDefinition channelGroupDefinition) {
        ChannelGroupDefinitionEntity entity = new ChannelGroupDefinitionEntity();
        entity.id = channelGroupDefinition.getId();
        entity.typeUid = channelGroupDefinition.getTypeUID();
        entity.label = channelGroupDefinition.getLabel();
        entity.description = channelGroupDefinition.getDescription();
    static ChannelTypeEntity mapToEntity(ChannelType channelType) {
        ChannelTypeEntity entity = new ChannelTypeEntity();
        entity.uid = channelType.getUID();
        entity.label = channelType.getLabel();
        entity.description = channelType.getDescription();
        entity.configDescriptionURI = channelType.getConfigDescriptionURI();
        entity.advanced = channelType.isAdvanced();
        entity.itemType = channelType.getItemType();
        entity.kind = channelType.getKind();
        entity.tags = channelType.getTags();
        entity.category = channelType.getCategory();
        StateDescription stateDescription = channelType.getState();
            StateDescriptionFragment fragment = StateDescriptionFragmentBuilder.create(stateDescription).build();
            entity.stateDescriptionFragment = mapToEntity(fragment);
            entity.stateDescriptionFragment = null;
        entity.commandDescription = channelType.getCommandDescription();
        entity.event = channelType.getEvent();
        entity.autoUpdatePolicy = channelType.getAutoUpdatePolicy();
        entity.unitHint = channelType.getUnitHint();
    static ChannelGroupTypeEntity mapToEntity(ChannelGroupType channelGroupType) {
        ChannelGroupTypeEntity entity = new ChannelGroupTypeEntity();
        entity.uid = channelGroupType.getUID();
        entity.label = channelGroupType.getLabel();
        entity.description = channelGroupType.getDescription();
        entity.category = channelGroupType.getCategory();
        entity.channelDefinitions = channelGroupType.getChannelDefinitions().stream()
    static StateDescriptionFragmentEntity mapToEntity(StateDescriptionFragment fragment) {
        StateDescriptionFragmentEntity entity = new StateDescriptionFragmentEntity();
        entity.maximum = fragment.getMaximum();
        entity.minimum = fragment.getMinimum();
        entity.step = fragment.getStep();
        entity.options = fragment.getOptions();
        entity.pattern = fragment.getPattern();
        entity.isReadOnly = fragment.isReadOnly();
    static ThingType mapFromEntity(ThingTypeEntity entity) {
        ThingTypeBuilder builder = ThingTypeBuilder.instance(entity.uid, entity.label)
                .withSupportedBridgeTypeUIDs(entity.supportedBridgeTypeRefs).withProperties(entity.properties)
                .withChannelDefinitions(entity.channelDefinitions.stream()
                        .map(AbstractStorageBasedTypeProvider::mapFromEntity).toList())
                .withChannelGroupDefinitions(entity.channelGroupDefinitions.stream()
                .isListed(entity.isListed).withExtensibleChannelTypeIds(entity.extensibleChannelTypeIds);
        if (entity.description != null) {
            builder.withDescription(Objects.requireNonNull(entity.description));
        if (entity.category != null) {
            builder.withCategory(Objects.requireNonNull(entity.category));
        if (entity.configDescriptionUri != null) {
            builder.withConfigDescriptionURI(Objects.requireNonNull(entity.configDescriptionUri));
        if (entity.representationProperty != null) {
            builder.withRepresentationProperty(Objects.requireNonNull(entity.representationProperty));
        return entity.isBridge ? builder.buildBridge() : builder.build();
    static ChannelDefinition mapFromEntity(ChannelDefinitionEntity entity) {
        return new ChannelDefinitionBuilder(entity.id, entity.uid).withLabel(entity.label)
                .withDescription(entity.description).withProperties(entity.properties)
                .withAutoUpdatePolicy(entity.autoUpdatePolicy).build();
    static ChannelGroupDefinition mapFromEntity(ChannelGroupDefinitionEntity entity) {
        return new ChannelGroupDefinition(entity.id, entity.typeUid, entity.label, entity.description);
    static ChannelType mapFromEntity(ChannelTypeEntity entity) {
        ChannelTypeBuilder<?> builder = (entity.kind == ChannelKind.STATE)
                ? ChannelTypeBuilder.state(entity.uid, entity.label, Objects.requireNonNull(entity.itemType))
                : ChannelTypeBuilder.trigger(entity.uid, entity.label);
        builder.isAdvanced(entity.advanced).withTags(entity.tags);
        if (entity.configDescriptionURI != null) {
            builder.withConfigDescriptionURI(Objects.requireNonNull(entity.configDescriptionURI));
        if (builder instanceof StateChannelTypeBuilder stateBuilder) {
            if (entity.stateDescriptionFragment != null) {
                stateBuilder.withStateDescriptionFragment(
                        mapFromEntity(Objects.requireNonNull(entity.stateDescriptionFragment)));
            if (entity.commandDescription != null) {
                stateBuilder.withCommandDescription(Objects.requireNonNull(entity.commandDescription));
            if (entity.autoUpdatePolicy != null) {
                stateBuilder.withAutoUpdatePolicy(Objects.requireNonNull(entity.autoUpdatePolicy));
            if (entity.unitHint != null) {
                stateBuilder.withUnitHint(entity.unitHint);
        if (builder instanceof TriggerChannelTypeBuilderImpl triggerBuilder) {
            if (entity.event != null) {
                triggerBuilder.withEventDescription(Objects.requireNonNull(entity.event));
    static ChannelGroupType mapFromEntity(ChannelGroupTypeEntity entity) {
        ChannelGroupTypeBuilder builder = ChannelGroupTypeBuilder.instance(entity.uid, entity.label)
                        .map(AbstractStorageBasedTypeProvider::mapFromEntity).toList());
    static StateDescriptionFragment mapFromEntity(StateDescriptionFragmentEntity entity) {
        StateDescriptionFragmentBuilder builder = StateDescriptionFragmentBuilder.create();
        if (entity.maximum != null) {
            builder.withMaximum(Objects.requireNonNull(entity.maximum));
        if (entity.minimum != null) {
            builder.withMinimum(Objects.requireNonNull(entity.minimum));
        if (entity.step != null) {
            builder.withStep(Objects.requireNonNull(entity.step));
        if (entity.options != null) {
            builder.withOptions(Objects.requireNonNull(entity.options));
        if (entity.pattern != null) {
            builder.withPattern(Objects.requireNonNull(entity.pattern));
        builder.withReadOnly(entity.isReadOnly);
    static class ThingTypeEntity {
        public @NonNullByDefault({}) ThingTypeUID uid;
        public @NonNullByDefault({}) String label;
        public @Nullable String category;
        public List<String> supportedBridgeTypeRefs = List.of();
        public @Nullable URI configDescriptionUri;
        public List<String> extensibleChannelTypeIds = List.of();
        public List<ChannelGroupDefinitionEntity> channelGroupDefinitions = List.of();
        public List<ChannelDefinitionEntity> channelDefinitions = List.of();
        public Map<String, String> properties = Map.of();
        public boolean isListed = false;
        public boolean isBridge = false;
        public @Nullable String semanticEquipmentTag;
    static class ChannelDefinitionEntity {
        public @NonNullByDefault({}) String id;
        public @NonNullByDefault({}) ChannelTypeUID uid;
        public @Nullable AutoUpdatePolicy autoUpdatePolicy;
    static class ChannelGroupDefinitionEntity {
        public @NonNullByDefault({}) ChannelGroupTypeUID typeUid;
    static class ChannelTypeEntity {
        public @Nullable URI configDescriptionURI;
        public boolean advanced;
        public @Nullable String itemType;
        public @NonNullByDefault({}) ChannelKind kind;
        public Set<String> tags = Set.of();
        public @Nullable StateDescriptionFragmentEntity stateDescriptionFragment;
        public @Nullable CommandDescription commandDescription;
        public @Nullable EventDescription event;
        public @Nullable String unitHint;
    static class ChannelGroupTypeEntity {
        public @NonNullByDefault({}) ChannelGroupTypeUID uid;
        private @Nullable String category;
    static class StateDescriptionFragmentEntity {
        public @Nullable BigDecimal maximum;
        public @Nullable BigDecimal minimum;
        public @Nullable BigDecimal step;
        public @Nullable List<StateOption> options;
        public @Nullable String pattern;
        public boolean isReadOnly = false;
