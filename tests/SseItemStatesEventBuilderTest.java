 * The {@link SseItemStatesEventBuilderTest} contains tests for the method getDisplayState from
 * {@link SseItemStatesEventBuilder}
public class SseItemStatesEventBuilderTest {
    private static final String ITEM_NAME = "test";
    private static final String ITEM_STATE_VALUE = "value";
    private static final String ITEM_STATE_VALUE2 = "other";
    private static final int ITEM_STATE_VALUE3 = 16;
    private static final String ITEM_STATE_OPTION_LABEL = "The value";
    private static final String PATTERN = "__ %s __";
    private static final String PATTERN2 = "__ %d __";
    private static final String TRANSFORM_NAME = "TRANSFORM";
    private static final String TRANSFORM_PATTERN = "Pattern";
    private static final String TRANSFORM_FORMAT = "%s-1";
    private static final String TRANSFORM_INPUT = String.format(TRANSFORM_FORMAT, ITEM_STATE_VALUE);
    private static final String TRANSFORM_INPUT2 = String.format(TRANSFORM_FORMAT, ITEM_STATE_VALUE2);
    private static final String TRANSFORM_RESULT = "Result with string";
    private static final String TRANSFORM_FORMAT_NUMBER = "_%d_";
    private static final String TRANSFORM_INPUT3 = String.format(TRANSFORM_FORMAT_NUMBER, ITEM_STATE_VALUE3);
    private static final String TRANSFORM_RESULT_NUMBER = "Result with number";
    private static final String TRANSFORM_RESULT_NULL = "State is NULL";
    private static final String TRANSFORM_RESULT_UNDEF = "State is UNDEF";
    private @Mock @NonNullByDefault({}) ServiceReference<TransformationService> serviceRefMock;
    private @NonNullByDefault({}) TransformationHelper transformationHelper;
    private @NonNullByDefault({}) SseItemStatesEventBuilder sseItemStatesEventBuilder;
    public void init() throws TransformationException {
        Mockito.when(transformationServiceMock.transform(eq(TRANSFORM_PATTERN), eq(TRANSFORM_INPUT)))
                .thenAnswer(answer -> TRANSFORM_RESULT);
        Mockito.when(transformationServiceMock.transform(eq(TRANSFORM_PATTERN), eq(TRANSFORM_INPUT2)))
                .thenAnswer(answer -> null);
        Mockito.when(transformationServiceMock.transform(eq(TRANSFORM_PATTERN), eq(TRANSFORM_INPUT3)))
                .thenAnswer(answer -> TRANSFORM_RESULT_NUMBER);
        Mockito.when(transformationServiceMock.transform(eq(TRANSFORM_PATTERN), eq("NULL")))
                .thenAnswer(answer -> TRANSFORM_RESULT_NULL);
        Mockito.when(transformationServiceMock.transform(eq(TRANSFORM_PATTERN), eq("UNDEF")))
                .thenAnswer(answer -> TRANSFORM_RESULT_UNDEF);
        Mockito.when(serviceRefMock.getProperty(any())).thenReturn(TRANSFORM_NAME);
        Mockito.when(bundleContextMock.getService(serviceRefMock)).thenReturn(transformationServiceMock);
        transformationHelper = new TransformationHelper(bundleContextMock);
        transformationHelper.setTransformationService(serviceRefMock);
        Mockito.when(itemMock.getName()).thenReturn(ITEM_NAME);
        sseItemStatesEventBuilder = new SseItemStatesEventBuilder(itemRegistryMock, localeServiceMock,
                timeZoneProviderMock, startLevelServiceMock);
        transformationHelper.deactivate();
    public void getDisplayStateWhenMatchingStateOptionAndNoPattern() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create()
                .withOption(new StateOption(ITEM_STATE_VALUE, ITEM_STATE_OPTION_LABEL)).build().toStateDescription();
        Mockito.when(itemMock.getStateDescription(eq(Locale.ENGLISH))).thenReturn(stateDescription);
        Mockito.when(itemMock.getState()).thenReturn(new StringType(ITEM_STATE_VALUE));
        String result = sseItemStatesEventBuilder.getDisplayState(itemMock, Locale.ENGLISH);
        assertEquals(ITEM_STATE_OPTION_LABEL, result);
    public void getDisplayStateWhenNoMatchingStateOptionAndNoPattern() {
        Mockito.when(itemMock.getState()).thenReturn(new StringType(ITEM_STATE_VALUE2));
        assertEquals(ITEM_STATE_VALUE2, result);
    public void getDisplayStateWhenMatchingStateOptionAndPattern() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create().withPattern(PATTERN)
        assertEquals(String.format(PATTERN, ITEM_STATE_OPTION_LABEL), result);
    public void getDisplayStateWhenMatchingStateOptionAndWrongPattern() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create().withPattern(PATTERN2)
    public void getDisplayStateWhenNoMatchingStateOptionAndPattern() {
        assertEquals(String.format(PATTERN, ITEM_STATE_VALUE2), result);
    public void getDisplayStateWhenTransformAndNoStateOption() {
                .withPattern(TRANSFORM_NAME + "(" + TRANSFORM_PATTERN + "):" + TRANSFORM_FORMAT).build()
                .toStateDescription();
        assertEquals(TRANSFORM_RESULT, result);
        StateDescription stateDescription2 = StateDescriptionFragmentBuilder.create()
                .withPattern(TRANSFORM_NAME + "(" + TRANSFORM_PATTERN + "):" + TRANSFORM_FORMAT_NUMBER).build()
        Mockito.when(itemMock.getStateDescription(eq(Locale.ENGLISH))).thenReturn(stateDescription2);
        Mockito.when(itemMock.getState()).thenReturn(new DecimalType(ITEM_STATE_VALUE3));
        result = sseItemStatesEventBuilder.getDisplayState(itemMock, Locale.ENGLISH);
        assertEquals(TRANSFORM_RESULT_NUMBER, result);
    public void getDisplayStateWhenTransformAndMatchingStateOption() {
                .withPattern(TRANSFORM_NAME + "(" + TRANSFORM_PATTERN + "):" + TRANSFORM_FORMAT)
    public void getDisplayStateWhenTransformReturningNull() {
    public void getDisplayStateWhenStateUndef() {
        Mockito.when(itemMock.getState()).thenReturn(UnDefType.UNDEF);
        assertEquals("UNDEF", result);
    public void getDisplayStateWhenStateNull() {
        Mockito.when(itemMock.getState()).thenReturn(UnDefType.NULL);
        assertEquals("NULL", result);
    public void getDisplayStateWhenTransformAndStateUndef() {
        assertEquals(TRANSFORM_RESULT_UNDEF, result);
    public void getDisplayStateWhenTransformAndStateNull() {
        assertEquals(TRANSFORM_RESULT_NULL, result);
    public void getDisplayStateWhenPatternProvided() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create().withPattern(PATTERN).build()
        assertEquals(String.format(PATTERN, ITEM_STATE_VALUE), result);
        StateDescription stateDescription2 = StateDescriptionFragmentBuilder.create().withPattern(PATTERN2).build()
        assertEquals(String.format(PATTERN2, ITEM_STATE_VALUE3), result);
    public void getDisplayStateWhenWrongPatternProvided() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create().withPattern(PATTERN2).build()
        assertEquals(ITEM_STATE_VALUE, result);
    public void getDisplayStateWhenNoPatternProvided() {
        StateDescription stateDescription = StateDescriptionFragmentBuilder.create().build().toStateDescription();
        assertEquals(String.format("%d", ITEM_STATE_VALUE3), result);
    public void getDisplayStateWhenNoStateDescription() {
        Mockito.when(itemMock.getStateDescription(eq(Locale.ENGLISH))).thenReturn(null);
