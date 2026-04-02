 * ConditionHandler implementation to check item state
public class ItemStateConditionHandler extends BaseConditionModuleHandler implements EventSubscriber {
     * Constants for Config-Parameters corresponding to Definition in ItemConditions.json
    public static final String STATE = "state";
    private final Logger logger = LoggerFactory.getLogger(ItemStateConditionHandler.class);
    public static final String ITEM_STATE_CONDITION = "core.ItemStateCondition";
    public ItemStateConditionHandler(Condition condition, String ruleUID, BundleContext bundleContext,
            ItemRegistry itemRegistry, TimeZoneProvider timeZoneProvider) {
        this.itemName = (String) module.getConfiguration().get(ITEM_NAME);
        this.types = Set.of(ItemAddedEvent.TYPE, ItemRemovedEvent.TYPE);
        if (itemRegistry.get(itemName) == null) {
            logger.warn("Item '{}' needed for rule '{}' is missing. Condition '{}' will not work.", itemName, ruleUID,
                logger.info("Item '{}' needed for rule '{}' added. Condition '{}' will now work.", itemName, ruleUID,
        } else if ((event instanceof ItemRemovedEvent removedEvent) && itemName.equals(removedEvent.getItem().name)) {
            logger.warn("Item '{}' needed for rule '{}' removed. Condition '{}' will no longer work.", itemName,
        String state = (String) module.getConfiguration().get(STATE);
        String operator = (String) module.getConfiguration().get(OPERATOR);
        if (operator == null || state == null || itemName == null) {
            logger.error("Module is not well configured: itemName={}  operator={}  state = {} for rule {}", itemName,
                    operator, state, ruleUID);
            logger.debug("ItemStateCondition '{}' checking if {} {} {} for rule {}", module.getId(), itemName, operator,
                    state, ruleUID);
                    return equalsToItemState(itemName, state);
                case "!=":
                    return !equalsToItemState(itemName, state);
                    return !greaterThanOrEqualsToItemState(itemName, state);
                    return lessThanOrEqualsToItemState(itemName, state);
                    return !lessThanOrEqualsToItemState(itemName, state);
                    return greaterThanOrEqualsToItemState(itemName, state);
            logger.error("Item with name {} not found in ItemRegistry for condition of rule {}.", itemName, ruleUID);
    private boolean lessThanOrEqualsToItemState(String itemName, String state) throws ItemNotFoundException {
        State compareState = TypeParser.parseState(item.getAcceptedDataTypes(), state);
        State itemState = item.getState();
        if (itemState instanceof DateTimeType dateTimeState) {
            Instant itemTime = dateTimeState.getInstant();
            Instant compareTime = getCompareTime(state);
            return itemTime.compareTo(compareTime) <= 0;
        } else if (itemState instanceof QuantityType qtState) {
            if (compareState instanceof DecimalType type) {
                // allow compareState without unit -> implicitly assume its the same as the one from the
                // state, but warn the user
                if (!Units.ONE.equals(qtState.getUnit())) {
                            "Received a QuantityType state '{}' with unit for item {}, but the condition is defined as a plain number without unit ({}), please consider adding a unit to the condition for rule {}.",
                            qtState, itemName, state, ruleUID);
                return qtState.compareTo(new QuantityType<>(type.toBigDecimal(), qtState.getUnit())) <= 0;
            } else if (compareState instanceof QuantityType type) {
                return qtState.compareTo(type) <= 0;
        } else if (itemState instanceof PercentType type && null != compareState) {
            // we need to handle PercentType first, otherwise the comparison will fail
            PercentType percentState = compareState.as(PercentType.class);
            if (null != percentState) {
                return type.compareTo(percentState) <= 0;
        } else if (itemState instanceof DecimalType type && null != compareState) {
            DecimalType decimalState = compareState.as(DecimalType.class);
            if (null != decimalState) {
                return type.compareTo(decimalState) <= 0;
    private boolean greaterThanOrEqualsToItemState(String itemName, String state) throws ItemNotFoundException {
            return itemTime.compareTo(compareTime) >= 0;
                return qtState.compareTo(new QuantityType<>(type.toBigDecimal(), qtState.getUnit())) >= 0;
                return qtState.compareTo(type) >= 0;
                return type.compareTo(percentState) >= 0;
                return type.compareTo(decimalState) >= 0;
    private boolean equalsToItemState(String itemName, String state) throws ItemNotFoundException {
        if (itemState instanceof QuantityType qtState && compareState instanceof DecimalType type) {
            if (Units.ONE.equals(qtState.getUnit())) {
                // allow compareStates without unit if the unit of the state equals to ONE
                return itemState.equals(new QuantityType<>(type.toBigDecimal(), qtState.getUnit()));
                // log a warning if the unit of the state differs from ONE
                        "Received a QuantityType state '{}' with unit for item {}, but the condition is defined as a plain number without unit ({}), comparison will fail unless a unit is added to the condition for rule {}.",
                        itemState, itemName, state, ruleUID);
        return itemState.equals(compareState);
    private Instant getCompareTime(String input) {
        if (input.isBlank()) {
            // no parameter given, use now
            return ZonedDateTime.parse(input).toInstant();
        } catch (DateTimeParseException ignored) {
            return LocalDateTime.parse(input, DateTimeFormatter.ISO_LOCAL_DATE_TIME)
                    .atZone(timeZoneProvider.getTimeZone()).toInstant();
            int dayPosition = input.indexOf("D");
            if (dayPosition == -1) {
                // no date in string, add period symbol and time separator
                return Instant.now().plus(Duration.parse("PT" + input));
            } else if (dayPosition == input.length() - 1) {
                // day is the last symbol, only add the period symbol
                return Instant.now().plus(Duration.parse("P" + input));
                // add period symbol and time separator
                return Instant.now().plus(Duration
                        .parse("P" + input.substring(0, dayPosition + 1) + "T" + input.substring(dayPosition + 1)));
            logger.warn("Couldn't get a comparable time from '{}', using now", input);
