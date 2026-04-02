public class ToggleProfileTest extends JavaTest {
    private @Mock @NonNullByDefault({}) SystemProfileFactory systemProfileFactory;
    public static Stream<ProfileTypeUID> getAllToggleButtonSwitchProfiles() {
        return Stream.of(RAWBUTTON_TOGGLE_SWITCH, BUTTON_TOGGLE_SWITCH);
    @MethodSource("getAllToggleButtonSwitchProfiles")
    public void testSwitchItem(ProfileTypeUID profileUID) {
        prepareContextMock();
        TriggerProfile profile = newToggleProfile(profileUID);
        verifyAction(profile, UnDefType.NULL, OnOffType.ON);
        verifyAction(profile, OnOffType.ON, OnOffType.OFF);
        verifyAction(profile, OnOffType.OFF, OnOffType.ON);
    public void testDimmerItem(ProfileTypeUID profileUID) {
        verifyAction(profile, PercentType.HUNDRED, OnOffType.OFF);
        verifyAction(profile, PercentType.ZERO, OnOffType.ON);
        verifyAction(profile, new PercentType(50), OnOffType.OFF);
    public void testColorItem(ProfileTypeUID profileUID) {
        verifyAction(profile, HSBType.WHITE, OnOffType.OFF);
        verifyAction(profile, HSBType.BLACK, OnOffType.ON);
        verifyAction(profile, new HSBType("0,50,50"), OnOffType.OFF);
    public void testRollershutterItem() {
        TriggerProfile profile = newToggleProfile(RAWBUTTON_TOGGLE_ROLLERSHUTTER);
        verifyAction(profile, UnDefType.NULL, UpDownType.UP);
        verifyAction(profile, UpDownType.UP, UpDownType.DOWN);
        verifyAction(profile, UpDownType.DOWN, UpDownType.UP);
        profile = newToggleProfile(BUTTON_TOGGLE_ROLLERSHUTTER);
    public void testPlayerItem() {
        TriggerProfile profile = newToggleProfile(RAWBUTTON_TOGGLE_PLAYER);
        verifyAction(profile, UnDefType.NULL, PlayPauseType.PLAY);
        verifyAction(profile, PlayPauseType.PLAY, PlayPauseType.PAUSE);
        verifyAction(profile, PlayPauseType.PAUSE, PlayPauseType.PLAY);
        profile = newToggleProfile(BUTTON_TOGGLE_PLAYER);
    public void testCorrectUserConfiguredEvent() {
        setupInterceptedLogger(ToggleProfile.class, LogLevel.WARN);
        initializeContextMock(CommonTriggerEvents.RELEASED);
        TriggerProfile profile = newToggleProfile(RAWBUTTON_TOGGLE_SWITCH);
        stopInterceptedLogger(ToggleProfile.class);
        assertNoLogMessage(ToggleProfile.class);
        verifyAction(profile, UnDefType.NULL, OnOffType.ON, CommonTriggerEvents.RELEASED);
        verifyAction(profile, OnOffType.ON, OnOffType.OFF, CommonTriggerEvents.RELEASED);
        verifyAction(profile, OnOffType.OFF, OnOffType.ON, CommonTriggerEvents.RELEASED);
    public void testWrongUserConfiguredEvent() {
        initializeContextMock(CommonTriggerEvents.SHORT_PRESSED);
        assertLogMessage(ToggleProfile.class, LogLevel.WARN,
                "'" + CommonTriggerEvents.SHORT_PRESSED + "' is not a valid trigger event for Profile '"
                        + profile.getProfileTypeUID().getAsString() + "'. Default trigger event '"
                        + CommonTriggerEvents.PRESSED + "' is used instead.");
    private void initializeContextMock(@Nullable String triggerEvent) {
        Map<String, Object> params = triggerEvent == null ? Map.of() : Map.of(ToggleProfile.EVENT_PARAM, triggerEvent);
        when(contextMock.getConfiguration()).thenReturn(new Configuration(params));
    private void prepareContextMock() {
        initializeContextMock(null);
    private TriggerProfile newToggleProfile(ProfileTypeUID profileUID) {
        when(systemProfileFactory.createProfile(profileUID, callbackMock, contextMock)).thenCallRealMethod();
        TriggerProfile profile = (TriggerProfile) systemProfileFactory.createProfile(profileUID, callbackMock,
                contextMock);
        assertNotNull(profile);
    private void verifyAction(TriggerProfile profile, State preCondition, Command expectation, String triggerEvent) {
        profile.onStateUpdateFromItem(preCondition);
        profile.onTriggerFromHandler(triggerEvent);
    private void verifyAction(TriggerProfile profile, State preCondition, Command expectation) {
        verifyAction(profile, preCondition, expectation,
                profile.getProfileTypeUID().getAsString().contains("rawbutton") ? CommonTriggerEvents.PRESSED
                        : CommonTriggerEvents.SHORT_PRESSED);
