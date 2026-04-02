import static org.mockito.ArgumentMatchers.anyBoolean;
import org.jupnp.model.meta.LocalService;
import org.jupnp.model.types.ServiceType;
 * Tests {@link UpnpIOServiceImpl}.
public class UpnpIOServiceTest {
    private static final String UDN_1_STRING = "UDN";
    private static final UDN UDN_1 = new UDN(UDN_1_STRING);
    private static final String UDN_2_STRING = "UDN2";
    private static final UDN UDN_2 = new UDN(UDN_2_STRING);
    private static final String SERVICE_ID = "serviceId";
    private static final String SERVICE_ID_2 = "serviceId2";
    private static final String ACTION_ID = "actionId";
    private static final String SERVICE_TYPE = "serviceType";
    private @Mock @NonNullByDefault({}) UpnpIOParticipant upnpIoParticipantMock;
    private @Mock @NonNullByDefault({}) UpnpIOParticipant upnpIoParticipant2Mock;
    private @Mock @NonNullByDefault({}) Registry upnpRegistryMock;
    private @Mock @NonNullByDefault({}) ControlPoint controlPointMock;
    private @Mock @NonNullByDefault({}) UpnpService upnpServiceMock;
    private @NonNullByDefault({}) UpnpIOServiceImpl upnpIoService;
        when(upnpIoParticipantMock.getUDN()).thenReturn(UDN_1_STRING);
        when(upnpIoParticipant2Mock.getUDN()).thenReturn(UDN_2_STRING);
        DeviceIdentity deviceIdentity = new DeviceIdentity(UDN_1);
        DeviceType deviceType = new DeviceType(UDAServiceId.DEFAULT_NAMESPACE, DEVICE_TYPE, 1);
        ServiceType serviceType = new ServiceType(UDAServiceId.DEFAULT_NAMESPACE, SERVICE_TYPE);
        ServiceId serviceId = new ServiceId(UDAServiceId.DEFAULT_NAMESPACE, SERVICE_ID);
        LocalService<?> service = new LocalService<>(serviceType, serviceId, null, null);
        LocalDevice device = new LocalDevice(deviceIdentity, deviceType, (DeviceDetails) null, service);
        ServiceId serviceId2 = new ServiceId(UDAServiceId.DEFAULT_NAMESPACE, SERVICE_ID_2);
        LocalService<?> service2 = new LocalService<>(serviceType, serviceId2, null, null);
        LocalDevice device2 = new LocalDevice(deviceIdentity, deviceType, (DeviceDetails) null, service2);
        when(upnpRegistryMock.getDevice(eq(UDN_1), anyBoolean())).thenReturn(device);
        when(upnpRegistryMock.getDevice(eq(UDN_2), anyBoolean())).thenReturn(device2);
        when(upnpServiceMock.getRegistry()).thenReturn(upnpRegistryMock);
        when(upnpServiceMock.getControlPoint()).thenReturn(controlPointMock);
        upnpIoService = new UpnpIOServiceImpl(upnpServiceMock);
    public void testIsRegistered() {
        assertTrue(upnpIoService.isRegistered(upnpIoParticipantMock));
    public void testIsRegisteredEverythingEmptyInitially() {
        assertThatEverythingIsEmpty();
    public void testRegisterParticipant() {
        upnpIoService.registerParticipant(upnpIoParticipantMock);
        assertEquals(1, upnpIoService.participants.size());
        assertTrue(upnpIoService.participants.contains(upnpIoParticipantMock));
        assertTrue(upnpIoService.pollingJobs.keySet().isEmpty());
        assertTrue(upnpIoService.currentStates.keySet().isEmpty());
        assertTrue(upnpIoService.subscriptionCallbacks.keySet().isEmpty());
    public void testAddStatusListener() {
        upnpIoService.addStatusListener(upnpIoParticipantMock, SERVICE_ID, ACTION_ID, 60);
        assertEquals(1, upnpIoService.pollingJobs.keySet().size());
        assertTrue(upnpIoService.pollingJobs.containsKey(upnpIoParticipantMock));
        assertEquals(1, upnpIoService.currentStates.keySet().size());
        assertTrue(upnpIoService.currentStates.containsKey(upnpIoParticipantMock));
        upnpIoService.removeStatusListener(upnpIoParticipantMock);
    public void testAddSubscription() {
        upnpIoService.addSubscription(upnpIoParticipantMock, SERVICE_ID, 60);
        assertEquals(1, upnpIoService.subscriptionCallbacks.size());
        upnpIoService.addSubscription(upnpIoParticipant2Mock, SERVICE_ID_2, 60);
        assertEquals(2, upnpIoService.participants.size());
        assertEquals(2, upnpIoService.subscriptionCallbacks.size());
        upnpIoService.removeSubscription(upnpIoParticipantMock, SERVICE_ID);
        upnpIoService.unregisterParticipant(upnpIoParticipantMock);
        assertTrue(upnpIoService.participants.contains(upnpIoParticipant2Mock));
        upnpIoService.removeSubscription(upnpIoParticipant2Mock, SERVICE_ID_2);
        upnpIoService.unregisterParticipant(upnpIoParticipant2Mock);
    private void assertThatEverythingIsEmpty() {
        assertTrue(upnpIoService.participants.isEmpty());
