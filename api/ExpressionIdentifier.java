 * Expression that successfully parses, if a thing identifier token is found. This class is immutable.
public final class ExpressionIdentifier extends Expression {
    private AbstractRuleBasedInterpreter interpreter;
    private Expression stopper;
     * @param interpreter the interpreter it belongs to. Used for dynamically fetching item name tokens
    public ExpressionIdentifier(AbstractRuleBasedInterpreter interpreter) {
        this(interpreter, null);
     * @param stopper Expression that should not match, if the current token should be accepted as identifier
    public ExpressionIdentifier(AbstractRuleBasedInterpreter interpreter, Expression stopper) {
        this.stopper = stopper;
        ASTNode node = new ASTNode();
        node.setSuccess(list.size() > 0 && (stopper == null || !stopper.parse(language, list).isSuccess()));
        if (node.isSuccess()) {
            node.setRemainingTokens(list.skipHead());
            node.setValue(list.head());
            node.setChildren(new ASTNode[0]);
        Set<String> f = new HashSet<>(interpreter.getAllItemTokens(language.getLocale()));
            f.removeAll(stopper.getFirsts(language));
        firsts.addAll(f);
        return "identifier(stop=" + stopper + ")";
     * @return the interpreter
    public AbstractRuleBasedInterpreter getInterpreter() {
        return interpreter;
     * @return the stopper expression
    public Expression getStopper() {
        return stopper;
