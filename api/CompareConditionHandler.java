import org.openhab.core.automation.internal.module.exception.UncomparableException;
 * Generic Comparation Condition
public class CompareConditionHandler extends BaseConditionModuleHandler {
    public static final String MODULE_TYPE = "core.GenericCompareCondition";
    public static final String INPUT_LEFT_OBJECT = "input";
    public static final String INPUT_LEFT_FIELD = "inputproperty";
    public static final String RIGHT_OP = "right";
    public static final String OPERATOR = "operator";
    public final Logger logger = LoggerFactory.getLogger(CompareConditionHandler.class);
    public CompareConditionHandler(Condition module) {
    public boolean isSatisfied(Map<String, @Nullable Object> context) {
        Object operatorObj = this.module.getConfiguration().get(OPERATOR);
        String operator = operatorObj instanceof String s ? s : null;
        Object rightObj = this.module.getConfiguration().get(RIGHT_OP);
        String rightOperandString = rightObj instanceof String s ? s : null;
        Object leftObjFieldNameObj = this.module.getConfiguration().get(INPUT_LEFT_FIELD);
        String leftObjectFieldName = leftObjFieldNameObj instanceof String s ? s : null;
        if (rightOperandString == null || operator == null) {
            Object leftObj = context.get(INPUT_LEFT_OBJECT);
            Object toCompare = getCompareValue(leftObj, leftObjectFieldName);
            Object rightValue = getRightOperandValue(rightOperandString, toCompare);
            if (rightValue == null) {
                if (leftObj != null) {
                    logger.info("unsupported type for compare condition: {}", leftObj.getClass());
                    logger.info("unsupported type for compare condition: null ({})",
                            module.getInputs().get(INPUT_LEFT_FIELD));
                    case "eq":
                    case "EQ":
                    case "=":
                    case "==":
                    case "equals":
                    case "EQUALS":
                        // EQUALS
                        if (toCompare == null) {
                            return "null".equals(rightOperandString) || "".equals(rightOperandString);
                            return toCompare.equals(rightValue);
                    case "gt":
                    case "GT":
                    case ">":
                        // Greater
                        return (toCompare != null) && (compare(toCompare, rightValue) > 0);
                    case "gte":
                    case "GTE":
                    case ">=":
                    case "=>":
                        // Greater or equal
                        return (toCompare != null) && (compare(toCompare, rightValue) >= 0);
                    case "lt":
                    case "LT":
                    case "<":
                        return (toCompare != null) && (compare(toCompare, rightValue) < 0);
                    case "lte":
                    case "LTE":
                    case "<=":
                    case "=<":
                        return (toCompare != null) && (compare(toCompare, rightValue) <= 0);
                    case "matches":
                        // Matcher...
                        if (toCompare instanceof String string1 && rightValue instanceof String string2) {
                            return string1.matches(string2);
            } catch (UncomparableException e) {
                // values can not be compared, so assume that the condition is not satisfied
    private int compare(Object a, Object b) throws UncomparableException {
        if (Comparable.class.isAssignableFrom(a.getClass()) && a.getClass().equals(b.getClass())) {
                return ((Comparable) a).compareTo(b);
            } catch (ClassCastException e) {
                // should never happen but to be save here!
                throw new UncomparableException();
    private @Nullable Object getRightOperandValue(String rightOperandString2, @Nullable Object toCompare) {
        if ("null".equals(rightOperandString2)) {
            return rightOperandString2;
        if (toCompare instanceof State state) {
            return TypeParser.parseState(List.of(state.getClass()), rightOperandString2);
        } else if (toCompare instanceof Integer) {
            return Integer.parseInt(rightOperandString2);
        } else if (toCompare instanceof String) {
        } else if (toCompare instanceof Long) {
            return Long.parseLong(rightOperandString2);
        } else if (toCompare instanceof Double) {
            return Double.parseDouble(rightOperandString2);
    private @Nullable Object getCompareValue(@Nullable Object leftObj, @Nullable String leftObjFieldName) {
        if (leftObj == null || leftObjFieldName == null || leftObjFieldName.isEmpty() || leftObj instanceof String
                || leftObj instanceof Integer || leftObj instanceof Long || leftObj instanceof Double) {
            return leftObj;
                Method m = leftObj.getClass().getMethod(
                        "get" + leftObjFieldName.substring(0, 1).toUpperCase() + leftObjFieldName.substring(1));
                return m.invoke(leftObj);
            } catch (NoSuchMethodException | SecurityException | StringIndexOutOfBoundsException
                    | IllegalAccessException | IllegalArgumentException | InvocationTargetException e) {
