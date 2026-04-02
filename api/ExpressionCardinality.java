 * Expression that successfully parses, if a given expression occurs or repeats with a specified cardinality. This class
 * is immutable.
public final class ExpressionCardinality extends Expression {
    private Expression subExpression;
    private boolean atLeastOne = false;
    private boolean atMostOne = true;
     * @param subExpression expression that could occur or repeat
     * @param atLeastOne true, if expression should occur at least one time
     * @param atMostOne true, if expression should occur at most one time
    public ExpressionCardinality(Expression subExpression, boolean atLeastOne, boolean atMostOne) {
        this.subExpression = subExpression;
        this.atLeastOne = atLeastOne;
        this.atMostOne = atMostOne;
    ASTNode parse(ResourceBundle language, TokenList tokenList) {
        TokenList list = tokenList;
        List<ASTNode> nodes = new ArrayList<>();
        List<Object> values = new ArrayList<>();
        while ((cr = subExpression.parse(language, list)).isSuccess()) {
            nodes.add(cr);
            values.add(cr.getValue());
            list = cr.getRemainingTokens();
            if (atMostOne) {
        if (!(atLeastOne && nodes.isEmpty())) {
            node.setChildren(nodes.toArray(new ASTNode[0]));
            node.setRemainingTokens(list);
            node.setValue(atMostOne ? (values.isEmpty() ? null : values.getFirst()) : values.toArray());
        return List.of(subExpression);
        return subExpression.collectFirsts(language, firsts) || atLeastOne;
        return "cardinal(" + atLeastOne + ", " + atMostOne + "' " + subExpression.toString() + ")";
     * @return the subExpression
    public Expression getSubExpression() {
        return subExpression;
     * @return the atLeastOne
    public boolean isAtLeastOne() {
        return atLeastOne;
     * @return the atMostOne
    public boolean isAtMostOne() {
        return atMostOne;
