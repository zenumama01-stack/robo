 * This class is implementation of {@link ModuleTypeProvider}. It serves for providing {@link ModuleType}s by loading
 * bundle resources. It extends functionality of {@link AbstractResourceBundleProvider} by specifying:
 * <li>the path to resources, corresponding to the {@link ModuleType}s - root directory
 * {@link AbstractResourceBundleProvider#ROOT_DIRECTORY} with sub-directory "moduletypes".
 * <li>type of the {@link Parser}s, corresponding to the {@link ModuleType}s - {@link Parser#PARSER_MODULE_TYPE}
 * <li>specific functionality for loading the {@link ModuleType}s
 * <li>tracking the managing service of the {@link ModuleType}s.
@Component(immediate = true, service = { ModuleTypeProvider.class, Provider.class }, property = "provider.type=bundle")
public class ModuleTypeResourceBundleProvider extends AbstractResourceBundleProvider<ModuleType>
        implements ModuleTypeProvider {
     * This constructor is responsible for initializing the path to resources and tracking the
     * {@link ModuleTypeRegistry}.
    public ModuleTypeResourceBundleProvider(final @Reference ModuleTypeI18nService moduleTypeI18nService) {
        super(ROOT_DIRECTORY + "/moduletypes/");
    @Reference(cardinality = ReferenceCardinality.AT_LEAST_ONE, policy = ReferencePolicy.DYNAMIC, target = "(parser.type=parser.module.type)")
    protected void addParser(Parser<ModuleType> parser, Map<String, String> properties) {
        super.addParser(parser, properties);
    protected void removeParser(Parser<ModuleType> parser, Map<String, String> properties) {
        super.removeParser(parser, properties);
        return (T) getPerLocale(providedObjectsHolder.get(uid), locale);
        List<ModuleType> moduleTypesList = new ArrayList<>();
        for (ModuleType mt : providedObjectsHolder.values()) {
            ModuleType mtPerLocale = getPerLocale(mt, locale);
            if (mtPerLocale != null) {
                moduleTypesList.add(mtPerLocale);
        return moduleTypesList;
    protected String getUID(ModuleType parsedObject) {
        return parsedObject.getUID();
     * This method is used to localize the {@link ModuleType}s.
     * @param defModuleType is the {@link ModuleType} that must be localized.
     * @param locale represents a specific geographical, political, or cultural region.
     * @return the localized {@link ModuleType}.
    private @Nullable ModuleType getPerLocale(@Nullable ModuleType defModuleType, @Nullable Locale locale) {
        if (defModuleType == null) {
        String uid = defModuleType.getUID();
        Bundle bundle = getBundle(uid);
        return bundle != null ? moduleTypeI18nService.getModuleTypePerLocale(defModuleType, locale, bundle) : null;
