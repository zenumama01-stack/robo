package org.openhab.core.automation.internal.module.provider;
import org.openhab.core.automation.AnnotatedActions;
import org.openhab.core.automation.internal.module.handler.AnnotationActionHandler;
import org.openhab.core.automation.module.provider.ModuleInformation;
import org.openhab.core.automation.module.provider.i18n.ModuleTypeI18nService;
 * This provider parses annotated {@link AnnotatedActions}s and creates action module types, as well as their handlers
 * from them
 * @author Laurent Garnier - Injected components AnnotationActionModuleTypeHelper and ActionInputsHelper
@Component(service = { ModuleTypeProvider.class, ModuleHandlerFactory.class })
public class AnnotatedActionModuleTypeProvider extends BaseModuleHandlerFactory implements ModuleTypeProvider {
    private final Collection<ProviderChangeListener<ModuleType>> changeListeners = ConcurrentHashMap.newKeySet();
    private final Map<String, Set<ModuleInformation>> moduleInformation = new ConcurrentHashMap<>();
    private final AnnotationActionModuleTypeHelper helper;
    private final ModuleTypeI18nService moduleTypeI18nService;
    public AnnotatedActionModuleTypeProvider(final @Reference ModuleTypeI18nService moduleTypeI18nService,
            final @Reference AnnotationActionModuleTypeHelper helper,
            final @Reference ActionInputsHelper actionInputsHelper) {
        this.moduleTypeI18nService = moduleTypeI18nService;
        this.helper = helper;
        moduleInformation.clear();
        for (String moduleUID : moduleInformation.keySet()) {
            ModuleType mt = helper.buildModuleType(moduleUID, moduleInformation);
        return (T) localizeModuleType(uid, locale);
        List<T> result = new ArrayList<>();
        for (Entry<String, Set<ModuleInformation>> entry : moduleInformation.entrySet()) {
            ModuleType localizedModuleType = localizeModuleType(entry.getKey(), locale);
            if (localizedModuleType != null) {
                result.add((T) localizedModuleType);
    private @Nullable ModuleType localizeModuleType(String uid, @Nullable Locale locale) {
        Set<ModuleInformation> mis = moduleInformation.get(uid);
        if (mis != null && !mis.isEmpty()) {
            ModuleInformation mi = mis.iterator().next();
            Bundle bundle = FrameworkUtil.getBundle(mi.getActionProvider().getClass());
            ModuleType mt = helper.buildModuleType(uid, moduleInformation);
            return moduleTypeI18nService.getModuleTypePerLocale(mt, locale, bundle);
    public void addActionProvider(AnnotatedActions actionProvider, Map<String, Object> properties) {
        Collection<ModuleInformation> moduleInformations = helper.parseAnnotations(actionProvider);
        String configName = getConfigNameFromService(properties);
        for (ModuleInformation mi : moduleInformations) {
            mi.setConfigName(configName);
            ModuleType oldType = null;
            if (moduleInformation.containsKey(mi.getUID())) {
                oldType = helper.buildModuleType(mi.getUID(), moduleInformation);
                Set<ModuleInformation> availableModuleConfigs = moduleInformation.get(mi.getUID());
                availableModuleConfigs.add(mi);
                Set<ModuleInformation> configs = ConcurrentHashMap.newKeySet();
                configs.add(mi);
                moduleInformation.put(mi.getUID(), configs);
            ModuleType mt = helper.buildModuleType(mi.getUID(), moduleInformation);
                for (ProviderChangeListener<ModuleType> l : changeListeners) {
                    if (oldType != null) {
                        l.updated(this, oldType, mt);
                        l.added(this, mt);
    public void removeActionProvider(AnnotatedActions actionProvider, Map<String, Object> properties) {
            if (availableModuleConfigs != null) {
                if (availableModuleConfigs.size() > 1) {
                    availableModuleConfigs.remove(mi);
                    moduleInformation.remove(mi.getUID());
                // localize moduletype -> remove from map
                            l.removed(this, mt);
    private @Nullable String getConfigNameFromService(Map<String, Object> properties) {
        Object o = properties.get(OpenHAB.SERVICE_CONTEXT);
        String configName = null;
        if (o instanceof String string) {
            configName = string;
        return configName;
        return moduleInformation.keySet();
        if (module instanceof Action actionModule) {
            if (moduleInformation.containsKey(actionModule.getTypeUID())) {
                ModuleInformation finalMI = helper.getModuleInformationForIdentifier(actionModule, moduleInformation,
                if (finalMI != null) {
                    ActionType moduleType = helper.buildModuleType(module.getTypeUID(), moduleInformation);
                    if (moduleType == null) {
                    return new AnnotationActionHandler(actionModule, moduleType, finalMI.getMethod(),
                            finalMI.getActionProvider(), actionInputsHelper);
