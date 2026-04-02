import org.openhab.core.transform.TransformationHelper;
import org.openhab.core.types.StateDescriptionFragmentBuilder;
 * The {@link EnrichedItemDTOMapper} is a utility class to map items into enriched item data transform objects (DTOs).
 * @author Jochen Hiller - Fix #473630 - handle optional dependency to TransformationHelper
public class EnrichedItemDTOMapper {
    private static final Pattern EXTRACT_TRANSFORM_FUNCTION_PATTERN = Pattern.compile("(.*?)\\((.*)\\):(.*)");
    private static final Logger LOGGER = LoggerFactory.getLogger(EnrichedItemDTOMapper.class);
     * Maps item into enriched item DTO object.
     * @param drillDown defines whether the whole tree should be traversed or only direct members are considered
     * @param itemFilter a predicate that filters items while traversing the tree (true means that an item is
     *            considered, can be null)
     * @param uriBuilder if present the URI builder contains one template that will be replaced by the specific item
     *            name
     * @param locale locale (can be null)
     * @param zoneId time-zone id (can be null)
     * @return item DTO object
    public static EnrichedItemDTO map(Item item, boolean drillDown, @Nullable Predicate<Item> itemFilter,
            @Nullable UriBuilder uriBuilder, @Nullable Locale locale, @Nullable ZoneId zoneId) {
        ItemDTO itemDTO = ItemDTOMapper.map(item);
        return map(item, itemDTO, drillDown, itemFilter, uriBuilder, locale, zoneId, new ArrayList<>());
    private static EnrichedItemDTO mapRecursive(Item item, @Nullable Predicate<Item> itemFilter,
            @Nullable UriBuilder uriBuilder, @Nullable Locale locale, @Nullable ZoneId zoneId, List<Item> parents) {
        return map(item, itemDTO, true, itemFilter, uriBuilder, locale, zoneId, parents);
    private static EnrichedItemDTO map(Item item, ItemDTO itemDTO, boolean drillDown,
            @Nullable Predicate<Item> itemFilter, @Nullable UriBuilder uriBuilder, @Nullable Locale locale,
            @Nullable ZoneId zoneId, List<Item> parents) {
        if (item instanceof GroupItem) {
            // only add as parent item if it is a group, otherwise duplicate memberships trigger false warnings
            parents.add(item);
        if (item instanceof DateTimeItem dateTimeItem && zoneId != null) {
            DateTimeType dateTime = dateTimeItem.getStateAs(DateTimeType.class);
            if (dateTime == null) {
                state = item.getState().toFullString();
                state = dateTime.toFullString(zoneId);
        String transformedState = considerTransformation(item, locale);
        if (state.equals(transformedState)) {
            transformedState = null;
        StateDescription stateDescription = considerTransformation(item.getStateDescription(locale));
        String lastState = Optional.ofNullable(item.getLastState()).map(State::toFullString).orElse(null);
        Long lastStateUpdate = Optional.ofNullable(item.getLastStateUpdate()).map(zdt -> zdt.toInstant().toEpochMilli())
        Long lastStateChange = Optional.ofNullable(item.getLastStateChange()).map(zdt -> zdt.toInstant().toEpochMilli())
        final String link;
        if (uriBuilder != null) {
            link = uriBuilder.build(itemDTO.name).toASCIIString();
            link = null;
        EnrichedItemDTO enrichedItemDTO;
        String unitSymbol = null;
        if (item instanceof NumberItem numberItem) {
            unitSymbol = numberItem.getUnitSymbol();
            if (groupItem.getBaseItem() instanceof NumberItem baseNumberItem) {
                unitSymbol = baseNumberItem.getUnitSymbol();
            EnrichedItemDTO[] memberDTOs;
            if (drillDown) {
                Collection<EnrichedItemDTO> members = new LinkedHashSet<>();
                for (Item member : groupItem.getMembers()) {
                    if (parents.contains(member)) {
                        LOGGER.error(
                                "Recursive group membership found: {} is a member of {}, but it is also one of its ancestors.",
                                member.getName(), groupItem.getName());
                    } else if (itemFilter == null || itemFilter.test(member)) {
                        members.add(
                                mapRecursive(member, itemFilter, uriBuilder, locale, zoneId, new ArrayList<>(parents)));
                memberDTOs = members.toArray(new EnrichedItemDTO[0]);
                memberDTOs = new EnrichedItemDTO[0];
            enrichedItemDTO = new EnrichedGroupItemDTO(itemDTO, memberDTOs, link, state, lastState, lastStateUpdate,
                    lastStateChange, transformedState, stateDescription, unitSymbol);
            enrichedItemDTO = new EnrichedItemDTO(itemDTO, link, state, lastState, lastStateUpdate, lastStateChange,
                    transformedState, stateDescription, item.getCommandDescription(locale), unitSymbol);
        return enrichedItemDTO;
    private static @Nullable StateDescription considerTransformation(@Nullable StateDescription stateDescription) {
        if (stateDescription != null) {
            String pattern = stateDescription.getPattern();
                return TransformationHelper.isTransform(pattern)
                        ? StateDescriptionFragmentBuilder.create(stateDescription).withPattern(pattern).build()
                                .toStateDescription()
                        : stateDescription;
        return stateDescription;
    private static @Nullable String considerTransformation(Item item, @Nullable Locale locale) {
        StateDescription stateDescription = item.getStateDescription(locale);
            Matcher matcher;
            if (pattern != null && (matcher = EXTRACT_TRANSFORM_FUNCTION_PATTERN.matcher(pattern)).find()) {
                    String type = matcher.group(1);
                    String function = matcher.group(2);
                    String value = matcher.group(3);
                    TransformationService transformation = TransformationHelper.getTransformationService(type);
                        String format = state instanceof UnDefType ? "%s" : value;
                            return transformation.transform(function, state.format(format));
                                    "Cannot format state '" + state + "' to format '" + format + "'", e);
                            throw new TransformationException("Transformation service of type '" + type
                                    + "' threw an exception: " + e.getMessage(), e);
                                "Transformation service of type '" + type + "' is not available.");
                    LOGGER.warn("Failed transforming the state '{}' on item '{}' with pattern '{}': {}", state,
                            item.getName(), pattern, e.getMessage());
