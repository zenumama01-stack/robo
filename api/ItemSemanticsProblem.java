 * This is a representation of an item semantics configuration problem.
 * @param item item with problem
 * @param semanticType item semantic type
 * @param reason description for the item semantics configuration problem
 * @param explanation longer explanation of problem
 * @param editable true if the item with the problem is a managed item, null if editable status is not checked
public record ItemSemanticsProblem(String item, @Nullable String semanticType, String reason,
        @Nullable String explanation, @Nullable Boolean editable) {
    public ItemSemanticsProblem setEditable(boolean editable) {
        return new ItemSemanticsProblem(this.item, this.semanticType, this.reason, this.explanation, editable);
