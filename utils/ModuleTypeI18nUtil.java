 * This class is used as utility for resolving the localized {@link org.openhab.core.automation.type.ModuleType
 * ModuleTypes}s.
 * It automatically infers the key if the default text is not a constant with the assistance of
 * {@link TranslationProvider}.
public class ModuleTypeI18nUtil {
    public static final String MODULE_TYPE = "module-type";
    public ModuleTypeI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getLocalizedModuleTypeLabel(Bundle bundle, String moduleTypeUID,
            @Nullable String defaultLabel, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferModuleTypeKey(moduleTypeUID, "label"));
    public @Nullable String getLocalizedModuleTypeDescription(Bundle bundle, String moduleTypeUID,
            @Nullable String defaultDescription, @Nullable Locale locale) {
                () -> inferModuleTypeKey(moduleTypeUID, "description"));
    public List<Input> getLocalizedInputs(@Nullable List<Input> inputs, Bundle bundle, String uid,
        List<Input> linputs = new ArrayList<>();
        if (inputs != null) {
                String inputName = input.getName();
                String ilabel = getInputLabel(bundle, uid, inputName, input.getLabel(), locale);
                String idescription = getInputDescription(bundle, uid, inputName, input.getDescription(), locale);
                linputs.add(new Input(inputName, input.getType(), ilabel, idescription, input.getTags(),
                        input.isRequired(), input.getReference(), input.getDefaultValue()));
        return linputs;
    public List<Output> getLocalizedOutputs(@Nullable List<Output> outputs, Bundle bundle, String uid,
        List<Output> loutputs = new ArrayList<>();
                String outputName = output.getName();
                String olabel = getOutputLabel(bundle, uid, outputName, output.getLabel(), locale);
                String odescription = getOutputDescription(bundle, uid, outputName, output.getDescription(), locale);
                loutputs.add(new Output(outputName, output.getType(), olabel, odescription, output.getTags(),
                        output.getReference(), output.getDefaultValue()));
        return loutputs;
    private @Nullable String getInputLabel(Bundle bundle, String moduleTypeUID, String inputName,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferInputKey(moduleTypeUID, inputName, "label"));
    private @Nullable String getInputDescription(Bundle bundle, String moduleTypeUID, String inputName,
                () -> inferInputKey(moduleTypeUID, inputName, "description"));
    private @Nullable String getOutputLabel(Bundle bundle, String ruleTemplateUID, String outputName,
            String defaultLabel, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferOutputKey(ruleTemplateUID, outputName, "label"));
    private @Nullable String getOutputDescription(Bundle bundle, String moduleTypeUID, String outputName,
            String defaultDescription, @Nullable Locale locale) {
                () -> inferOutputKey(moduleTypeUID, outputName, "description"));
    private String inferModuleTypeKey(String moduleTypeUID, String lastSegment) {
        return MODULE_TYPE + "." + moduleTypeUID + "." + lastSegment;
    private String inferInputKey(String moduleTypeUID, String inputName, String lastSegment) {
        return MODULE_TYPE + "." + moduleTypeUID + ".input." + inputName + "." + lastSegment;
    private String inferOutputKey(String moduleTypeUID, String outputName, String lastSegment) {
        return MODULE_TYPE + "." + moduleTypeUID + ".output." + outputName + "." + lastSegment;
