 * Expression that successfully parses, if a sequence of given expressions is matching. This class is immutable.
public final class ExpressionSequence extends Expression {
     * @param subExpressions the sub expressions that are parsed in the given order
    public ExpressionSequence(Expression... subExpressions) {
        int l = subExpressions.size();
        ASTNode[] children = new ASTNode[l];
        Object[] values = new Object[l];
            cr = children[i] = subExpressions.get(i).parse(language, list);
            if (!cr.isSuccess()) {
            values[i] = cr.getValue();
        node.setChildren(children);
        node.setValue(values);
        boolean blocking = false;
            blocking = e.collectFirsts(language, firsts);
            if (blocking) {
        return "seq(" + s + ")";
