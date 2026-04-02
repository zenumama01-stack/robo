 * A NumberItem has a decimal value and is usually used for all kinds
 * of sensors, like temperature, brightness, wind, etc.
 * It can also be used as a counter or as any other thing that can be expressed
 * as a number.
public class NumberItem extends GenericItem implements MetadataAwareItem {
    public static final String UNIT_METADATA_NAMESPACE = "unit";
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(DecimalType.class,
            QuantityType.class, UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(DecimalType.class,
            QuantityType.class, RefreshType.class);
    private final Logger logger = LoggerFactory.getLogger(NumberItem.class);
    private final @Nullable Class<? extends Quantity<?>> dimension;
    private Unit<?> unit = Units.ONE;
    private final @Nullable UnitProvider unitProvider;
    public NumberItem(String name) {
        this(CoreItemFactory.NUMBER, name, null);
    public NumberItem(String type, String name, @Nullable UnitProvider unitProvider) {
        String itemTypeExtension = ItemUtil.getItemTypeExtension(getType());
        if (itemTypeExtension != null) {
            Class<? extends Quantity<?>> dimension = UnitUtils.parseDimension(itemTypeExtension);
                throw new IllegalArgumentException("The given dimension " + itemTypeExtension + " is unknown.");
            } else if (unitProvider == null) {
                throw new IllegalArgumentException("A unit provider is required for items with a dimension.");
            this.unit = unitProvider.getUnit((Class<? extends Quantity>) dimension);
            logger.trace("Item '{}' now has unit '{}'", name, unit);
            dimension = null;
     * Send a DecimalType command to the item.
    public void send(DecimalType command) {
    public void send(DecimalType command, @Nullable String source) {
     * Send a QuantityType command to the item.
    public void send(QuantityType<?> command) {
    public void send(QuantityType<?> command, @Nullable String source) {
            DecimalType strippedCommand = new DecimalType(command);
            internalSend(strippedCommand, source);
        } else if (command.getUnit().isCompatible(unit) || command.getUnit().inverse().isCompatible(unit)) {
            logger.warn("Command '{}' to item '{}' was rejected because it is incompatible with the item unit '{}'",
                    command, name, unit);
        StateDescription stateDescription = super.getStateDescription(locale);
        if (getDimension() == null && stateDescription != null) {
            if (pattern != null && pattern.contains(UnitUtils.UNIT_PLACEHOLDER)) {
                return StateDescriptionFragmentBuilder.create(stateDescription)
                        .withPattern(pattern.replaceAll(UnitUtils.UNIT_PLACEHOLDER, "").trim()).build()
     * Returns the {@link javax.measure.Dimension} associated with this {@link NumberItem}, may be null.
     * @return the {@link javax.measure.Dimension} associated with this {@link NumberItem}, may be null.
    public @Nullable Class<? extends Quantity<?>> getDimension() {
        return dimension;
    private @Nullable State getInternalState(State state) {
        if (state instanceof QuantityType<?> quantityType) {
                // QuantityType update to a NumberItem without unit, strip unit
                return new DecimalType(quantityType);
                // QuantityType update to a NumberItem with unit, convert to item unit (if possible)
                Unit<?> stateUnit = quantityType.getUnit();
                State convertedState = (stateUnit.isCompatible(unit) || stateUnit.inverse().isCompatible(unit))
                        ? quantityType.toInvertibleUnit(unit)
                    logger.warn("Failed to update item '{}' because '{}' could not be converted to the item unit '{}'",
                            name, state, unit);
                // DecimalType update to NumberItem with unit
                return decimalType;
                // DecimalType update for a NumberItem with dimension, convert to QuantityType
                return new QuantityType<>(decimalType.doubleValue(), unit);
            State internalState = getInternalState(state);
            if (internalState != null) {
                applyState(internalState, source);
        TimeSeries internalSeries = new TimeSeries(timeSeries.getPolicy());
        timeSeries.getStates().forEach(s -> internalSeries.add(s.timestamp(),
                Objects.requireNonNullElse(getInternalState(s.state()), UnDefType.NULL)));
        if (dimension != null && internalSeries.getStates().allMatch(s -> s.state() instanceof QuantityType<?>)) {
            applyTimeSeries(internalSeries);
        } else if (internalSeries.getStates().allMatch(s -> s.state() instanceof DecimalType)) {
     * Returns the optional unit symbol for this {@link NumberItem}.
     * @return the optional unit symbol for this {@link NumberItem}.
    public @Nullable String getUnitSymbol() {
        return (dimension != null) ? unit.toString() : null;
     * Get the unit for this item, either:
     * <li>the unit retrieved from the <code>unit</code> namespace in the item's metadata</li>
     * <li>the default system unit for the item's dimension</li>
     * @return the {@link Unit} for this item if available, {@code null} otherwise.
    public @Nullable Unit<? extends Quantity<?>> getUnit() {
        return (dimension != null) ? unit : null;
        if (dimension != null && UNIT_METADATA_NAMESPACE.equals(metadata.getUID().getNamespace())) {
            Unit<?> unit = UnitUtils.parseUnit(metadata.getValue());
            if ((unit == null) || (!unit.isCompatible(this.unit) && !unit.inverse().isCompatible(this.unit))) {
                logger.warn("Unit '{}' could not be parsed to a known unit. Keeping old unit '{}' for item '{}'.",
                        metadata.getValue(), this.unit, name);
        addedMetadata(newMetadata);
        Class<? extends Quantity<?>> dimension = this.dimension;
            unit = Objects.requireNonNull(unitProvider).getUnit((Class<? extends Quantity>) dimension);
