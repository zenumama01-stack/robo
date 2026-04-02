package org.openhab.core.library;
 * {@link CoreItemFactory}-Implementation for the core ItemTypes
 * @author Alexander Kostadinov - Initial contribution
@Component(immediate = true, service = { CoreItemFactory.class, ItemFactory.class })
public class CoreItemFactory implements ItemFactory {
    public static final String CALL = "Call";
    public static final String COLOR = "Color";
    public static final String CONTACT = "Contact";
    public static final String DATETIME = "DateTime";
    public static final String DIMMER = "Dimmer";
    public static final String LOCATION = "Location";
    public static final String NUMBER = "Number";
    public static final String PLAYER = "Player";
    public static final String ROLLERSHUTTER = "Rollershutter";
    public static final String STRING = "String";
    public static final Set<String> VALID_ITEM_TYPES = Set.of( //
            CALL, COLOR, CONTACT, DATETIME, DIMMER, IMAGE, LOCATION, NUMBER, PLAYER, ROLLERSHUTTER, STRING, SWITCH //
    public CoreItemFactory(final @Reference UnitProvider unitProvider) {
    public @Nullable GenericItem createItem(@Nullable String itemTypeName, String itemName) {
        String itemType = ItemUtil.getMainItemType(itemTypeName);
        return switch (itemType) {
            case CALL -> new CallItem(itemName);
            case COLOR -> new ColorItem(itemName);
            case CONTACT -> new ContactItem(itemName);
            case DATETIME -> new DateTimeItem(itemName);
            case DIMMER -> new DimmerItem(itemName);
            case IMAGE -> new ImageItem(itemName);
            case LOCATION -> new LocationItem(itemName);
            case NUMBER -> new NumberItem(itemTypeName, itemName, unitProvider);
            case PLAYER -> new PlayerItem(itemName);
            case ROLLERSHUTTER -> new RollershutterItem(itemName);
            case STRING -> new StringItem(itemName);
            case SWITCH -> new SwitchItem(itemName);
    public String[] getSupportedItemTypes() {
        return VALID_ITEM_TYPES.toArray(new String[0]);
