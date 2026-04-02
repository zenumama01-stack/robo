import java.lang.reflect.Parameter;
import java.math.BigInteger;
import java.security.MessageDigest;
import org.openhab.core.automation.annotation.ActionOutput;
import org.openhab.core.automation.annotation.ActionOutputs;
import org.openhab.core.automation.annotation.ActionScope;
 * Helper methods for {@link AnnotatedActions} {@link ModuleTypeProvider}
 * @author Florian Hotze - Added configuration description parameters for thing modules, Added method signature hash to
 *         module ID in case of method overloads
 * @author Laurent Garnier - Converted into a an OSGi component
@Component(service = AnnotationActionModuleTypeHelper.class)
public class AnnotationActionModuleTypeHelper {
    private final Logger logger = LoggerFactory.getLogger(AnnotationActionModuleTypeHelper.class);
    private static final String SELECT_SERVICE_LABEL = "Select Service Instance";
    private static final String SELECT_THING_LABEL = "Select Thing";
    public static final String CONFIG_PARAM = "config";
    public AnnotationActionModuleTypeHelper(final @Reference ActionInputsHelper actionInputsHelper) {
    public Collection<ModuleInformation> parseAnnotations(Object actionProvider) {
        Class<?> clazz = actionProvider.getClass();
        if (clazz.isAnnotationPresent(ActionScope.class)) {
            ActionScope scope = clazz.getAnnotation(ActionScope.class);
            return parseAnnotations(scope.name(), actionProvider);
    public Collection<ModuleInformation> parseAnnotations(String name, Object actionProvider) {
        Collection<ModuleInformation> moduleInformation = new ArrayList<>();
        Method[] methods = clazz.getDeclaredMethods();
            if (method.isAnnotationPresent(RuleAction.class)) {
                List<Input> inputs = getInputsFromAction(method);
                List<Output> outputs = getOutputsFromAction(method);
                RuleAction ruleAction = method.getAnnotation(RuleAction.class);
                String uid = getModuleIdFromMethod(name, method);
                Set<String> tags = new HashSet<>(Arrays.asList(ruleAction.tags()));
                ModuleInformation mi = new ModuleInformation(uid, actionProvider, method);
                mi.setLabel(ruleAction.label());
                mi.setDescription(ruleAction.description());
                mi.setVisibility(ruleAction.visibility());
                mi.setInputs(inputs);
                mi.setOutputs(outputs);
                mi.setTags(tags);
                moduleInformation.add(mi);
        return moduleInformation;
    public String getModuleIdFromMethod(String actionScope, Method method) {
        String uid = actionScope + "." + method.getName() + "#";
        MessageDigest md5 = null;
            md5 = MessageDigest.getInstance("MD5");
        } catch (NoSuchAlgorithmException e) {
        for (Class<?> parameter : method.getParameterTypes()) {
            md5.update(parameter.getName().getBytes());
        return uid + String.format("%032x", new BigInteger(1, md5.digest()));
    private List<Input> getInputsFromAction(Method method) {
        List<Input> inputs = new ArrayList<>();
        Parameter[] params = method.getParameters();
            Parameter param = params[i];
            Annotation[] paramAnnotations = annotations[i];
            if (paramAnnotations.length == 0) {
                // we do not have an annotation with a name for this parameter
                inputs.add(new Input("p" + i, param.getType().getCanonicalName(), "", "", Set.of(), false, "", ""));
            } else if (paramAnnotations.length == 1) {
                Annotation a = paramAnnotations[0];
                if (a instanceof ActionInput inp) {
                    // check if a type is given, otherwise use the java type specified on the parameter
                    String type;
                    if (!"".equals(inp.type())) {
                        type = inp.type();
                        type = param.getType().getCanonicalName();
                    inputs.add(new Input(inp.name(), type, inp.label(), inp.description(),
                            Arrays.stream(inp.tags()).collect(Collectors.toSet()), inp.required(), inp.reference(),
                            inp.defaultValue()));
    private Output getOutputFromActionOutputAnnotation(ActionOutput ruleActionOutput, @Nullable String nameOverride) {
        return new Output((nameOverride != null ? nameOverride : ruleActionOutput.name()), ruleActionOutput.type(),
                ruleActionOutput.label(), ruleActionOutput.description(),
                Arrays.stream(ruleActionOutput.tags()).collect(Collectors.toSet()), ruleActionOutput.reference(),
                ruleActionOutput.defaultValue());
    private List<Output> getOutputsFromAction(Method method) {
        // ActionOutputs annotation
        if (method.isAnnotationPresent(ActionOutputs.class)) {
            for (ActionOutput ruleActionOutput : method.getAnnotation(ActionOutputs.class).value()) {
                outputs.add(getOutputFromActionOutputAnnotation(ruleActionOutput, null));
            // no ActionOutputs annotation, but a Map<String, Object> return type
        } else if (method.getAnnotatedReturnType().toString()
                .equals("java.util.Map<java.lang.String, java.lang.Object>")) {
                    "Method {}::{} returns a Map<String, Object> but is not annotated with ActionOutputs. This should be fixed in the binding.",
                    method.getDeclaringClass().getSimpleName(), method.getName());
            // no ActionOutputs annotation and no Map<String, Object> return type, but a single ActionOutput annotation
        } else if (method.isAnnotationPresent(ActionOutput.class)) {
            ActionOutput ruleActionOutput = method.getAnnotation(ActionOutput.class);
            if (!ruleActionOutput.name().equals(MODULE_RESULT)) {
                        "Method {}::{} has a single output but does not use the default output name in the ActionOutput annotation. This should be fixed in the binding.",
            outputs.add(getOutputFromActionOutputAnnotation(ruleActionOutput, MODULE_RESULT));
    public @Nullable ActionType buildModuleType(String uid, Map<String, Set<ModuleInformation>> moduleInformation) {
        List<ConfigDescriptionParameter> configDescriptions = new ArrayList<>();
            ModuleInformation mi = (ModuleInformation) mis.toArray()[0];
            ActionModuleKind kind = ActionModuleKind.SINGLE;
            if (mi.getConfigName() != null && mi.getThingUID() != null) {
                        "ModuleType with UID {} has thingUID ({}) and multi-service ({}) property set, ignoring it.",
                        uid, mi.getConfigName(), mi.getThingUID());
            } else if (mi.getConfigName() != null) {
                kind = ActionModuleKind.SERVICE;
            } else if (mi.getThingUID() != null) {
                kind = ActionModuleKind.THING;
            ConfigDescriptionParameter configParam = buildConfigParam(mis, kind);
            if (configParam != null) {
                configDescriptions.add(configParam);
            Visibility visibility = mi.getVisibility();
            if (kind == ActionModuleKind.THING) {
                // we have a Thing module, so we have to map the inputs to config description parameters for the UI
                    List<ConfigDescriptionParameter> inputConfigDescriptions = actionInputsHelper
                            .mapActionInputsToConfigDescriptionParameters(mi.getInputs());
                    configDescriptions.addAll(inputConfigDescriptions);
                    // we have an input without a supported type, so hide the Thing action
                    visibility = Visibility.HIDDEN;
                    logger.debug("{} Thing action {} has an input with an unsupported type, hiding it in the UI.",
                            e.getMessage(), uid);
            return new ActionType(uid, configDescriptions, mi.getLabel(), mi.getDescription(), mi.getTags(), visibility,
                    mi.getInputs(), mi.getOutputs());
    private @Nullable ConfigDescriptionParameter buildConfigParam(Set<ModuleInformation> moduleInformations,
            ActionModuleKind kind) {
        if (kind == ActionModuleKind.SINGLE) {
            if (moduleInformations.size() == 1) {
                if (((ModuleInformation) moduleInformations.toArray()[0]).getConfigName() == null
                        && ((ModuleInformation) moduleInformations.toArray()[0]).getThingUID() == null) {
                    // we have a single service and thus no configuration at all
        } else if (kind == ActionModuleKind.SERVICE) {
                String configName = mi.getConfigName();
                options.add(new ParameterOption(configName, configName));
            return ConfigDescriptionParameterBuilder.create(CONFIG_PARAM, Type.TEXT).withLabel(SELECT_SERVICE_LABEL)
                    .withOptions(options).withLimitToOptions(true).withRequired(true).build();
        } else if (kind == ActionModuleKind.THING) {
                String thingUID = mi.getThingUID();
                options.add(new ParameterOption(thingUID, null));
            return ConfigDescriptionParameterBuilder.create(CONFIG_PARAM, Type.TEXT).withLabel(SELECT_THING_LABEL)
                    .withContext("thing").withOptions(options).withLimitToOptions(true).withRequired(true).build();
    public @Nullable ModuleInformation getModuleInformationForIdentifier(Action module,
            Map<String, Set<ModuleInformation>> moduleInformation, boolean isThing) {
        Configuration c = module.getConfiguration();
        String config = (String) c.get(AnnotationActionModuleTypeHelper.CONFIG_PARAM);
        Set<ModuleInformation> mis = moduleInformation.get(module.getTypeUID());
        ModuleInformation finalMI = null;
        if (mis.size() == 1 && config == null) {
            finalMI = (ModuleInformation) mis.toArray()[0];
            for (ModuleInformation mi : mis) {
                if (isThing) {
                    if (Objects.equals(mi.getThingUID(), config)) {
                        finalMI = mi;
                    if (Objects.equals(mi.getConfigName(), config)) {
        return finalMI;
