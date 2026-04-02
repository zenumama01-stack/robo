package org.openhab.core.config.discovery.sddp.test;
 * JUnit tests for parsing SDDP discovery results.
public class SddpDiscoveryServiceTests {
    private static final String ALIVE_NOTIFICATION = """
            NOTIFY ALIVE SDDP/1.0
            From: "192.168.4.237:1902"
            Host: "JVC_PROJECTOR-E0DADC152802"
            Max-Age: 1800
            Type: "JVCKENWOOD:Projector"
            Primary-Proxy: "projector"
            Proxies: "projector"
            Manufacturer: "JVCKENWOOD"
            Model: "DLA-RS3100_NZ8"
            Driver: "projector_JVCKENWOOD_DLA-RS3100_NZ8.c4i"
            """;
    private static final String IDENTIFY_NOTIFICATION = """
            NOTIFY IDENTIFY SDDP/1.0
            Host: "JVC_PROJECTOR-E0:DA:DC:15:28:02"
    private static final String BAD_HEADER = """
            SDDP/1.0 404 NOT FOUND\r
            From: "192.168.4.237:1902"\r
            Host: "JVC_PROJECTOR-E0DADC152802"\r
            Max-Age: 1800\r
            Type: "JVCKENWOOD:Projector"\r
            Primary-Proxy: "projector"\r
            Proxies: "projector"\r
            Manufacturer: "JVCKENWOOD"\r
            Model: "DLA-RS3100_NZ8"\r
            Driver: "projector_JVCKENWOOD_DLA-RS3100_NZ8.c4i"\r
    private static final String BAD_PAYLOAD = """
            SDDP/1.0 200 OK\r
    private static final String SEARCH_RESPONSE = """
    private @NonNullByDefault({}) NetworkAddressService networkAddressService;
        networkAddressService = mock(NetworkAddressService.class);
        when(networkAddressService.getPrimaryIpv4HostAddress()).thenReturn("192.168.1.1");
    void testAliveNotification() throws Exception {
        try (SddpDiscoveryService service = new SddpDiscoveryService(null, networkAddressService,
                mock(TranslationProvider.class), mock(LocaleProvider.class))) {
            Optional<SddpDevice> deviceOptional = service.createSddpDevice(ALIVE_NOTIFICATION);
            assertTrue(deviceOptional.isPresent());
            SddpDevice device = deviceOptional.orElse(null);
            assertNotNull(device);
            assertEquals("192.168.4.237:1902", device.from);
            assertEquals("JVC_PROJECTOR-E0DADC152802", device.host);
            assertEquals("1800", device.maxAge);
            assertEquals("JVCKENWOOD:Projector", device.type);
            assertEquals("projector", device.primaryProxy);
            assertEquals("projector", device.proxies);
            assertEquals("JVCKENWOOD", device.manufacturer);
            assertEquals("DLA-RS3100_NZ8", device.model);
            assertEquals("projector_JVCKENWOOD_DLA-RS3100_NZ8.c4i", device.driver);
            assertEquals("192.168.4.237", device.ipAddress);
            assertEquals("e0-da-dc-15-28-02", device.macAddress);
            assertEquals("1902", device.port);
    void testIdentifyNotification() throws Exception {
            Optional<SddpDevice> deviceOptional = service.createSddpDevice(IDENTIFY_NOTIFICATION);
            assertEquals("JVC_PROJECTOR-E0:DA:DC:15:28:02", device.host);
            assertTrue(device.maxAge.isBlank());
    void testBadHeader() throws Exception {
            Optional<SddpDevice> deviceOptional = service.createSddpDevice(BAD_HEADER);
            assertFalse(deviceOptional.isPresent());
    void testBadPayload() throws Exception {
            Optional<SddpDevice> deviceOptional = service.createSddpDevice(BAD_PAYLOAD);
    void testSearchResponse() throws Exception {
            Optional<SddpDevice> deviceOptional = service.createSddpDevice(SEARCH_RESPONSE);
