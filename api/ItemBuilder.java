 * This class allows the easy construction of an {@link Item} using the builder pattern.
public interface ItemBuilder {
     * Creates an item with the currently configured values.
     * @return an item
     * @throws IllegalStateException in case no item factory can create the given item type
    Item build();
     * Set the label of the item.
    ItemBuilder withLabel(@Nullable String label);
     * Set the group membership of the item.
     * @param groups the group names the item belongs to
    ItemBuilder withGroups(@Nullable Collection<String> groups);
     * Set the category of the item.
     * @param category the category
    ItemBuilder withCategory(@Nullable String category);
     * Set the base item..
     * @param baseItem the base item
     * @throws IllegalArgumentException in case this is not a group item
    ItemBuilder withBaseItem(@Nullable Item baseItem);
     * Set the group function
     * @param function the group function
    ItemBuilder withGroupFunction(@Nullable GroupFunction function);
     * Set the tags
    ItemBuilder withTags(@Nullable Set<String> tags);
