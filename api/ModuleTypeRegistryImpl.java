package org.openhab.core.automation.internal.type;
 * The implementation of {@link ModuleTypeRegistry} that is registered as a service.
@Component(service = ModuleTypeRegistry.class, immediate = true)
public class ModuleTypeRegistryImpl extends AbstractRegistry<ModuleType, String, ModuleTypeProvider>
        implements ModuleTypeRegistry {
    public ModuleTypeRegistryImpl() {
        super(ModuleTypeProvider.class);
    protected void addProvider(Provider<ModuleType> provider) {
        if (provider instanceof ModuleTypeProvider) {
    public @Nullable ModuleType get(String typeUID) {
        return get(typeUID, null);
    public <T extends ModuleType> @Nullable T get(String moduleTypeUID, @Nullable Locale locale) {
        Entry<Provider<ModuleType>, ModuleType> mType = getValueAndProvider(moduleTypeUID);
        if (mType == null) {
            ModuleType mt = locale == null ? mType.getValue()
                    : ((ModuleTypeProvider) mType.getKey()).getModuleType(mType.getValue().getUID(), locale);
            return (T) createCopy(mt);
    public <T extends ModuleType> Collection<T> getByTag(@Nullable String moduleTypeTag) {
        return getByTag(moduleTypeTag, null);
    public <T extends ModuleType> Collection<T> getByTag(@Nullable String moduleTypeTag, @Nullable Locale locale) {
        Collection<T> result = new ArrayList<>(20);
        forEach((provider, mType) -> {
            ModuleType mt = locale == null ? mType
                    : ((ModuleTypeProvider) provider).getModuleType(mType.getUID(), locale);
            if (mt != null && (moduleTypeTag == null || mt.getTags().contains(moduleTypeTag))) {
                T mtCopy = (T) createCopy(mt);
                if (mtCopy != null) {
                    result.add(mtCopy);
    public <T extends ModuleType> Collection<T> getByTags(String... tags) {
    public <T extends ModuleType> Collection<T> getByTags(@Nullable Locale locale, String... tags) {
            if (mt != null && (mt.getTags().containsAll(tagSet))) {
    public Collection<TriggerType> getTriggers(@Nullable Locale locale, String... tags) {
        Collection<ModuleType> moduleTypes = getByTags(locale, tags);
        Collection<TriggerType> triggerTypes = new ArrayList<>();
        for (ModuleType mt : moduleTypes) {
            if (mt instanceof TriggerType type) {
                triggerTypes.add(type);
        return triggerTypes;
    public Collection<TriggerType> getTriggers(String... tags) {
        Collection<ModuleType> moduleTypes = getByTags(tags);
    public Collection<ConditionType> getConditions(String... tags) {
        Collection<ConditionType> conditionTypes = new ArrayList<>();
            if (mt instanceof ConditionType type) {
                conditionTypes.add(type);
        return conditionTypes;
    public Collection<ConditionType> getConditions(@Nullable Locale locale, String... tags) {
    public Collection<ActionType> getActions(String... tags) {
        Collection<ActionType> actionTypes = new ArrayList<>();
            if (mt instanceof ActionType type) {
                actionTypes.add(type);
        return actionTypes;
    public Collection<ActionType> getActions(@Nullable Locale locale, String... tags) {
    private @Nullable ModuleType createCopy(@Nullable ModuleType mType) {
        ModuleType result;
        if (mType instanceof CompositeTriggerType m) {
            result = new CompositeTriggerType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getOutputs(),
                    new ArrayList<>(m.getChildren()));
        } else if (mType instanceof TriggerType m) {
            result = new TriggerType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getOutputs());
        } else if (mType instanceof CompositeConditionType m) {
            result = new CompositeConditionType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getInputs(),
        } else if (mType instanceof ConditionType m) {
            result = new ConditionType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getInputs());
        } else if (mType instanceof CompositeActionType m) {
            result = new CompositeActionType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getInputs(), m.getOutputs(),
        } else if (mType instanceof ActionType m) {
            result = new ActionType(mType.getUID(), mType.getConfigurationDescriptions(), mType.getLabel(),
                    mType.getDescription(), mType.getTags(), mType.getVisibility(), m.getInputs(), m.getOutputs());
            throw new IllegalArgumentException("Invalid template type:" + mType);
