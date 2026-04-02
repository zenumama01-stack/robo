 * The {@link ChannelTransformationTest} contains tests for the {@link ChannelTransformation}
public class ChannelTransformationTest {
    private static final String T1_NAME = "TRANSFORM1";
    private static final String T1_PATTERN = "T1Pattern";
    private static final String T1_INPUT = "T1Input";
    private static final String T1_RESULT = "T1Result";
    private static final String T2_NAME = "TRANSFORM2";
    private static final String T2_PATTERN = "T2Pattern";
    private static final String T2_INPUT = T1_RESULT;
    private static final String T2_RESULT = "T2Result";
    private static final String T3_NAME = T2_NAME;
    private static final String T3_PATTERN = "a()b()))";
    private static final String T3_INPUT = T2_RESULT;
    private static final String T3_RESULT = "T3Result";
    private static final String NULL_NAME = T1_NAME;
    private static final String NULL_PATTERN = "NullPattern";
    private static final String NULL_INPUT = T1_RESULT;
    private static final @Nullable String NULL_RESULT = null;
    private @Mock @NonNullByDefault({}) TransformationService transformationService1Mock;
    private @Mock @NonNullByDefault({}) TransformationService transformationService2Mock;
    private @Mock @NonNullByDefault({}) ServiceReference<TransformationService> serviceRef1Mock;
    private @Mock @NonNullByDefault({}) ServiceReference<TransformationService> serviceRef2Mock;
        Mockito.when(transformationService1Mock.transform(eq(T1_PATTERN), eq(T1_INPUT)))
                .thenAnswer(answer -> T1_RESULT);
        Mockito.when(transformationService2Mock.transform(eq(T2_PATTERN), eq(T1_INPUT)))
                .thenAnswer(answer -> T2_RESULT);
        Mockito.when(transformationService2Mock.transform(eq(T2_PATTERN), eq(T2_INPUT)))
        Mockito.when(transformationService2Mock.transform(eq(T3_PATTERN), eq(T3_INPUT)))
                .thenAnswer(answer -> T3_RESULT);
        Mockito.when(transformationService1Mock.transform(eq(NULL_PATTERN), eq(NULL_INPUT)))
                .thenAnswer(answer -> NULL_RESULT);
        Mockito.when(serviceRef1Mock.getProperty(any())).thenReturn("TRANSFORM1");
        Mockito.when(serviceRef2Mock.getProperty(any())).thenReturn("TRANSFORM2");
        Mockito.when(bundleContextMock.getService(serviceRef1Mock)).thenReturn(transformationService1Mock);
        Mockito.when(bundleContextMock.getService(serviceRef2Mock)).thenReturn(transformationService2Mock);
        transformationHelper.setTransformationService(serviceRef1Mock);
        transformationHelper.setTransformationService(serviceRef2Mock);
    public void testMissingTransformation() {
        String pattern = "TRANSFORM:pattern";
        ChannelTransformation transformation = new ChannelTransformation(pattern);
        String result = transformation.apply(T1_INPUT).orElse(null);
    public void testNullReturningTransformation() {
        String pattern = NULL_NAME + ":" + NULL_PATTERN;
        Optional<String> result = transformation.apply(NULL_INPUT);
        assertTrue(result.isEmpty());
    public void testSingleTransformationWithColon() {
        String pattern = T1_NAME + ":" + T1_PATTERN;
        assertEquals(T1_RESULT, result);
    public void testSingleTransformationWithParens() {
        String pattern = T1_NAME + "(" + T1_PATTERN + ")";
    public void testParensTransformationWithNestedParensInPattern() {
        String pattern = T3_NAME + "(" + T3_PATTERN + ")";
        String result = transformation.apply(T3_INPUT).orElse(null);
        assertEquals(T3_RESULT, result);
    public void testInvalidFirstTransformation() {
        String pattern = T1_NAME + "X:" + T1_PATTERN + "∩" + T2_NAME + ":" + T2_PATTERN;
    public void testInvalidSecondTransformation() {
        String pattern = T1_NAME + ":" + T1_PATTERN + "∩" + T2_NAME + "X:" + T2_PATTERN;
    public void testFirstTransformationReturningNull() {
        List<String> pattern = List.of(NULL_NAME + ":" + NULL_PATTERN, T2_NAME + ":" + T2_PATTERN);
    public void testSecondTransformationReturningNull() {
        List<String> pattern = List.of(T1_NAME + ":" + T1_PATTERN, NULL_NAME + ":" + NULL_PATTERN);
        Optional<String> result = transformation.apply(T1_INPUT);
    public void testColonDoubleTransformationWithoutSpaces() {
        String pattern = T1_NAME + ":" + T1_PATTERN + "∩" + T2_NAME + ":" + T2_PATTERN;
        assertEquals(T2_RESULT, result);
    public void testTransformationsInAList() {
        List<String> patterns = List.of(T1_NAME + ":" + T1_PATTERN, T2_NAME + ":" + T2_PATTERN);
        ChannelTransformation transformation = new ChannelTransformation(patterns);
    public void testMixedTransformationsInAList1() {
        List<String> patterns = List.of(T1_NAME + ":" + T1_PATTERN + "∩" + T2_NAME + ":" + T2_PATTERN,
                T3_NAME + ":" + T3_PATTERN);
    public void testMixedTransformationsInAList2() {
        List<String> patterns = List.of(T1_NAME + ":" + T1_PATTERN,
                T2_NAME + ":" + T2_PATTERN + "∩" + T3_NAME + ":" + T3_PATTERN);
    public void testParensDoubleTransformationWithoutSpaces() {
        String pattern = T1_NAME + "(" + T1_PATTERN + ")∩" + T2_NAME + "(" + T2_PATTERN + ")";
    public void testMixedDoubleTransformationWithoutSpaces1() {
        String pattern = T1_NAME + ":" + T1_PATTERN + "∩" + T2_NAME + "(" + T2_PATTERN + ")";
    public void testMixedDoubleTransformationWithoutSpaces2() {
        String pattern = T1_NAME + "(" + T1_PATTERN + ")∩" + T2_NAME + ":" + T2_PATTERN;
    public void testColonDoubleTransformationWithSpaces() {
        String pattern = " " + T1_NAME + " : " + T1_PATTERN + " ∩ " + T2_NAME + " : " + T2_PATTERN + " ";
    public void testParensDoubleTransformationWithSpaces() {
        String pattern = " " + T1_NAME + " ( " + T1_PATTERN + " ) ∩ " + T2_NAME + " ( " + T2_PATTERN + " ) ";
    public void testMixedDoubleTransformationWithSpaces1() {
        String pattern = " " + T1_NAME + " : " + T1_PATTERN + " ∩ " + T2_NAME + " ( " + T2_PATTERN + " ) ";
    public void testMixedDoubleTransformationWithSpaces2() {
        String pattern = " " + T1_NAME + " ( " + T1_PATTERN + " ) ∩ " + T2_NAME + " : " + T2_PATTERN + " ";
    public void testIsValidTransform() {
        // single with colon
        assertTrue(ChannelTransformation.isValidTransformation("FOO:BAR"));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO : BAR "));
        // single with parens
        assertTrue(ChannelTransformation.isValidTransformation("FOO(BAR())"));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO ( BAR) )")); // deliberate extra closing parens
        // chained with colon
        assertTrue(ChannelTransformation.isValidTransformation("FOO:BAR∩BAZ:QUX"));
        assertTrue(ChannelTransformation.isValidTransformation("FOO:BAR∩BAZ:QUX()"));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO : BAR ∩ BAZ : QUX() "));
        // chained with parens
        assertTrue(ChannelTransformation.isValidTransformation("FOO(BAR)∩BAZ(QUX)"));
        assertTrue(ChannelTransformation.isValidTransformation("FOO(BAR)∩BAZ(QUX())"));
        assertTrue(ChannelTransformation.isValidTransformation("FOO(BAR)∩BAZ(QUX))")); // deliberate extra parens
        assertTrue(ChannelTransformation.isValidTransformation(" FOO ( BAR ) ∩ BAZ ( QUX )"));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO ( BAR ) ∩ BAZ ( QUX() )"));
        // mixed chains
        assertTrue(ChannelTransformation.isValidTransformation("FOO:BAR∩BAZ(QUX)"));
        assertTrue(ChannelTransformation.isValidTransformation("FOO(BAR)∩BAZ:QUX"));
        assertTrue(ChannelTransformation.isValidTransformation("FOO:BAR()∩BAZ(QUX())"));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO : BAR ∩ BAZ ( QUX ) "));
        assertTrue(ChannelTransformation.isValidTransformation(" FOO ( BAR ) ∩ BAZ : QUX "));
        // invalid syntaxes
        assertFalse(ChannelTransformation.isValidTransformation(null));
        assertFalse(ChannelTransformation.isValidTransformation(""));
        assertFalse(ChannelTransformation.isValidTransformation(" "));
        assertFalse(ChannelTransformation.isValidTransformation("FOOBAR"));
        assertFalse(ChannelTransformation.isValidTransformation("(FOO)BAR"));
        assertFalse(ChannelTransformation.isValidTransformation("FOO∩BAR"));
        assertFalse(ChannelTransformation.isValidTransformation("FOO:BAR∩BAZ"));
        assertFalse(ChannelTransformation.isValidTransformation("FOO(BAR)∩BAZ"));
        assertFalse(ChannelTransformation.isValidTransformation("FOO∩BAZ:BAR"));
        assertFalse(ChannelTransformation.isValidTransformation("FOO∩BAZ(BAR)"));
    public void testBlanksAndCommentsAreDiscarded() {
        List<String> pattern = List.of("#hash comment", "//double slashes", "  # preceded by spaces",
                "  // ditto for slashes", "     ", "\t", "\t\t\t", "\t# preceded by a tab",
                "\t // preceded by tab and space");
        assertTrue(transformation.isEmpty());
