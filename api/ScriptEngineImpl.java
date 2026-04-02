import org.eclipse.xtext.diagnostics.Severity;
import org.eclipse.xtext.util.CancelIndicator;
import org.eclipse.xtext.util.StringInputStream;
import org.eclipse.xtext.validation.CheckMode;
import org.eclipse.xtext.validation.IResourceValidator;
import org.eclipse.xtext.validation.Issue;
import org.openhab.core.model.script.ScriptStandaloneSetup;
import org.openhab.core.model.script.runtime.ScriptRuntime;
 * This is the implementation of a {@link ScriptEngine} which is made available as an OSGi service.
 * @author Oliver Libutzki - Reorganization of Guice injection
public class ScriptEngineImpl implements ScriptEngine, ModelParser {
    protected XtextResourceSet resourceSet;
    private final Logger logger = LoggerFactory.getLogger(ScriptEngineImpl.class);
    private ScriptServiceUtil scriptServiceUtil;
    public ScriptEngineImpl() {
        ScriptStandaloneSetup.doSetup(scriptServiceUtil, this);
        logger.debug("Registered 'script' configuration parser");
    private XtextResourceSet getResourceSet() {
        if (resourceSet == null) {
            resourceSet = ScriptStandaloneSetup.getInjector().getInstance(XtextResourceSet.class);
            resourceSet.addLoadOption(XtextResource.OPTION_RESOLVE_ALL, Boolean.FALSE);
        return resourceSet;
        this.resourceSet = null;
        ScriptStandaloneSetup.unregister();
     * we need to make sure that the scriptRuntime service has been started before us
    protected void setScriptRuntime(final ScriptRuntime scriptRuntime) {
    protected void unsetScriptRuntime(final ScriptRuntime scriptRuntime) {
    protected void setScriptServiceUtil(ScriptServiceUtil scriptServiceUtil) {
        scriptServiceUtil.setScriptEngine(this);
    protected void unsetScriptServiceUtil(ScriptServiceUtil scriptServiceUtil) {
        scriptServiceUtil.unsetScriptEngine(this);
        this.scriptServiceUtil = null;
    public Script newScriptFromString(String scriptAsString) throws ScriptParsingException {
        return newScriptFromXExpression(parseScriptIntoXTextEObject(scriptAsString));
    public Script newScriptFromXExpression(XExpression expression) {
        ScriptImpl script = ScriptStandaloneSetup.getInjector().getInstance(ScriptImpl.class);
        script.setXExpression(expression);
    private XExpression parseScriptIntoXTextEObject(String scriptAsString) throws ScriptParsingException {
        XtextResourceSet resourceSet = getResourceSet();
        Resource resource = resourceSet.createResource(computeUnusedUri(resourceSet)); // IS-A XtextResource
            resource.load(new StringInputStream(scriptAsString, StandardCharsets.UTF_8.name()),
                    resourceSet.getLoadOptions());
            throw new ScriptParsingException(
                    "Unexpected IOException; from close() of a String-based ByteArrayInputStream, no real I/O; how is that possible???",
                    scriptAsString, e);
        List<Diagnostic> errors = resource.getErrors();
            deleteResource(resource);
            throw new ScriptParsingException("Failed to parse expression (due to managed SyntaxError/s)",
                    scriptAsString).addDiagnosticErrors(errors);
        EList<EObject> contents = resource.getContents();
        if (!contents.isEmpty()) {
            Iterable<Issue> validationErrors = getValidationErrors(contents.getFirst());
            if (!validationErrors.iterator().hasNext()) {
                return (XExpression) contents.getFirst();
                throw new ScriptParsingException("Failed to parse expression (due to managed ValidationError/s)",
                        scriptAsString).addValidationIssues(validationErrors);
    protected URI computeUnusedUri(ResourceSet resourceSet) {
        String name = "__synthetic";
        final int MAX_TRIES = 1000;
        for (int i = 0; i < MAX_TRIES; i++) {
            // NOTE: The "filename extension" (".script") must match the file.extensions in the *.mwe2
            URI syntheticUri = URI
                    .createURI(name + ThreadLocalRandom.current().nextDouble() + "." + Script.SCRIPT_FILEEXT);
            if (resourceSet.getResource(syntheticUri, false) == null) {
                return syntheticUri;
    protected List<Issue> validate(EObject model) {
        IResourceValidator validator = ((XtextResource) model.eResource()).getResourceServiceProvider()
                .getResourceValidator();
        return validator.validate(model.eResource(), CheckMode.ALL, CancelIndicator.NullImpl);
    protected Iterable<Issue> getValidationErrors(final EObject model) {
        final List<Issue> validate = validate(model);
        return validate.stream().filter(input -> Severity.ERROR == input.getSeverity()).toList();
        return "script";
    private void deleteResource(Resource resource) {
            resource.delete(Map.of());
