 * This class is implementation of {@link TemplateProvider}. It extends functionality of {@link AbstractCommandProvider}
 * It is responsible for execution of {@link AutomationCommandsPluggable}, corresponding to the {@link RuleTemplate}s:
 * <li>imports the {@link RuleTemplate}s from local files or from URL resources
 * <li>provides functionality for persistence of the {@link RuleTemplate}s
 * <li>removes the {@link RuleTemplate}s and their persistence
public class CommandlineTemplateProvider extends AbstractCommandProvider<RuleTemplate> implements RuleTemplateProvider {
     * This field holds a reference to the {@link ModuleTypeProvider} service registration.
    protected @Nullable ServiceRegistration tpReg;
    private final TemplateRegistry<RuleTemplate> templateRegistry;
     * This constructor creates instances of this particular implementation of {@link TemplateProvider}. It does not add
     * any new functionality to the constructors of the providers. Only provides consistency by invoking the parent's
     * @param bundleContext is the {@link BundleContext}, used for creating a tracker for {@link Parser} services.
    public CommandlineTemplateProvider(BundleContext bundleContext, TemplateRegistry<RuleTemplate> templateRegistry) {
        tpReg = bundleContext.registerService(RuleTemplateProvider.class.getName(), this, null);
     * For this concrete provider, this type is a {@link RuleTemplate} {@link Parser}.
        if (reference != null && Parser.PARSER_TEMPLATE.equals(reference.getProperty(Parser.PARSER_TYPE))) {
     * This method is responsible for exporting a set of RuleTemplates in a specified file.
     * @param parserType is relevant to the format that you need for conversion of the RuleTemplates in text.
     * @param set a set of RuleTemplates to export.
     * @see AutomationCommandsPluggable#exportTemplates(String, Set, File)
     * This method is responsible for importing a set of RuleTemplates from a specified file or URL resource.
     * @see AutomationCommandsPluggable#importTemplates(String, URL)
    public Set<RuleTemplate> importTemplates(String parserType, URL url) throws IOException, ParsingException {
        Parser<RuleTemplate> parser = parsers.get(parserType);
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.TEMPLATE, null,
            return providedObjectsHolder.values();
        if (tpReg != null) {
            tpReg.unregister();
            tpReg = null;
    protected Set<RuleTemplate> importData(URL url, Parser<RuleTemplate> parser, InputStreamReader inputStreamReader)
        Set<RuleTemplate> providedObjects = parser.parse(inputStreamReader);
            for (RuleTemplate ruleT : providedObjects) {
                String uid = ruleT.getUID();
                        notifyListeners(providedObjectsHolder.put(uid, ruleT), ruleT);
     * This method is responsible for checking the existence of {@link Template}s with the same
     * @param uid UID of the newly created {@link Template}, which to be checked.
        if (templateRegistry == null) {
            exceptions.add(new ParsingNestedException(ParsingNestedException.TEMPLATE, uid,
                    new IllegalArgumentException("Failed to create Rule Template with UID \"" + uid
                            + "\"! Can't guarantee yet that other Rule Template with the same UID does not exist.")));
        if (templateRegistry.get(uid) != null) {
                    new IllegalArgumentException("Rule Template with UID \"" + uid
    public Collection<RuleTemplate> getAll() {
    public void addProviderChangeListener(ProviderChangeListener<RuleTemplate> listener) {
    public void removeProviderChangeListener(ProviderChangeListener<RuleTemplate> listener) {
    protected void notifyListeners(@Nullable RuleTemplate oldElement, RuleTemplate newElement) {
            for (ProviderChangeListener<RuleTemplate> listener : listeners) {
    protected void notifyListeners(@Nullable RuleTemplate removedObject) {
