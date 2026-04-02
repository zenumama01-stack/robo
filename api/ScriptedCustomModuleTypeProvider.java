 * The {@link ScriptedCustomModuleTypeProvider} is used in combination with the
 * {@link ScriptedCustomModuleHandlerFactory} to allow scripts to define custom types in the RuleManager. These
@Component(immediate = true, service = { ScriptedCustomModuleTypeProvider.class, ModuleTypeProvider.class })
public class ScriptedCustomModuleTypeProvider implements ModuleTypeProvider {
    private final Map<String, ModuleType> modulesTypes = new HashMap<>();
    private final Set<ProviderChangeListener<ModuleType>> listeners = new HashSet<>();
        return modulesTypes.values();
        this.listeners.add(listener);
        this.listeners.remove(listener);
    public <T extends ModuleType> T getModuleType(String uid, @Nullable Locale locale) {
        return (T) modulesTypes.get(uid);
    public <T extends ModuleType> Collection<T> getModuleTypes(@Nullable Locale locale) {
        return (Collection<T>) modulesTypes.values();
    public void addModuleType(ModuleType moduleType) {
        modulesTypes.put(moduleType.getUID(), moduleType);
        for (ProviderChangeListener<ModuleType> listener : listeners) {
            listener.added(this, moduleType);
    public void removeModuleType(ModuleType moduleType) {
        removeModuleType(moduleType.getUID());
    public void removeModuleType(String moduleTypeUID) {
        ModuleType element = modulesTypes.remove(moduleTypeUID);
        if (element != null) {
                listener.removed(this, element);
    public void updateModuleHandler(String uid) {
        ModuleType modType = modulesTypes.get(uid);
        if (modType != null) {
                listener.updated(this, modType, modType);
