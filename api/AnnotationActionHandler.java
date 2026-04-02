package org.openhab.core.automation.internal.module.handler;
import java.lang.annotation.Annotation;
import java.lang.reflect.InvocationTargetException;
import java.time.LocalDate;
import java.time.LocalTime;
import org.openhab.core.automation.annotation.ActionInput;
 * ActionHandler which is dynamically created upon annotation on services
 * @author Laurent Garnier - Added ActionInputsHelper
public class AnnotationActionHandler extends BaseActionModuleHandler {
    public static final String MODULE_RESULT = "result";
    private final Logger logger = LoggerFactory.getLogger(AnnotationActionHandler.class);
    private final Method method;
    private final ActionType moduleType;
    private final Object actionProvider;
    public AnnotationActionHandler(Action module, ActionType mt, Method method, Object actionProvider,
            ActionInputsHelper actionInputsHelper) {
        this.moduleType = mt;
        this.actionProvider = actionProvider;
        Map<String, @Nullable Object> output = new HashMap<>();
        Annotation[][] annotations = method.getParameterAnnotations();
        List<@Nullable Object> args = new ArrayList<>();
        for (int i = 0; i < annotations.length; i++) {
            Annotation[] annotationsOnParam = annotations[i];
            if (annotationsOnParam != null && annotationsOnParam.length == 1) {
                if (annotationsOnParam[0] instanceof ActionInput inputAnnotation) {
                    // check if the moduleType has a configdescription with this input
                    if (hasInput(moduleType, inputAnnotation.name())) {
                        Object value = context.get(inputAnnotation.name());
                        // fallback to configuration as this is where the UI stores the input values
                            Object configValue = module.getConfiguration().get(inputAnnotation.name());
                                    value = actionInputsHelper.mapSerializedInputToActionInput(
                                            moduleType.getInputs().get(i), configValue);
                                    logger.debug("{} Input parameter is ignored.", e.getMessage());
                                    // Ignore it and keep null in value
                        args.add(i, value);
                                "Annotated method defines input '{}' but the module type '{}' does not specify an input with this name.",
                                inputAnnotation.name(), moduleType);
                // no annotation on parameter, try to fetch the generic parameter from the context
                args.add(i, context.get("p" + i));
        Object result = null;
        Object @Nullable [] arguments = args.toArray();
        if (arguments.length > 0 && logger.isDebugEnabled()) {
            logger.debug("Calling action method {} with the following arguments:", method.getName());
            for (int i = 0; i < arguments.length; i++) {
                if (arguments[i] == null) {
                    logger.debug("  - Argument {}: null", i);
                    logger.debug("  - Argument {}: type {} value {}", i, arguments[i].getClass().getCanonicalName(),
                            arguments[i]);
            result = method.invoke(this.actionProvider, arguments);
        } catch (IllegalAccessException | IllegalArgumentException | InvocationTargetException e) {
            logger.error("Could not call method '{}' from module type '{}'.", method, moduleType.getUID(), e);
        if (result != null) {
            if (result instanceof Map<?, ?>) {
                    Map<String, Object> resultMap = (Map<String, Object>) result;
                    for (Entry<String, Object> entry : resultMap.entrySet()) {
                        if (hasOutput(moduleType, entry.getKey())) {
                            output.put(entry.getKey(), entry.getValue());
                } catch (ClassCastException ex) {
                            "The return type of action method '{}' from module type '{}' should be Map<String, Object>, because {}",
                            method, moduleType.getUID(), ex.getMessage());
                // we allow simple data types as return values and put them under the context key "result".
            } else if (result instanceof Boolean booleanValue) {
                output.put(MODULE_RESULT, booleanValue);
            } else if (result instanceof String stringValue) {
                output.put(MODULE_RESULT, stringValue);
            } else if (result instanceof Byte byteValue) {
                output.put(MODULE_RESULT, byteValue);
            } else if (result instanceof Short shortValue) {
                output.put(MODULE_RESULT, shortValue);
            } else if (result instanceof Integer integerValue) {
                output.put(MODULE_RESULT, integerValue);
            } else if (result instanceof Long longValue) {
                output.put(MODULE_RESULT, longValue);
            } else if (result instanceof Double doubleValue) {
                output.put(MODULE_RESULT, doubleValue);
            } else if (result instanceof Float floatValue) {
                output.put(MODULE_RESULT, floatValue);
            } else if (result instanceof BigDecimal bigDecimalValue) {
                output.put(MODULE_RESULT, bigDecimalValue.doubleValue());
            } else if (result instanceof QuantityType<?> || result instanceof LocalDate || result instanceof LocalTime
                    || result instanceof LocalDateTime || result instanceof ZonedDateTime || result instanceof Instant
                    || result instanceof Duration) {
                output.put(MODULE_RESULT, result.toString());
                logger.warn("Non compatible return type '{}' on action method.", result.getClass());
    private boolean hasInput(ActionType moduleType, String in) {
        for (Input i : moduleType.getInputs()) {
            if (i.getName().equals(in)) {
    private boolean hasOutput(ActionType moduleType, String out) {
        for (Output o : moduleType.getOutputs()) {
            if (o.getName().equals(out)) {
