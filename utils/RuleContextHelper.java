import org.eclipse.emf.common.notify.Adapter;
import org.eclipse.emf.ecore.util.EContentAdapter;
import org.eclipse.xtext.naming.QualifiedName;
import org.openhab.core.model.rule.RulesStandaloneSetup;
import org.openhab.core.model.rule.rules.VariableDeclaration;
import org.openhab.core.model.script.engine.ScriptExecutionException;
 * Helper class to deal with rule evaluation contexts.
public class RuleContextHelper {
     * Retrieves the evaluation context (= set of variables) for a ruleModel. The context is shared with all rules in
     * same model (= rule file).
     * @param ruleModel the ruleModel to get the context for
     * @return the evaluation context
    public static synchronized IEvaluationContext getContext(RuleModel ruleModel) {
        Logger logger = LoggerFactory.getLogger(RuleContextHelper.class);
        Injector injector = RulesStandaloneSetup.getInjector();
        // check if a context already exists on the resource
        for (Adapter adapter : ruleModel.eAdapters()) {
            if (adapter instanceof RuleContextAdapter contextAdapter) {
                return contextAdapter.getContext();
        Provider<@NonNull IEvaluationContext> contextProvider = injector.getProvider(IEvaluationContext.class);
        // no evaluation context found, so create a new one
        ScriptEngine scriptEngine = injector.getInstance(ScriptEngine.class);
        IEvaluationContext evaluationContext = contextProvider.get();
        for (VariableDeclaration var : ruleModel.getVariables()) {
                Object initialValue = var.getRight() == null ? null
                        : scriptEngine.newScriptFromXExpression(var.getRight()).execute(evaluationContext);
                evaluationContext.newValue(QualifiedName.create(var.getName()), initialValue);
            } catch (ScriptExecutionException e) {
                logger.warn("Variable '{}' on rule file '{}' cannot be initialized with value '{}': {}", var.getName(),
                        ruleModel.eResource().getURI().path(), var.getRight(), e.getMessage());
        ruleModel.eAdapters().add(new RuleContextAdapter(evaluationContext));
        return evaluationContext;
     * Inner class that wraps an evaluation context into an EMF adapters
    private static class RuleContextAdapter extends EContentAdapter {
        private final IEvaluationContext context;
        public RuleContextAdapter(IEvaluationContext context) {
        public IEvaluationContext getContext() {
