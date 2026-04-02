package org.openhab.core.voice.text;
 * Abstract syntax tree node. Result of parsing an expression.
public class ASTNode {
    private boolean success = false;
    private ASTNode[] children;
    private TokenList remainingTokens;
    private Object value;
    private Object tag;
    public ASTNode() {
     * Constructs a new AST node.
     * @param children the node's children
     * @param remainingTokens remaining token list starting with the first token that was not covered/consumed
    public ASTNode(ASTNode[] children, TokenList remainingTokens) {
        this.success = true;
        this.children = children;
        this.remainingTokens = remainingTokens;
     * Breadth searching this (sub-) tree/node for a node with the given name.
     * @param name the name that's used for looking up the tree
     * @return first node with the given name or null, if none was found
    public ASTNode findNode(String name) {
        if (this.name != null && this.name.equals(name)) {
        ASTNode n;
        for (ASTNode sn : children) {
            n = sn.findNode(name);
            if (n != null) {
     * @return the value of this node as {@code String[]}
    public String[] getValueAsStringArray() {
        Object[] objs = value instanceof Object[] os ? os : new Object[] { value };
        String[] result = new String[objs.length];
        for (int i = 0; i < objs.length; i++) {
            result[i] = objs[i] == null ? "" : ("" + objs[i]);
     * @return the value of this node as {@link String}.
    public String getValueAsString() {
        return value == null ? "" : ("" + value);
     * Breadth searches this (sub-) tree/node for a node with the given name and returning its value as a
     * {@code String[]}.
     * @param name the name of the named node to be found
     * @return the value of the resulting node as {@code String[]} or null if not found
    public String[] findValueAsStringArray(String name) {
        ASTNode node = findNode(name);
        return node == null ? null : node.getValueAsStringArray();
     * Breadth searches this (sub-) tree/node for a node with the given name and returning its value as a {@link String}
     * .
     * @return the value of the resulting node as {@link String} or null if not found
    public String findValueAsString(String name) {
        return node == null ? null : node.getValueAsString();
     * Breadth searches this (sub-) tree/node for a node with the given name and type and returning its value.
     * @param cls the node's value has to be assignable to a reference of this class to match during search
     * @return the value of the resulting node. Null, if not found or the value does not match {@code cls}.
    public Object findValue(String name, Class<?> cls) {
        return node == null ? null
                : ((node.value != null && cls.isAssignableFrom(node.value.getClass())) ? node.value : null);
     * Breadth searches this (sub-) tree/node for a node with the given name and returning its value.
     * @return the value of the resulting node. Null, if not found.
    public Object findValue(String name) {
        return node == null ? null : node.value;
     * @return if the node is a valid one (true) or parsing was not successful (false)
    public boolean isSuccess() {
     * @param success if the node is a valid one (true) or parsing was not successful (false)
    public void setSuccess(boolean success) {
     * @return the children
    public ASTNode[] getChildren() {
     * @param children the children to set
    public void setChildren(ASTNode[] children) {
     * @return the remainingTokens
    public TokenList getRemainingTokens() {
        return remainingTokens;
     * @param remainingTokens the remainingTokens to set
    public void setRemainingTokens(TokenList remainingTokens) {
     * @return the name
     * @param name the name to set
     * @param value the value to set
    public void setValue(Object value) {
     * @return the tag
    public Object getTag() {
        return tag;
     * @param tag the tag to set
    public void setTag(Object tag) {
