 * Tests for {@link BaseDynamicStateDescriptionProvider}.
class BaseDynamicStateDescriptionProviderTest {
    class TestBaseDynamicStateDescriptionProvider extends BaseDynamicStateDescriptionProvider {
        public TestBaseDynamicStateDescriptionProvider() {
    private @NonNullByDefault({}) BaseDynamicStateDescriptionProvider subject;
        subject = new TestBaseDynamicStateDescriptionProvider();
    public void setStatePatternPublishesEvent() {
        subject.setStatePattern(CHANNEL_UID, "%s");
        assertEquals(CommonChannelDescriptionField.PATTERN, cdce.getField());
    public void setStateOptionsPublishesEvent() {
        subject.setStateOptions(CHANNEL_UID, List.of(new StateOption("offline", "Offline")));
        assertEquals(CommonChannelDescriptionField.STATE_OPTIONS, cdce.getField());
