public class TransformationHelper {
    private static final Map<String, TransformationService> SERVICES = new ConcurrentHashMap<>();
    private static final Logger LOGGER = LoggerFactory.getLogger(TransformationHelper.class);
    public static final String FUNCTION_VALUE_DELIMITER = ":";
    /* RegEx to extract and parse a function String <code>'(.*?)\((.*)\):(.*)'</code> */
    protected static final Pattern EXTRACT_TRANSFORMFUNCTION_PATTERN = Pattern
            .compile("(.*?)\\((.*)\\)" + FUNCTION_VALUE_DELIMITER + "(.*)");
    public TransformationHelper(BundleContext bundleContext) {
        SERVICES.clear();
    public void setTransformationService(ServiceReference<TransformationService> ref) {
        String key = (String) ref.getProperty(TransformationService.SERVICE_PROPERTY_NAME);
        TransformationService service = bundleContext.getService(ref);
            SERVICES.put(key, service);
            LOGGER.debug("Added transformation service {}", key);
    public void unsetTransformationService(ServiceReference<TransformationService> ref) {
        if (SERVICES.remove(key) != null) {
            LOGGER.debug("Removed transformation service {}", key);
     * determines whether a pattern refers to a transformation service
     * @param pattern the pattern to check
     * @return true, if the pattern contains a transformation
    public static boolean isTransform(String pattern) {
        return EXTRACT_TRANSFORMFUNCTION_PATTERN.matcher(pattern).matches();
    public static @Nullable TransformationService getTransformationService(String serviceName) {
        return SERVICES.get(serviceName);
     * Return the transformation service that provides a given transformation type (e.g. REGEX, XSLT, etc.)
     * @param context the bundle context which can be null
     * @param transformationType the desired transformation type
     * @return a service instance or null, if none could be found
     * @deprecated use {@link #getTransformationService(String)} instead
    public static @Nullable TransformationService getTransformationService(@Nullable BundleContext context,
            String transformationType) {
        return getTransformationService(transformationType);
     * Transforms a state string using transformation functions within a given pattern.
     * @param context a valid bundle context, required for accessing the services
     * @param transformationString the pattern that contains the transformation instructions
     * @param state the state to be formatted before being passed into the transformation function
     * @return the result of the transformation. If no transformation was done, <code>null</code> is returned
     * @throws TransformationException if transformation service is not available or the transformation failed
     * @deprecated Use {@link #transform(String, String)} instead
    public static @Nullable String transform(BundleContext context, String transformationString, String state)
        return transform(transformationString, state);
    public static @Nullable String transform(String transformationString, String state) throws TransformationException {
        Matcher matcher = EXTRACT_TRANSFORMFUNCTION_PATTERN.matcher(transformationString);
            String pattern = matcher.group(2);
            TransformationService transformation = SERVICES.get(type);
                return transform(transformation, pattern, value, state);
                throw new TransformationException("Couldn't transform value because transformation service of type '"
                        + type + "' is not available.");
     * Transforms a state string using a transformation service
     * @param service the {@link TransformationService} to be used
     * @param function the function containing the transformation instruction
     * @param format the format the state should be converted to before transformation
     * @throws TransformationException if transformation service fails or the state cannot be formatted according to the
     *             format
    public static @Nullable String transform(TransformationService service, String function, String format,
            String state) throws TransformationException {
            String value = String.format(format, state);
            return service.transform(function, value);
            throw new TransformationException("Cannot format state '" + state + "' to format '" + format + "'", e);
            throw new TransformationException("Transformation service threw an exception: " + e.getMessage(), e);
