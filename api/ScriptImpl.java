import org.eclipse.xtext.xbase.interpreter.IEvaluationResult;
import org.eclipse.xtext.xbase.interpreter.IExpressionInterpreter;
 * This is the default implementation of a {@link Script}.
public class ScriptImpl implements Script {
    private XExpression xExpression;
    public ScriptImpl() {
    /* package-local */
    void setXExpression(XExpression xExpression) {
        this.xExpression = xExpression;
    XExpression getXExpression() {
        return xExpression;
    public Object execute() throws ScriptExecutionException {
            IEvaluationContext evaluationContext = null;
                evaluationContext = provider.get(IEvaluationContext.class);
            return execute(evaluationContext);
            throw new ScriptExecutionException("Script does not contain any expression");
    public Object execute(final IEvaluationContext evaluationContext) throws ScriptExecutionException {
            IExpressionInterpreter interpreter = null;
                interpreter = provider.get(IExpressionInterpreter.class);
            if (interpreter == null) {
                throw new ScriptExecutionException("Script interpreter couldn't be obtained");
                IEvaluationResult result = interpreter.evaluate(xExpression, evaluationContext,
                        CancelIndicator.NullImpl);
                    // this can only happen on an InterpreterCancelledException,
                    // i.e. NEVER ;-)
                if (result.getException() != null) {
                    throw new ScriptExecutionException(result.getException().getMessage(), result.getException());
                return result.getResult();
            } catch (Throwable e) {
                if (e instanceof ScriptExecutionException exception) {
                    throw new ScriptExecutionException(
                            "An error occurred during the script execution: " + e.getMessage(), e);
