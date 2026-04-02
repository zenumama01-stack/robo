 * Base class for all expressions.
public abstract class Expression {
    Expression() {
    abstract ASTNode parse(ResourceBundle language, TokenList list);
    void generateValue(ASTNode node) {
    List<Expression> getChildExpressions() {
    abstract boolean collectFirsts(ResourceBundle language, Set<String> firsts);
    Set<String> getFirsts(ResourceBundle language) {
        Set<String> firsts = new HashSet<>();
        collectFirsts(language, firsts);
        return firsts;
