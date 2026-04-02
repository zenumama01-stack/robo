import java.time.temporal.TemporalAccessor;
import javax.measure.Quantity;
import javax.measure.Unit;
import org.openhab.core.i18n.UnitProvider;
import org.openhab.core.types.util.UnitUtils;
 * This is an utility class to convert the {@link Input}s of a {@link ActionType} into a list of
 * {@link ConfigDescriptionParameter}s and to convert serialised inputs to the Java types required by the
 * {@link Input}s of a {@link ActionType}.
 * @author Laurent Garnier and Florian Hotze - Initial contribution
@Component(service = ActionInputsHelper.class)
public class ActionInputsHelper {
    private static final Pattern QUANTITY_TYPE_PATTERN = Pattern
            .compile("([a-z0-9]+\\.)*QuantityType<([a-z0-9]+\\.)*(?<dimension>[A-Z][a-zA-Z0-9]*)>");
    private final Logger logger = LoggerFactory.getLogger(ActionInputsHelper.class);
    private final UnitProvider unitProvider;
    public ActionInputsHelper(final @Reference TimeZoneProvider timeZoneProvider,
            final @Reference UnitProvider unitProvider) {
        this.unitProvider = unitProvider;
     * Maps a list of {@link Input}s to a list of {@link ConfigDescriptionParameter}s.
     * @param inputs the list of inputs to map to config description parameters
     * @return the list of config description parameters
     * @throws IllegalArgumentException if at least one of the input parameters has an unsupported type
    public List<ConfigDescriptionParameter> mapActionInputsToConfigDescriptionParameters(List<Input> inputs)
            throws IllegalArgumentException {
        List<ConfigDescriptionParameter> configDescriptionParameters = new ArrayList<>();
            configDescriptionParameters.add(mapActionInputToConfigDescriptionParameter(input));
        return configDescriptionParameters;
     * Maps an {@link Input} to a {@link ConfigDescriptionParameter}.
     * @param input the input to map to a config description parameter
     * @return the config description parameter
     * @throws IllegalArgumentException if the input parameter has an unsupported type
    public ConfigDescriptionParameter mapActionInputToConfigDescriptionParameter(Input input)
        ConfigDescriptionParameter.Type parameterType = ConfigDescriptionParameter.Type.TEXT;
        String defaultValue = null;
        Unit<?> unit = null;
        String context = null;
        BigDecimal step = null;
        Matcher matcher = QUANTITY_TYPE_PATTERN.matcher(input.getType());
            parameterType = ConfigDescriptionParameter.Type.DECIMAL;
            step = BigDecimal.ZERO;
                unit = getDefaultUnit(matcher.group("dimension"));
                throw new IllegalArgumentException("Input parameter '" + input.getName() + "' with type "
                        + input.getType() + "cannot be converted into a config description parameter!", e);
            switch (input.getType()) {
                    defaultValue = "false";
                    required = true;
                case "java.lang.Boolean":
                    parameterType = ConfigDescriptionParameter.Type.BOOLEAN;
                case "byte":
                case "short":
                case "int":
                case "long":
                    defaultValue = "0";
                case "java.lang.Byte":
                case "java.lang.Short":
                case "java.lang.Integer":
                case "java.lang.Long":
                    parameterType = ConfigDescriptionParameter.Type.INTEGER;
                case "float":
                case "double":
                case "java.lang.Float":
                case "java.lang.Double":
                case "java.lang.Number":
                case "org.openhab.core.library.types.DecimalType":
                case "java.lang.String":
                case "java.time.LocalDate":
                    context = "date";
                case "java.time.LocalTime":
                    context = "time";
                    step = BigDecimal.ONE;
                case "java.time.LocalDateTime":
                case "java.util.Date":
                case "java.time.ZonedDateTime":
                case "java.time.Instant":
                    context = "datetime";
                case "java.time.Duration":
                    // There is no available configuration parameter context for these types.
                    // A text parameter is used. The expected value must respect a particular format specific
                    // to each of these types.
                            + input.getType() + "cannot be converted into a config description parameter!");
        ConfigDescriptionParameterBuilder builder = ConfigDescriptionParameterBuilder
                .create(input.getName(), parameterType).withLabel(input.getLabel())
                .withDescription(input.getDescription()).withReadOnly(false)
                .withRequired(required || input.isRequired()).withContext(context);
        String inputDefaultValue = input.getDefaultValue();
        if (inputDefaultValue != null && !inputDefaultValue.isEmpty()) {
            builder = builder.withDefault(inputDefaultValue);
        } else if (defaultValue != null) {
            builder = builder.withDefault(defaultValue);
        if (unit != null) {
            builder = builder.withUnit(unit.getSymbol());
        if (step != null) {
            builder = builder.withStepSize(step);
     * Maps serialised inputs to the Java types required by the {@link Input}s of the given {@link ActionType}.
     * @param actionType the action type whose inputs to consider
     * @param arguments the serialised arguments
     * @return the mapped arguments
    public Map<String, Object> mapSerializedInputsToActionInputs(ActionType actionType, Map<String, Object> arguments) {
        Map<String, Object> newArguments = new HashMap<>();
        for (Input input : actionType.getInputs()) {
            Object value = arguments.get(input.getName());
                    newArguments.put(input.getName(), mapSerializedInputToActionInput(input, value));
                    logger.warn("{} Input parameter is ignored.", e.getMessage());
                value = getDefaultValueForActionInput(input);
                    newArguments.put(input.getName(), value);
        return newArguments;
    private @Nullable Object getDefaultValueForActionInput(Input input) {
        return switch (input.getType()) {
            case "boolean" -> false;
            case "byte" -> (byte) 0;
            case "short" -> (short) 0;
            case "int" -> 0;
            case "long" -> 0L;
            case "float" -> 0.0f;
            case "double" -> 0.0d;
     * Maps a serialised input to the Java type required by the given {@link Input}.
     * @param input the input whose type to consider
     * @param argument the serialised argument
     * @return the mapped argument
     * @throws IllegalArgumentException if the mapping failed
    public Object mapSerializedInputToActionInput(Input input, Object argument) throws IllegalArgumentException {
            if (argument instanceof Double valueDouble) {
                // When an integer value is provided as input value, the value type in the Map is Double.
                // We have to convert Double type into the target type.
                    return new QuantityType<>(valueDouble, getDefaultUnit(matcher.group("dimension")));
                        case "byte", "java.lang.Byte" -> Byte.valueOf(valueDouble.byteValue());
                        case "short", "java.lang.Short" -> Short.valueOf(valueDouble.shortValue());
                        case "int", "java.lang.Integer" -> Integer.valueOf(valueDouble.intValue());
                        case "long", "java.lang.Long" -> Long.valueOf(valueDouble.longValue());
                        case "float", "java.lang.Float" -> Float.valueOf(valueDouble.floatValue());
                        case "org.openhab.core.library.types.DecimalType" -> new DecimalType(valueDouble);
                        default -> argument;
            } else if (argument instanceof String valueString) {
                // String value is accepted to instantiate few target types
                    // The string can contain either a simple decimal value without unit or a decimal value with
                    // unit
                        BigDecimal bigDecimal = new BigDecimal(valueString);
                        return new QuantityType<>(bigDecimal, getDefaultUnit(matcher.group("dimension")));
                        return new QuantityType<>(valueString);
                        case "boolean", "java.lang.Boolean" -> Boolean.valueOf(valueString.toLowerCase());
                        case "byte", "java.lang.Byte" -> Byte.valueOf(valueString);
                        case "short", "java.lang.Short" -> Short.valueOf(valueString);
                        case "int", "java.lang.Integer" -> Integer.valueOf(valueString);
                        case "long", "java.lang.Long" -> Long.valueOf(valueString);
                        case "float", "java.lang.Float" -> Float.valueOf(valueString);
                        case "double", "java.lang.Double", "java.lang.Number" -> Double.valueOf(valueString);
                        case "org.openhab.core.library.types.DecimalType" -> new DecimalType(valueString);
                        case "java.time.LocalDate" ->
                            // Accepted format is: 2007-12-03
                            LocalDate.parse(valueString);
                        case "java.time.LocalTime" ->
                            // Accepted format is: 10:15:30
                            LocalTime.parse(valueString);
                        case "java.time.LocalDateTime" ->
                            // Accepted format is: 2007-12-03T10:15:30
                            LocalDateTime.parse(valueString);
                        case "java.util.Date" ->
                             * Accepted format is one of:
                             * - 2007-12-03T10:15:30
                             * - 2007-12-03T09:15:30Z
                             * - 2007-12-03T10:15:30+01:00
                             * - 2007-12-03T10:15:30+01:00[Europe/Paris]
                            Date.from(parseAsIsoDateTime(valueString).toInstant());
                        case "java.time.ZonedDateTime" ->
                            parseAsIsoDateTime(valueString);
                        case "java.time.Instant" ->
                            parseAsIsoDateTime(valueString).toInstant();
                        case "java.time.Duration" ->
                            // Accepted format is: P2DT17H25M30.5S
                            Duration.parse(valueString);
        } catch (IllegalArgumentException | DateTimeParseException e) {
            throw new IllegalArgumentException("Input parameter '" + input.getName() + "': converting value '"
                    + argument.toString() + "' into type " + input.getType() + " failed!");
    private Unit<?> getDefaultUnit(String dimensionName) throws IllegalArgumentException {
        Class<? extends Quantity<?>> dimension = UnitUtils.parseDimension(dimensionName);
        if (dimension == null) {
            throw new IllegalArgumentException("Unknown dimension " + dimensionName);
        return unitProvider.getUnit((Class<? extends Quantity>) dimension);
    private ZonedDateTime parseAsIsoDateTime(String valueString) {
        TemporalAccessor dt = DateTimeFormatter.ISO_DATE_TIME.parseBest(valueString, ZonedDateTime::from,
                LocalDateTime::from);
        if (dt instanceof ZonedDateTime zdt) {
            return zdt;
        return ((LocalDateTime) dt).atZone(timeZoneProvider.getTimeZone());
