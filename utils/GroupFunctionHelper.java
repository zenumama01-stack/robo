import org.openhab.core.library.types.DateTimeGroupFunction;
import org.openhab.core.library.types.QuantityTypeArithmeticGroupFunction;
 * Creates {@link GroupFunction}s according to the given parameters.
 * @author Robert Michalak - LATEST and EARLIEST group functions
public class GroupFunctionHelper {
    private final Logger logger = LoggerFactory.getLogger(GroupFunctionHelper.class);
     * Creates a {@link GroupFunction} according to the given parameters. In case dimension is given the resulting
     * arithmetic group function will take unit conversion into account.
     * @param function the {@link GroupFunctionDTO} describing the group function.
     * @param baseItem an optional {@link Item} defining the dimension/unit for unit conversion.
     * @return a {@link GroupFunction} according to the given parameters.
    public GroupFunction createGroupFunction(GroupFunctionDTO function, @Nullable Item baseItem) {
        if (baseItem instanceof NumberItem baseNumberItem && baseNumberItem.getDimension() != null) {
            return createDimensionGroupFunction(function, baseNumberItem);
        return createDefaultGroupFunction(function, baseItem);
    private List<State> parseStates(@Nullable Item baseItem, String @Nullable [] params) {
        if (params == null || baseItem == null) {
        List<State> states = new ArrayList<>();
        for (String param : params) {
            State state = TypeParser.parseState(baseItem.getAcceptedDataTypes(), param);
                logger.warn("State '{}' is not valid for a group item with base type '{}'", param, baseItem.getType());
                states.clear();
                states.add(state);
        return states;
    private GroupFunction createDimensionGroupFunction(GroupFunctionDTO function, NumberItem baseItem) {
        final String functionName = function.name;
        Unit<?> baseItemUnit = baseItem.getUnit();
        if (baseItemUnit != null) {
            switch (functionName.toUpperCase()) {
                case GroupFunction.AVG:
                    return new QuantityTypeArithmeticGroupFunction.Avg(baseItemUnit);
                case GroupFunction.MEDIAN:
                    return new QuantityTypeArithmeticGroupFunction.Median(baseItemUnit);
                case GroupFunction.SUM:
                    return new QuantityTypeArithmeticGroupFunction.Sum(baseItemUnit);
                case GroupFunction.MIN:
                    return new QuantityTypeArithmeticGroupFunction.Min(baseItemUnit);
                case GroupFunction.MAX:
                    return new QuantityTypeArithmeticGroupFunction.Max(baseItemUnit);
    private GroupFunction createDefaultGroupFunction(GroupFunctionDTO function, @Nullable Item baseItem) {
        final List<State> args;
            case GroupFunction.AND:
                args = parseStates(baseItem, function.params);
                if (args.size() == 2) {
                    return new ArithmeticGroupFunction.And(args.getFirst(), args.get(1));
                    logger.error("Group function 'AND' requires two arguments. Using Equality instead.");
            case GroupFunction.OR:
                    return new ArithmeticGroupFunction.Or(args.getFirst(), args.get(1));
                    logger.error("Group function 'OR' requires two arguments. Using Equality instead.");
            case GroupFunction.NAND:
                    return new ArithmeticGroupFunction.NAnd(args.getFirst(), args.get(1));
                    logger.error("Group function 'NOT AND' requires two arguments. Using Equality instead.");
            case GroupFunction.NOR:
                    return new ArithmeticGroupFunction.NOr(args.getFirst(), args.get(1));
                    logger.error("Group function 'NOT OR' requires two arguments. Using Equality instead.");
            case GroupFunction.XOR:
                    return new ArithmeticGroupFunction.Xor(args.getFirst(), args.get(1));
                    logger.error("Group function 'XOR' requires two arguments. Using Equality instead.");
            case GroupFunction.COUNT:
                if (function.params != null && function.params.length == 1) {
                    State countParam = new StringType(function.params[0]);
                    return new ArithmeticGroupFunction.Count(countParam);
                    logger.error("Group function 'COUNT' requires one argument. Using Equality instead.");
                return new ArithmeticGroupFunction.Avg();
                return new ArithmeticGroupFunction.Median();
                return new ArithmeticGroupFunction.Sum();
                return new ArithmeticGroupFunction.Min();
                return new ArithmeticGroupFunction.Max();
            case GroupFunction.LATEST:
                return new DateTimeGroupFunction.Latest();
            case GroupFunction.EARLIEST:
                return new DateTimeGroupFunction.Earliest();
            case GroupFunction.EQUALITY:
                return new GroupFunction.Equality();
                logger.error("Unknown group function '{}'. Using Equality instead.", functionName);
