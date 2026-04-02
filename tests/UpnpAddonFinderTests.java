package org.openhab.core.config.discovery.addon.upnp.tests;
import org.jupnp.model.ValidationException;
import org.jupnp.model.meta.RemoteService;
import org.openhab.core.config.discovery.addon.upnp.UpnpAddonFinder;
 * JUnit tests for the {@link UpnpAddonFinder}.
public class UpnpAddonFinderTests {
    private @NonNullByDefault({}) UpnpService upnpService;
        setupMockUpnpService();
        UpnpAddonFinder upnpAddonFinder = new UpnpAddonFinder(upnpService);
        assertNotNull(upnpAddonFinder);
        addonFinder = upnpAddonFinder;
    private void setupMockUpnpService() {
        upnpService = mock(UpnpService.class, Mockito.RETURNS_DEEP_STUBS);
        URL url = null;
            url = URI.create("http://www.openhab.org/").toURL();
            fail("MalformedURLException");
        UDN udn = new UDN("udn");
        InetAddress address = null;
            address = InetAddress.getByName("127.0.0.1");
            fail("UnknownHostException");
        RemoteDeviceIdentity identity = new RemoteDeviceIdentity(udn, 0, url, new byte[] {}, address);
        DeviceType type = new DeviceType("nameSpace", "type");
        ManufacturerDetails manDetails = new ManufacturerDetails("manufacturer", "manufacturerURI");
        ModelDetails modDetails = new ModelDetails("Philips hue bridge", "modelDescription", "modelNumber", "modelURI");
        DeviceDetails devDetails = new DeviceDetails("friendlyName", manDetails, modDetails, "serialNumber",
                "000123456789");
        List<@Nullable RemoteDevice> remoteDevices = new ArrayList<>();
            remoteDevices.add(new RemoteDevice(identity, type, devDetails, (RemoteService) null));
        } catch (ValidationException e1) {
            fail("ValidationException");
        when(upnpService.getRegistry().getRemoteDevices()).thenReturn(remoteDevices);
        assertNotNull(upnpService);
        List<RemoteDevice> result = new ArrayList<>(upnpService.getRegistry().getRemoteDevices());
        assertEquals(1, result.size());
        RemoteDevice device = result.getFirst();
        assertEquals("manufacturer", device.getDetails().getManufacturerDetails().getManufacturer());
        assertEquals("serialNumber", device.getDetails().getSerialNumber());
        AddonDiscoveryMethod hue = new AddonDiscoveryMethod().setServiceType(AddonFinderConstants.SERVICE_TYPE_UPNP)
        assertEquals(1, addons.size());
