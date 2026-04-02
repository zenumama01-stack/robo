import org.osgi.framework.ServiceRegistration;
 * This class is implementation of {@link ModuleTypeProvider}. It extends functionality of
 * {@link AbstractCommandProvider}.
 * It is responsible for execution of {@link AutomationCommandsPluggable}, corresponding to the {@link ModuleType}s:
 * <li>imports the {@link ModuleType}s from local files or from URL resources
 * <li>provides functionality for persistence of the {@link ModuleType}s
 * <li>removes the {@link ModuleType}s and their persistence
 * <li>lists the {@link ModuleType}s and their details
 * accordingly to the used command.
public class CommandlineModuleTypeProvider extends AbstractCommandProvider<ModuleType> implements ModuleTypeProvider {
     * This field holds a reference to the {@link TemplateProvider} service registration.
    protected @Nullable ServiceRegistration mtpReg;
     * This constructor creates instances of this particular implementation of {@link ModuleTypeProvider}. It does not
     * add any new functionality to the constructors of the providers. Only provides consistency by invoking the
     * parent's constructor.
     * @param bundleContext is the {@code BundleContext}, used for creating a tracker for {@link Parser} services.
     * @param moduleTypeRegistry a ModuleTypeRegistry service
    public CommandlineModuleTypeProvider(BundleContext bundleContext, ModuleTypeRegistry moduleTypeRegistry) {
        super(bundleContext);
        mtpReg = bundleContext.registerService(ModuleTypeProvider.class.getName(), this, null);
     * This method differentiates what type of {@link Parser}s is tracked by the tracker.
     * For this concrete provider, this type is a {@link ModuleType} {@link Parser}.
     * @see AbstractCommandProvider#addingService(org.osgi.framework.ServiceReference)
    public @Nullable Object addingService(@SuppressWarnings("rawtypes") @Nullable ServiceReference reference) {
        if (reference != null && Parser.PARSER_MODULE_TYPE.equals(reference.getProperty(Parser.PARSER_TYPE))) {
            return super.addingService(reference);
     * This method is responsible for exporting a set of ModuleTypes in a specified file.
     * @param parserType is relevant to the format that you need for conversion of the ModuleTypes in text.
     * @param set a set of ModuleTypes to export.
     * @param file a specified file for export.
     * @throws Exception when I/O operation has failed or has been interrupted or generating of the text fails
     * @see AutomationCommandsPluggable#exportModuleTypes(String, Set, File)
        return super.exportData(parserType, set, file);
     * This method is responsible for importing a set of ModuleTypes from a specified file or URL resource.
     * @param url a specified URL for import.
     * @throws IOException when I/O operation has failed or has been interrupted.
     * @throws ParsingException when parsing of the text fails for some reasons.
     * @see AutomationCommandsPluggable#importModuleTypes(String, URL)
    public Set<ModuleType> importModuleTypes(String parserType, URL url) throws IOException, ParsingException {
        Parser<ModuleType> parser = parsers.get(parserType);
            InputStream is = url.openStream();
            BufferedInputStream bis = new BufferedInputStream(is);
            InputStreamReader inputStreamReader = new InputStreamReader(bis);
                return importData(url, parser, inputStreamReader);
                inputStreamReader.close();
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.MODULE_TYPE, null,
                    new IllegalArgumentException("Parser " + parserType + " not available")));
            return providedObjectsHolder.get(uid);
            return !providedObjectsHolder.isEmpty() ? providedObjectsHolder.values() : List.of();
     * @param url is a specified file or URL resource.
     * @return the string <b>SUCCESS</b>.
    public String remove(URL url) {
        List<String> portfolio;
            portfolio = providerPortfolio.remove(url);
        if (portfolio != null && !portfolio.isEmpty()) {
                for (String uid : portfolio) {
                    notifyListeners(providedObjectsHolder.remove(uid));
        if (mtpReg != null) {
            mtpReg.unregister();
            mtpReg = null;
    protected Set<ModuleType> importData(URL url, Parser<ModuleType> parser, InputStreamReader inputStreamReader)
            throws ParsingException {
        Set<ModuleType> providedObjects = parser.parse(inputStreamReader);
        if (providedObjects != null && !providedObjects.isEmpty()) {
            String uid;
            List<String> portfolio = new ArrayList<>();
                providerPortfolio.put(url, portfolio);
            List<ParsingNestedException> importDataExceptions = new ArrayList<>();
            for (ModuleType providedObject : providedObjects) {
                List<ParsingNestedException> exceptions = new ArrayList<>();
                uid = providedObject.getUID();
                checkExistence(uid, exceptions);
                if (exceptions.isEmpty()) {
                    portfolio.add(uid);
                        notifyListeners(providedObjectsHolder.put(uid, providedObject), providedObject);
                    importDataExceptions.addAll(exceptions);
            if (!importDataExceptions.isEmpty()) {
                throw new ParsingException(importDataExceptions);
        return providedObjects;
     * This method is responsible for checking the existence of {@link ModuleType}s with the same
     * UIDs before these objects to be added in the system.
     * @param uid UID of the newly created {@link ModuleType}, which to be checked.
     * @param exceptions accumulates exceptions if {@link ModuleType} with the same UID exists.
    protected void checkExistence(String uid, List<ParsingNestedException> exceptions) {
        if (this.moduleTypeRegistry == null) {
            exceptions.add(new ParsingNestedException(ParsingNestedException.MODULE_TYPE, uid,
                    new IllegalArgumentException("Failed to create Module Type with UID \"" + uid
                            + "\"! Can't guarantee yet that other Module Type with the same UID does not exist.")));
        if (moduleTypeRegistry.get(uid) != null) {
                    new IllegalArgumentException("Module Type with UID \"" + uid
                            + "\" already exists! Failed to create a second with the same UID!")));
        return new LinkedList<>(providedObjectsHolder.values());
        synchronized (listeners) {
    protected void notifyListeners(@Nullable ModuleType oldElement, ModuleType newElement) {
                if (oldElement != null) {
                    listener.updated(this, oldElement, newElement);
                listener.added(this, newElement);
    protected void notifyListeners(@Nullable ModuleType removedObject) {
        if (removedObject != null) {
                    listener.removed(this, removedObject);
