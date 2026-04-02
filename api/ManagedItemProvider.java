import org.openhab.core.items.ManagedItemProvider.PersistedItem;
 * {@link ManagedItemProvider} is an OSGi service, that allows to add or remove
 * items at runtime by calling {@link #add} or {@link #remove}.
 * An added item is automatically
 * exposed to the {@link ItemRegistry}. Persistence of added Items is handled by
 * a {@link StorageService}. Items are being restored using the given {@link ItemFactory}s.
 * @author Dennis Nobel - Initial contribution, added support for GroupItems
 * @author Alex Tugarev - added tags
@Component(immediate = true, service = { ItemProvider.class, ManagedItemProvider.class })
public class ManagedItemProvider extends AbstractManagedProvider<Item, String, PersistedItem> implements ItemProvider {
    public static class PersistedItem {
        public PersistedItem(String itemType) {
        public @Nullable String baseItemType;
        public @Nullable List<String> groupNames;
        public @Nullable Set<String> tags;
        public @Nullable String functionName;
        public @Nullable List<String> functionParams;
        public @Nullable String dimension;
    private final Logger logger = LoggerFactory.getLogger(ManagedItemProvider.class);
    private final Map<String, PersistedItem> failedToCreate = new ConcurrentHashMap<>();
    public ManagedItemProvider(final @Reference StorageService storageService,
            final @Reference ItemBuilderFactory itemBuilderFactory) {
        Item item = get(key);
            removeGroupNameFromMembers(groupItem);
     * Removes an item and its member if recursive flag is set to true.
     * @param itemName item name to remove
     * @param recursive if set to true all members of the item will be removed, too.
     * @return removed item or null if no item with that name exists
    public void add(Item element) {
        if (!ItemUtil.isValidItemName(element.getName())) {
            throw new IllegalArgumentException("The item name '" + element.getName() + "' is invalid.");
    private Set<Item> getMembers(GroupItem groupItem, Collection<Item> allItems) {
        Set<Item> members = new HashSet<>();
                members.add(item);
    private @Nullable Item createItem(String itemType, String itemName) {
            return itemBuilderFactory.newItemBuilder(itemType, itemName).build();
            logger.debug("Couldn't create item '{}' of type '{}'", itemName, itemType);
    private void removeGroupNameFromMembers(GroupItem groupItem) {
        Set<Item> members = getMembers(groupItem, getAll());
            if (member instanceof GenericItem item) {
                item.removeGroupName(groupItem.getUID());
                update(member);
     * Translates the Items class simple name into a type name understandable by
     * the {@link ItemFactory}s.
     * @param item the Item to translate the name
     * @return the translated ItemTypeName understandable by the {@link ItemFactory}s
    private String toItemFactoryName(Item item) {
        return item.getType();
        if (!failedToCreate.isEmpty()) {
            // retry failed creation attempts
            Iterator<Entry<String, PersistedItem>> iterator = failedToCreate.entrySet().iterator();
                Entry<String, PersistedItem> entry = iterator.next();
                String itemName = entry.getKey();
                PersistedItem persistedItem = entry.getValue();
                Item item = itemFactory.createItem(persistedItem.itemType, itemName);
                    configureItem(persistedItem, genericItem);
                    logger.debug("The added item factory '{}' still could not instantiate item '{}'.", itemFactory,
                            itemName);
            if (failedToCreate.isEmpty()) {
                logger.info("Finished loading the items which could not have been created before.");
        return Item.class.getName();
    protected @Nullable Item toElement(String itemName, PersistedItem persistedItem) {
        if (GroupItem.TYPE.equals(persistedItem.itemType)) {
            String baseItemType = persistedItem.baseItemType;
            if (baseItemType != null) {
                Item baseItem = createItem(baseItemType, itemName);
                if (persistedItem.functionName != null) {
                    GroupFunction function = getGroupFunction(persistedItem, baseItem);
                    item = new GroupItem(itemName, baseItem, function);
                    item = new GroupItem(itemName, baseItem, new GroupFunction.Equality());
                item = new GroupItem(itemName);
            item = createItem(persistedItem.itemType, itemName);
            failedToCreate.put(itemName, persistedItem);
            logger.debug("Couldn't restore item '{}' of type '{}' ~ there is no appropriate ItemFactory available.",
                    itemName, persistedItem.itemType);
    private GroupFunction getGroupFunction(PersistedItem persistedItem, @Nullable Item baseItem) {
        GroupFunctionDTO functionDTO = new GroupFunctionDTO();
        functionDTO.name = persistedItem.functionName;
        if (persistedItem.functionParams instanceof List<?> list) {
            functionDTO.params = list.toArray(new String[list.size()]);
        return ItemDTOMapper.mapFunction(baseItem, functionDTO);
    private void configureItem(PersistedItem persistedItem, GenericItem item) {
        List<String> groupNames = persistedItem.groupNames;
        if (groupNames != null) {
                item.addGroupName(groupName);
        Set<String> tags = persistedItem.tags;
        item.setLabel(persistedItem.label);
        item.setCategory(persistedItem.category);
    protected PersistedItem toPersistableElement(Item item) {
        PersistedItem persistedItem = new PersistedItem(
                item instanceof GroupItem ? GroupItem.TYPE : toItemFactoryName(item));
            String baseItemType = null;
                baseItemType = toItemFactoryName(baseItem);
            persistedItem.baseItemType = baseItemType;
            addFunctionToPersisedItem(persistedItem, groupItem);
        persistedItem.label = item.getLabel();
        persistedItem.groupNames = new ArrayList<>(item.getGroupNames());
        persistedItem.tags = new HashSet<>(item.getTags());
        persistedItem.category = item.getCategory();
    private void addFunctionToPersisedItem(PersistedItem persistedItem, GroupItem groupItem) {
        if (groupItem.getFunction() != null) {
            GroupFunctionDTO functionDTO = ItemDTOMapper.mapFunction(groupItem.getFunction());
            if (functionDTO != null) {
                persistedItem.functionName = functionDTO.name;
                if (functionDTO.params != null) {
                    persistedItem.functionParams = Arrays.asList(functionDTO.params);
