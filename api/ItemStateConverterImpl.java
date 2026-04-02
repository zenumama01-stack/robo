 * Convert a {@link State} to an {@link Item} accepted {@link State}.
public class ItemStateConverterImpl implements ItemStateConverter {
    private final Logger logger = LoggerFactory.getLogger(ItemStateConverterImpl.class);
    public ItemStateConverterImpl(final @Reference UnitProvider unitProvider) {
    public State convertToAcceptedState(@Nullable State state, @Nullable Item item) {
            logger.error("A conversion of null was requested:",
                    new IllegalArgumentException("State must not be null."));
            return UnDefType.NULL;
        if (item != null && !isAccepted(item, state)) {
                State convertedState = state.as(acceptedType);
                if (convertedState != null) {
                    logger.debug("Converting {} '{}' to {} '{}' for item '{}'", state.getClass().getSimpleName(), state,
                            convertedState.getClass().getSimpleName(), convertedState, item.getName());
                    return convertedState;
        if (item instanceof NumberItem numberItem && state instanceof QuantityType quantityState) {
            if (numberItem.getDimension() != null) {
                // in case the item does define a unit it takes precedence over all other conversions:
                Unit<?> itemUnit = parseItemUnit(numberItem);
                if (itemUnit != null) {
                    if (!itemUnit.equals(quantityState.getUnit())) {
                        return convertOrUndef(quantityState, itemUnit);
                Class<? extends Quantity<?>> dimension = numberItem.getDimension();
                // explicit cast to Class<? extends Quantity> as JDK compiler complains
                Unit<? extends Quantity<?>> conversionUnit = dimension == null ? null
                        : unitProvider.getUnit((Class<? extends Quantity>) dimension);
                if (conversionUnit != null
                        && UnitUtils.isDifferentMeasurementSystem(conversionUnit, quantityState.getUnit())) {
                    return convertOrUndef(quantityState, conversionUnit);
                State convertedState = state.as(DecimalType.class);
                    // convertedState is always returned because the state is an instance
                    // of QuantityType which never returns null for as(DecimalType.class)
    private State convertOrUndef(QuantityType<?> quantityState, Unit<?> targetUnit) {
        QuantityType<?> converted = quantityState.toInvertibleUnit(targetUnit);
        if (converted != null) {
    private @Nullable Unit<?> parseItemUnit(NumberItem numberItem) {
        StateDescription stateDescription = numberItem.getStateDescription();
            return UnitUtils.parseUnit(pattern);
    private boolean isAccepted(Item item, State state) {
        return item.getAcceptedDataTypes().contains(state.getClass());
