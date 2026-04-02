import org.eclipse.xtext.xbase.typesystem.computation.ITypeComputationState;
import org.eclipse.xtext.xbase.typesystem.computation.ITypeExpectation;
import org.eclipse.xtext.xbase.typesystem.computation.XbaseTypeComputer;
import org.eclipse.xtext.xbase.typesystem.references.LightweightTypeReference;
import org.openhab.core.model.script.script.QuantityLiteral;
 * Calculates the type information used by Xbase to select the correct method during script execution.
public class ScriptTypeComputer extends XbaseTypeComputer {
    public void computeTypes(XExpression expression, ITypeComputationState state) {
        if (expression instanceof QuantityLiteral literal) {
            _computeTypes(literal, state);
            super.computeTypes(expression, state);
    protected void _computeTypes(final QuantityLiteral assignment, ITypeComputationState state) {
        LightweightTypeReference qt = null;
        for (ITypeExpectation exp : state.getExpectations()) {
            if (exp.getExpectedType() == null) {
            if (exp.getExpectedType().isType(Number.class)) {
                qt = getRawTypeForName(Number.class, state);
            if (exp.getExpectedType().isType(State.class)) {
            if (exp.getExpectedType().isType(Command.class)) {
        if (qt == null) {
            qt = getRawTypeForName(QuantityType.class, state);
        state.acceptActualType(qt);
