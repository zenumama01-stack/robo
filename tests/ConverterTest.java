 * The {@link ConverterTest} is a test class for state converters
public class ConverterTest {
    private @Mock @NonNullByDefault({}) Consumer<String> sendValueMock;
    private @Mock @NonNullByDefault({}) Consumer<State> updateStateMock;
    private @Mock @NonNullByDefault({}) Consumer<Command> postCommandMock;
    public void numberItemConverter() {
        NumberChannelHandler converter = new NumberChannelHandler(updateStateMock, postCommandMock, sendValueMock,
                new ChannelTransformation((String) null), new ChannelTransformation((String) null),
                new ChannelValueConverterConfig());
        // without unit
        Assertions.assertEquals(Optional.of(new DecimalType(1234)), converter.toState("1234"));
        // unit in transformation result
        Assertions.assertEquals(Optional.of(new QuantityType<>(100, SIUnits.CELSIUS)), converter.toState("100°C"));
        // no valid value
        Assertions.assertEquals(Optional.of(UnDefType.UNDEF), converter.toState("W"));
        Assertions.assertEquals(Optional.of(UnDefType.UNDEF), converter.toState(""));
    public void numberItemConverterWithUnit() {
        ChannelValueConverterConfig channelConfig = new ChannelValueConverterConfig();
        channelConfig.unit = "W";
                new ChannelTransformation((String) null), new ChannelTransformation((String) null), channelConfig);
        Assertions.assertEquals(Optional.of(new QuantityType<>(500, Units.WATT)), converter.toState("500"));
        Assertions.assertEquals(Optional.of(UnDefType.UNDEF), converter.toState("foo"));
    public void stringTypeConverter() {
        GenericChannelHandler converter = createConverter(StringType::new);
        Assertions.assertEquals(Optional.of(new StringType("Test")), converter.toState("Test"));
    public void decimalTypeConverter() {
        GenericChannelHandler converter = createConverter(DecimalType::new);
        Assertions.assertEquals(Optional.of(new DecimalType(15.6)), converter.toState("15.6"));
    public void pointTypeConverter() {
        GenericChannelHandler converter = createConverter(PointType::new);
        Assertions.assertEquals(
                Optional.of(new PointType(new DecimalType(51.1), new DecimalType(7.2), new DecimalType(100))),
                converter.toState("51.1, 7.2, 100"));
    public void playerItemTypeConverter() {
        ChannelValueConverterConfig cfg = new ChannelValueConverterConfig();
        cfg.playValue = "PLAY";
        ChannelHandlerContent content = new ChannelHandlerContent("PLAY".getBytes(StandardCharsets.UTF_8), "UTF-8",
        PlayerChannelHandler converter = new PlayerChannelHandler(updateStateMock, postCommandMock, sendValueMock,
                new ChannelTransformation((String) null), new ChannelTransformation((String) null), cfg);
        converter.process(content);
        Mockito.verify(postCommandMock).accept(PlayPauseType.PLAY);
        Mockito.verify(updateStateMock, Mockito.never()).accept(ArgumentMatchers.any());
    public void colorItemTypeRGBConverter() {
        cfg.colorMode = ColorChannelHandler.ColorMode.RGB;
        ChannelHandlerContent content = new ChannelHandlerContent("123,34,47".getBytes(StandardCharsets.UTF_8), "UTF-8",
        ColorChannelHandler converter = new ColorChannelHandler(updateStateMock, postCommandMock, sendValueMock,
        Mockito.verify(updateStateMock).accept(HSBType.fromRGB(123, 34, 47));
    public void colorItemTypeHSBConverter() {
        cfg.colorMode = ColorChannelHandler.ColorMode.HSB;
        Mockito.verify(updateStateMock).accept(new HSBType("123,34,47"));
    public void rollerSHutterConverter() {
        RollershutterChannelHandler converter = new RollershutterChannelHandler(updateStateMock, postCommandMock,
                sendValueMock, new ChannelTransformation((String) null), new ChannelTransformation((String) null), cfg);
        // test 0 and 100
        ChannelHandlerContent content = new ChannelHandlerContent("0".getBytes(StandardCharsets.UTF_8), "UTF-8", null);
        Mockito.verify(updateStateMock).accept(PercentType.ZERO);
        content = new ChannelHandlerContent("100".getBytes(StandardCharsets.UTF_8), "UTF-8", null);
        Mockito.verify(updateStateMock).accept(PercentType.HUNDRED);
        // test under/over-range (expect two times total for zero/100
        content = new ChannelHandlerContent("-1".getBytes(StandardCharsets.UTF_8), "UTF-8", null);
        Mockito.verify(updateStateMock, Mockito.times(2)).accept(PercentType.ZERO);
        content = new ChannelHandlerContent("105".getBytes(StandardCharsets.UTF_8), "UTF-8", null);
        Mockito.verify(updateStateMock, Mockito.times(2)).accept(PercentType.HUNDRED);
        // test value
        content = new ChannelHandlerContent("67".getBytes(StandardCharsets.UTF_8), "UTF-8", null);
        Mockito.verify(updateStateMock).accept(new PercentType(67));
    public GenericChannelHandler createConverter(Function<String, State> fcn) {
        return new GenericChannelHandler(fcn, updateStateMock, postCommandMock, sendValueMock,
