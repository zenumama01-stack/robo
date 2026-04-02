package org.openhab.core.model.yaml.internal.util;
 * Static utility methods that are helpful when dealing with YAML elements.
public class YamlElementUtils {
    public static @Nullable String getAdjustedItemType(@Nullable String type) {
        return type == null ? null : StringUtils.capitalize(type);
    public static boolean isValidItemType(@Nullable String type) {
        String adjustedType = getAdjustedItemType(type);
        return adjustedType == null ? true : CoreItemFactory.VALID_ITEM_TYPES.contains(adjustedType);
    public static boolean isNumberItemType(@Nullable String type) {
        return CoreItemFactory.NUMBER.equals(getAdjustedItemType(type));
    public static @Nullable String getAdjustedItemDimension(@Nullable String dimension) {
        return dimension == null ? null : StringUtils.capitalize(dimension);
    public static boolean isValidItemDimension(@Nullable String dimension) {
        String adjustedDimension = getAdjustedItemDimension(dimension);
        if (adjustedDimension != null) {
                UnitUtils.parseDimension(adjustedDimension);
    public static @Nullable String getItemTypeWithDimension(@Nullable String type, @Nullable String dimension) {
        return adjustedType != null ? adjustedType + (adjustedDimension == null ? "" : ":" + adjustedDimension) : null;
