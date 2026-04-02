import org.openhab.core.model.script.actions.Semantics;
 * This class registers an OSGi service for the Semantics action.
public class SemanticsActionService implements ActionService {
    private static @Nullable ItemRegistry itemRegistry;
    public SemanticsActionService(final @Reference ItemRegistry itemRegistry) {
        SemanticsActionService.itemRegistry = itemRegistry;
        return Semantics.class;
        return SemanticsPredicates.isLocation().test(item);
        return SemanticsPredicates.isEquipment().test(item);
        return SemanticsPredicates.isPoint().test(item);
    public static @Nullable Item getLocationItemFromGroupNames(List<String> groupNames) {
        ItemRegistry ir = itemRegistry;
        if (ir != null) {
            List<Item> groupItems = new ArrayList<>();
            for (String groupName : groupNames) {
                    Item group = ir.getItem(groupName);
                    // if group is a location, return it (first location found)
                    if (isLocation(group)) {
                    groupItems.add(group);
                    // should not happen
            // if no location is found, iterate the groups of each group
            for (Item group : groupItems) {
                Item locationItem = getLocationItemFromGroupNames(group.getGroupNames());
                if (locationItem != null) {
                    return locationItem;
    public static @Nullable Item getEquipmentItemFromGroupNames(List<String> groupNames) {
                    // if group is an equipment, return it (first equipment found)
                    if (isEquipment(group)) {
            // if no equipment is found, iterate the groups of each group
                Item equipmentItem = getEquipmentItemFromGroupNames(group.getGroupNames());
                if (equipmentItem != null) {
                    return equipmentItem;
