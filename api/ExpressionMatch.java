 * Expression that successfully parses, if a given string constant is found. This class is immutable.
public final class ExpressionMatch extends Expression {
     * @param pattern the token that has to match for successful parsing
    public ExpressionMatch(String pattern) {
        node.setSuccess(list.checkHead(pattern));
            node.setValue(pattern);
        firsts.add(pattern);
        return "match(\"" + pattern + "\")";
     * @return the pattern
    public String getPattern() {
