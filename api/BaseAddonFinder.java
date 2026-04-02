 * This is a {@link BaseAddonFinder} abstract class for finding suggested add-ons.
public abstract class BaseAddonFinder implements AddonFinder {
     * Helper method to check if the given {@code propertyName} is in the {@code propertyPatternMap} and if so, the
     * given {@code propertyValue} matches the respective regular expression {@code Pattern}.
     * @param propertyPatternMap map of property names and regex patterns for value matching
     * @param propertyName
     * @param propertyValue
     * @return true a) if the property name exists and the property value is not null and matches the regular
     *         expression, or b) the property name does not exist.
    protected static boolean propertyMatches(Map<String, Pattern> propertyPatternMap, String propertyName,
            @Nullable String propertyValue) {
        Pattern pattern = propertyPatternMap.get(propertyName);
        return pattern == null || (propertyValue != null && pattern.matcher(propertyValue).matches());
    protected volatile List<AddonInfo> addonCandidates = List.of();
        addonCandidates = candidates;
        addonCandidates = List.of();
    public abstract String getServiceName();
