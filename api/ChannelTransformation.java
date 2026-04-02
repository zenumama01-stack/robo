 * The {@link ChannelTransformation} can be used to transform an input value using one or more transformations.
 * Individual transformations can be chained with <code>∩</code> and must follow the pattern
 * <code>serviceName:function</code> where <code>serviceName</code> refers to a {@link TransformationService} and
 * <code>function</code> has to be a valid transformation function for this service
public class ChannelTransformation {
    private final Logger logger = LoggerFactory.getLogger(ChannelTransformation.class);
    private List<TransformationStep> transformationSteps;
    public ChannelTransformation(@Nullable String transformationString) {
        if (transformationString != null) {
                transformationSteps = splitTransformationString(transformationString).map(TransformationStep::new)
                logger.warn("Transformation ignored, failed to parse {}: {}", transformationString, e.getMessage());
        transformationSteps = List.of();
    public ChannelTransformation(@Nullable List<String> transformationStrings) {
        if (transformationStrings != null) {
                transformationSteps = transformationStrings.stream() //
                        .filter(line -> !line.isBlank()) //
                        .filter(line -> !line.startsWith("#")) //
                        .filter(line -> !line.startsWith("//")) //
                        .flatMap(ChannelTransformation::splitTransformationString) //
                        .map(TransformationStep::new) //
                logger.warn("Transformation ignored, failed to parse {}: {}", transformationStrings, e.getMessage());
    private static Stream<String> splitTransformationString(String transformationString) {
        return Arrays.stream(transformationString.split("∩")).filter(s -> !s.isBlank());
     * Checks whether this object contains no transformation steps.
     * @return <code>true</code> if the transformation is empty, <code>false</code> otherwise.
        return transformationSteps.isEmpty();
     * Checks whether this object contains at least one transformation step.
     * @return <code>true</code> if the transformation is present, <code>false</code> otherwise.
    public boolean isPresent() {
        return !isEmpty();
     * Applies all transformations to the given value.
     * @param value the value to transform.
     * @return the transformed value or an empty optional if the transformation failed.
     *         If the transformation is empty, the original value is returned.
    public Optional<String> apply(String value) {
        Optional<String> valueOptional = Optional.of(value);
        // process all transformations
        for (TransformationStep transformationStep : transformationSteps) {
            valueOptional = valueOptional.flatMap(v -> {
                Optional<String> result = transformationStep.apply(v);
                    logger.trace("Transformed '{}' to '{}' using '{}'", v, result.orElse(null), transformationStep);
        return valueOptional;
     * Checks whether the given string contains valid transformations.
     * Valid single and chained transformations will return true.
     * @param value the transformation string to check.
     * @return <code>true</code> if the string contains valid transformations, <code>false</code> otherwise.
    public static boolean isValidTransformation(@Nullable String value) {
            return splitTransformationString(value).map(TransformationStep::new).count() > 0;
    private static class TransformationStep {
        private static final List<Pattern> TRANSFORMATION_PATTERNS = List.of( //
                Pattern.compile("(?<service>[a-zA-Z0-9]+)\\s*\\((?<function>.*)\\)$"), //
                Pattern.compile("(?<service>[a-zA-Z0-9]+)\\s*:(?<function>.*)") //
        private final Logger logger = LoggerFactory.getLogger(TransformationStep.class);
        private final String serviceName;
        private final String function;
        public TransformationStep(String pattern) throws IllegalArgumentException {
            pattern = pattern.trim();
            for (Pattern p : TRANSFORMATION_PATTERNS) {
                Matcher matcher = p.matcher(pattern);
                    this.serviceName = matcher.group("service").trim().toUpperCase();
                    this.function = matcher.group("function").trim();
                    "The transformation pattern must be in the syntax of TYPE:PATTERN or TYPE(PATTERN)");
            TransformationService service = TransformationHelper.getTransformationService(serviceName);
                    return Optional.ofNullable(service.transform(function, value));
                    if (e.getCause() instanceof ScriptException ex) {
                        logger.error("Applying {} failed: {}", this, ex.getMessage());
                        logger.debug("Applying {} failed: {}", this, e.getMessage());
                logger.warn("Failed to use {}, service not found", this);
            return "TransformationStep{serviceName='" + serviceName + "', function='" + function + "'}";
