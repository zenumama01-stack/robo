 * Expression that decorates the resulting (proxied) AST node of a given expression by a name, value and tag.
 * This class is immutable.
public final class ExpressionLet extends Expression {
     * @param name the name that should be set on the node. Null, if the node's name should not be changed.
     * @param subExpression the expression who's resulting node should be altered
    public ExpressionLet(String name, Expression subExpression) {
        this(name, subExpression, null, null);
     * @param value the value that should be set on the node. Null, if the node's value should not be changed.
    public ExpressionLet(Expression subExpression, Object value) {
        this(null, subExpression, value, null);
     * @param tag the tag that should be set on the node. Null, if the node's tag should not be changed.
    public ExpressionLet(String name, Expression subExpression, Object value, Object tag) {
        ASTNode node = subExpression.parse(language, list);
            node.setName(name);
                node.setValue(value);
                node.setTag(tag);
        return subExpression.collectFirsts(language, firsts);
        return "let(\"" + name + "\", " + subExpression.toString() + ", \"" + value + "\", \"" + tag + "\")";
