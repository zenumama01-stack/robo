 * Expression that successfully parses, if one of the given alternative expressions matches. This class is immutable.
final class ExpressionAlternatives extends Expression {
    private List<Expression> subExpressions;
     * Constructs a new instance.
     * @param subExpressions the sub expressions that are tried/parsed as alternatives in the given order
    public ExpressionAlternatives(Expression... subExpressions) {
        this.subExpressions = List.of(Arrays.copyOf(subExpressions, subExpressions.length));
    ASTNode parse(ResourceBundle language, TokenList list) {
        ASTNode node = new ASTNode(), cr;
        for (Expression subExpression : subExpressions) {
            cr = subExpression.parse(language, list);
            if (cr.isSuccess()) {
                node.setChildren(new ASTNode[] { cr });
                node.setRemainingTokens(cr.getRemainingTokens());
                node.setSuccess(true);
                node.setValue(cr.getValue());
                generateValue(node);
        return subExpressions;
    boolean collectFirsts(ResourceBundle language, Set<String> firsts) {
        boolean blocking = true;
        for (Expression e : subExpressions) {
            blocking = blocking && e.collectFirsts(language, firsts);
        return blocking;
        String s = null;
            s = s == null ? e.toString() : (s + ", " + e.toString());
        return "alt(" + s + ")";
