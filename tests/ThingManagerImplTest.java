public class ThingManagerImplTest extends JavaTest {
    private @Mock @NonNullByDefault({}) ChannelGroupTypeRegistry channelGroupTypeRegistryMock;
    private @Mock @NonNullByDefault({}) CommunicationManager communicationManagerMock;
    private @Mock @NonNullByDefault({}) ConfigDescriptionValidator configDescriptionValidatorMock;
    private @Mock @NonNullByDefault({}) ThingRegistryImpl thingRegistryMock;
    private @Mock @NonNullByDefault({}) BundleResolver bundleResolverMock;
    private @Mock @NonNullByDefault({}) ThingUpdateInstructionReader thingUpdateInstructionReaderMock;
    private @Mock @NonNullByDefault({}) TranslationProvider translationProviderMock;
    // This class is final so it cannot be mocked
    private final ThingStatusInfoI18nLocalizationService thingStatusInfoI18nLocalizationService = new ThingStatusInfoI18nLocalizationService();
        when(thingMock.getUID()).thenReturn(new ThingUID("test", "thing"));
        when(thingMock.getStatusInfo())
                .thenReturn(new ThingStatusInfo(ThingStatus.UNINITIALIZED, ThingStatusDetail.NONE, null));
    private ThingManagerImpl createThingManager() {
        return new ThingManagerImpl(channelGroupTypeRegistryMock, channelTypeRegistryMock, communicationManagerMock,
                configDescriptionRegistryMock, configDescriptionValidatorMock, eventPublisherMock,
                itemChannelLinkRegistryMock, readyServiceMock, safeCallerMock, storageServiceMock, thingRegistryMock,
                thingStatusInfoI18nLocalizationService, thingTypeRegistryMock, thingUpdateInstructionReaderMock,
                bundleResolverMock, translationProviderMock, bundleContextMock);
    public void thingHandlerFactoryLifecycle() {
        ThingHandlerFactory mockFactory1 = mock(ThingHandlerFactory.class);
        ThingHandlerFactory mockFactory2 = mock(ThingHandlerFactory.class);
        when(storageServiceMock.getStorage(any(), any())).thenReturn(storageMock);
        when(storageMock.get(any())).thenReturn(null);
        ThingManagerImpl thingManager = createThingManager();
        thingManager.thingAdded(thingMock, ThingTrackerEvent.THING_ADDED);
        thingManager.addThingHandlerFactory(mockFactory1);
        verify(mockFactory1, atLeastOnce()).supportsThingType(any());
        thingManager.removeThingHandlerFactory(mockFactory1);
        thingManager.addThingHandlerFactory(mockFactory2);
        verify(mockFactory2, atLeastOnce()).supportsThingType(any());
        thingManager.removeThingHandlerFactory(mockFactory2);
    public void setEnabledWithUnknownThingUID() throws Exception {
        ThingUID unknownUID = new ThingUID("someBundle", "someType", "someID");
        when(storageServiceMock.getStorage(eq("thing_status_storage"), any(ClassLoader.class))).thenReturn(storageMock);
        thingManager.setEnabled(unknownUID, true);
        verify(storageMock).remove(eq(unknownUID.getAsString()));
        thingManager.setEnabled(unknownUID, false);
        verify(storageMock).put(eq(unknownUID.getAsString()), eq(""));
    public void isEnabledWithUnknownThingUIDAndNullStorage() throws Exception {
        assertTrue(thingManager.isEnabled(unknownUID));
    public void isEnabledWithUnknownThingUIDAndNonNullStorage() throws Exception {
        when(storageMock.containsKey(unknownUID.getAsString())).thenReturn(false);
        when(storageMock.containsKey(unknownUID.getAsString())).thenReturn(true);
        assertFalse(thingManager.isEnabled(unknownUID));
    public void removingNonExistingThingLogsError() {
        setupInterceptedLogger(ThingManagerImpl.class, LogLevel.DEBUG);
        thingManager.thingRemoved(thingMock, ThingTrackerEvent.THING_REMOVED);
        stopInterceptedLogger(ThingManagerImpl.class);
        assertLogMessage(ThingManagerImpl.class, LogLevel.ERROR,
                "Trying to remove thing 'test::thing', but is not tracked by ThingManager. This is a bug.");
    public void addingExistingThingLogsError() {
                "A thing with UID 'test::thing' is already tracked by ThingManager. This is a bug.");
    public void replacingThingWithWrongUIDLogsError() {
        Thing thingMock2 = mock(Thing.class);
        when(thingMock2.getUID()).thenReturn(new ThingUID("test", "thing2"));
        when(thingMock2.getStatusInfo())
        thingManager.thingUpdated(thingMock2, thingMock, ThingTrackerEvent.THING_ADDED);
                "Thing 'test::thing2' is different from thing tracked by ThingManager. This is a bug.");
