 * The {@link ItemBuilder} implementation.
public class ItemBuilderImpl implements ItemBuilder {
    private final Set<ItemFactory> itemFactories;
    private List<String> groups = List.of();
    private @Nullable Item baseItem;
    private @Nullable GroupFunction groupFunction;
    private Set<String> tags = Set.of();
    public ItemBuilderImpl(Set<ItemFactory> itemFactories, Item item) {
        this.itemFactories = itemFactories;
        this.itemType = item.getType();
        this.name = item.getName();
        this.category = item.getCategory();
        this.groups = item.getGroupNames();
        this.label = item.getLabel();
        this.tags = item.getTags();
            this.baseItem = groupItem.getBaseItem();
            this.groupFunction = groupItem.getFunction();
    public ItemBuilderImpl(Set<ItemFactory> itemFactories, String itemType, String itemName) {
        this.name = itemName;
    public ItemBuilder withLabel(@Nullable String label) {
    public ItemBuilder withGroups(@Nullable Collection<String> groups) {
            this.groups = new LinkedList<>(groups);
            this.groups = List.of();
    public ItemBuilder withCategory(@Nullable String category) {
    public ItemBuilder withBaseItem(@Nullable Item item) {
        if (item != null && !GroupItem.TYPE.equals(itemType)) {
            throw new IllegalArgumentException("Only group items can have a base item");
        baseItem = item;
    public ItemBuilder withGroupFunction(@Nullable GroupFunction function) {
        if (function != null && !GroupItem.TYPE.equals(itemType)) {
        groupFunction = function;
    public ItemBuilder withTags(@Nullable Set<String> tags) {
    public Item build() {
        if (GroupItem.TYPE.equals(itemType)) {
            item = new GroupItem(name, baseItem, groupFunction);
                item = itemFactory.createItem(itemType, name);
            throw new IllegalStateException("No item factory could handle type " + itemType);
            activeItem.setCategory(category);
            activeItem.addGroupNames(groups);
            activeItem.addTags(tags);
