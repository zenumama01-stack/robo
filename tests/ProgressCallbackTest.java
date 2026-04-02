import org.openhab.core.thing.firmware.FirmwareUpdateProgressInfoEvent;
import org.openhab.core.thing.firmware.FirmwareUpdateResultInfoEvent;
 * Testing the {@link ProgressCallback}.
 * @author Dimitar Ivanov - Adapted the tests to use firmware instead of firmware UID
public final class ProgressCallbackTest {
    private static final String BINDING_ID = "simpleBinding";
    private static final String THING_TYPE_ID1 = "simpleThingType1";
    private static final ThingTypeUID THING_TYPE_UID1 = new ThingTypeUID(BINDING_ID, THING_TYPE_ID1);
    private static final String THING1_ID = "simpleThing1";
    private static final String CANCEL_MESSAGE_KEY = "update-canceled";
    private static final ThingTypeUID THING_TYPE = new ThingTypeUID("thing:type");
    private static final ThingUID EXPECTED_THING_UID = new ThingUID(THING_TYPE, "thingid");
    private @NonNullByDefault({}) ProgressCallbackImpl sut;
    private @NonNullByDefault({}) Firmware expectedFirmware;
    private final List<Event> postedEvents = new LinkedList<>();
    private @Nullable String usedMessagedKey;
        expectedFirmware = FirmwareBuilder.create(THING_TYPE, "1").build();
        postedEvents.clear();
        EventPublisher publisher = new EventPublisher() {
            public void post(Event event) throws IllegalArgumentException, IllegalStateException {
                postedEvents.add(event);
        TranslationProvider i18nProvider = new TranslationProvider() {
                usedMessagedKey = key;
                return "Dummy Message";
        when(bundle.getSymbolicName()).thenReturn("");
        sut = new ProgressCallbackImpl(new DummyFirmwareHandler(), publisher, i18nProvider, bundleResolver,
                EXPECTED_THING_UID, expectedFirmware, null);
    public void assertThatUpdateThrowsIllegalStateExceptionIfUpdateIsFinished() {
        sut.defineSequence(ProgressStep.DOWNLOADING);
        sut.next();
        sut.success();
        assertThatUpdateResultEventIsValid(postedEvents.get(1), null, FirmwareUpdateResult.SUCCESS);
        assertThrows(IllegalStateException.class, () -> sut.update(100));
    public void assertThatDefineSequenceThrowsIllegalArguumentExceptionIfSequenceIsEmpty() {
        assertThrows(IllegalArgumentException.class, () -> sut.defineSequence());
    public void assertThatSuccessThrowsIllegalStateExceptionIfProgressIsNotAt100Percent() {
        sut.update(99);
        assertThrows(IllegalStateException.class, () -> sut.success());
    public void assertThatSuccessThrowsIllegalStateExceptionIfLastProgressStepIsNotReached() {
        sut.defineSequence(ProgressStep.DOWNLOADING, ProgressStep.TRANSFERRING);
    public void assertSuccessAt100PercentEvenIfThereIsARemainingProgressStep() {
        sut.update(100);
    public void assertThatUpdateThrowsIllegalArgumentExceptionIfProgressSmaller0() {
        assertThrows(IllegalArgumentException.class, () -> sut.update(-1));
    public void assertThatUpdateThrowsIllegalArgumentExceptionIfProgressGreater100() {
        assertThrows(IllegalArgumentException.class, () -> sut.update(101));
    public void assertThatUpdateThrowsIllegalArgumentExceptionIfNewProgressIsSmallerThanOldProgress() {
        sut.update(10);
        assertThrows(IllegalArgumentException.class, () -> sut.update(9));
    public void assertThatDefiningASequenceIsOptionalIfPercentageProgressIsUsed() {
        sut.update(5);
        assertThatProgressInfoEventIsValid(postedEvents.get(0), null, false, 5);
        sut.update(11);
        assertThatProgressInfoEventIsValid(postedEvents.get(1), null, false, 11);
        sut.update(22);
        assertThatProgressInfoEventIsValid(postedEvents.get(2), null, false, 22);
        sut.update(44);
        assertThatProgressInfoEventIsValid(postedEvents.get(3), null, false, 44);
        sut.pending();
        assertThatProgressInfoEventIsValid(postedEvents.get(4), null, true, 44);
        sut.update(50);
        assertThatProgressInfoEventIsValid(postedEvents.get(5), null, false, 50);
        sut.update(70);
        assertThatProgressInfoEventIsValid(postedEvents.get(6), null, false, 70);
        sut.update(89);
        assertThatProgressInfoEventIsValid(postedEvents.get(7), null, false, 89);
        assertThatProgressInfoEventIsValid(postedEvents.get(8), null, false, 100);
        assertThatUpdateResultEventIsValid(postedEvents.get(9), null, FirmwareUpdateResult.SUCCESS);
        assertThat(postedEvents.size(), is(10));
    public void assertThatUpdateResultsNotInFirmwareUpdateProgressInfoEventIfProgressNotChanged() {
        sut.defineSequence(ProgressStep.UPDATING);
        assertThatProgressInfoEventIsValid(postedEvents.get(0), ProgressStep.UPDATING, false, 10);
        assertThat(postedEvents.size(), is(1));
    public void assertThatSettingTheProgressToPendingResultsInAFirmwareUpdateProgressInfoEvent() {
        assertThatProgressInfoEventIsValid(postedEvents.get(0), ProgressStep.DOWNLOADING, true, null);
    public void assertThatPendingDoesNotChangeProgressStep() {
        assertThatProgressInfoEventIsValid(postedEvents.get(1), ProgressStep.DOWNLOADING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(2), ProgressStep.DOWNLOADING, true, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(3), ProgressStep.DOWNLOADING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(4), ProgressStep.TRANSFERRING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(5), ProgressStep.TRANSFERRING, true, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(6), ProgressStep.TRANSFERRING, false, null);
        assertThatUpdateResultEventIsValid(postedEvents.get(7), null, FirmwareUpdateResult.SUCCESS);
        assertThat(postedEvents.size(), is(8));
    public void assertThatNextChangesProgressStepIfNotPending() {
        sut.defineSequence(ProgressStep.DOWNLOADING, ProgressStep.TRANSFERRING, ProgressStep.UPDATING);
        assertThatProgressInfoEventIsValid(postedEvents.get(0), ProgressStep.DOWNLOADING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(1), ProgressStep.TRANSFERRING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(2), ProgressStep.TRANSFERRING, true, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(3), ProgressStep.TRANSFERRING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(4), ProgressStep.UPDATING, false, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(5), ProgressStep.UPDATING, true, null);
        assertThatProgressInfoEventIsValid(postedEvents.get(6), ProgressStep.UPDATING, false, null);
    public void assertThatCancelThrowsIllegalStateExceptionIfUpdateIsFinished() {
        assertThatUpdateResultEventIsValid(postedEvents.get(2), null, FirmwareUpdateResult.SUCCESS);
        assertThrows(IllegalStateException.class, () -> sut.canceled());
    public void assertThatCallingCancelResultsInAFirmwareUpdateResultInfoEvent() {
        sut.canceled();
        assertThat(postedEvents.get(0), is(instanceOf(FirmwareUpdateResultInfoEvent.class)));
        FirmwareUpdateResultInfoEvent resultEvent = (FirmwareUpdateResultInfoEvent) postedEvents.get(0);
        assertThat(resultEvent.getFirmwareUpdateResultInfo().getThingUID(), is(EXPECTED_THING_UID));
        assertThat(resultEvent.getFirmwareUpdateResultInfo().getResult(), is(FirmwareUpdateResult.CANCELED));
        assertThat(usedMessagedKey, is(CANCEL_MESSAGE_KEY));
     * Special behavior because of pending state:
     * Before calling next the ProgressStep is null which means the update was not started
     * but a valid ProgressStep is needed to create a FirmwareUpdateProgressInfoEvent.
     * As workaround the first step is returned to provide a valid ProgressStep.
     * This could be the case if the update directly goes in PENDING state after trying to start it.
    public void assertThatGetProgressStepReturnsFirstStepIfNextWasNotCalledBefore() {
        assertThat(sut.getCurrentStep(), is(ProgressStep.DOWNLOADING));
    public void assertThatGetProgressStepReturnsCurrentStepIfNextWasCalledBefore() {
        ProgressStep[] steps = new ProgressStep[] { ProgressStep.DOWNLOADING, ProgressStep.TRANSFERRING,
                ProgressStep.UPDATING, ProgressStep.REBOOTING };
        sut.defineSequence(steps);
        for (int i = 0; i < steps.length - 1; i++) {
            assertThat(sut.getCurrentStep(), is(steps[i]));
    public void assertThatPendingThrowsIllegalArgumentExceptionIfStepSequenceIsNotDefinedAndNoProgressWasSet() {
        assertThrows(IllegalArgumentException.class, () -> sut.pending());
    public void assertThatPendingThrowsNoIllegalStateExceptionIfStepSequenceIsNotDefinedAndProgressWasSet() {
        sut.update(0);
        assertThatProgressInfoEventIsValid(postedEvents.get(1), null, true, 0);
    public void assertThatCancelThrowsNoIllegalStateExceptionIfStepSequenceIsNotDefined() {
        assertThatUpdateResultEventIsValid(postedEvents.get(0), CANCEL_MESSAGE_KEY, FirmwareUpdateResult.CANCELED);
    public void assertThatFailedThrowsIllegalStateExceptionIfItsCalledMultipleTimes() {
        sut.failed("DummyMessageKey");
        assertThrows(IllegalStateException.class, () -> sut.failed("DummyMessageKey"));
    public void assertThatFailedThrowsIllegalStateExceptionForSuccessfulUpdates() {
    public void assertThatSuccessThrowsIllegalStateExceptionIfItsCalledMultipleTimes() {
    public void assertThatPendingThrowsIllegalStateExceptionIfUpdateFailed() {
        assertThrows(IllegalStateException.class, () -> sut.pending());
    public void assertThatNextThrowsIllegalStateExceptionIfUpdateIsNotPendingAndNoFurtherStepsAvailable() {
        assertThrows(IllegalStateException.class, () -> sut.next());
    public void assertThatNextThrowsNoIllegalStateExceptionIfUpdateIsPendingAndNoFurtherStepsAvailable() {
    public void assertThatPendingThrowsIllegalStateExceptionIfUpdateWasSuccessful() {
    private void assertThatProgressInfoEventIsValid(Event event, @Nullable ProgressStep expectedStep,
            boolean expectedPending, @Nullable Integer expectedProgress) {
        FirmwareUpdateProgressInfoEvent fpiEvent = (FirmwareUpdateProgressInfoEvent) event;
        assertThat(fpiEvent.getProgressInfo().getThingUID(), is(EXPECTED_THING_UID));
        assertThat(fpiEvent.getProgressInfo().getFirmwareVersion(), is(expectedFirmware.getVersion()));
        assertThat(fpiEvent.getProgressInfo().getProgressStep(), is(expectedStep));
        assertThat(fpiEvent.getProgressInfo().getProgress(), is(expectedProgress));
        assertThat(fpiEvent.getProgressInfo().isPending(), (is(expectedPending)));
    private void assertThatUpdateResultEventIsValid(Event event, @Nullable String expectedMessageKey,
            FirmwareUpdateResult expectedResult) {
        FirmwareUpdateResultInfoEvent fpiEvent = (FirmwareUpdateResultInfoEvent) event;
        assertThat(usedMessagedKey, is(expectedMessageKey));
        assertThat(fpiEvent.getFirmwareUpdateResultInfo().getThingUID(), is(EXPECTED_THING_UID));
        assertThat(fpiEvent.getFirmwareUpdateResultInfo().getResult(), is(expectedResult));
    static class DummyFirmwareHandler implements FirmwareUpdateHandler {
            return ThingBuilder.create(THING_TYPE_UID1, THING1_ID).build();
