import java.net.MulticastSocket;
import java.net.ServerSocket;
import org.openhab.core.common.ThreadFactoryBuilder;
 * This is a {@link DiscoveryService} implementation, which can find SDDP devices in the network.
 * Support for bindings can be achieved by implementing and registering a {@link SddpDiscoveryParticipant}.
 * Support for finders can be achieved by implementing and registering a {@link SddpDeviceParticipant}.
@Component(immediate = true, service = DiscoveryService.class, property = "protocol=sddp", configurationPid = "discovery.sddp")
public class SddpDiscoveryService extends AbstractDiscoveryService
        implements AutoCloseable, NetworkAddressChangeListener {
    private static final int SDDP_PORT = 1902;
    private static final String SDDP_IP_ADDRESS = "239.255.255.250";
    private static final InetSocketAddress SDDP_GROUP = new InetSocketAddress(SDDP_IP_ADDRESS, SDDP_PORT);
    private static final int READ_BUFFER_SIZE = 1024;
    private static final Duration SOCKET_TIMOUT = Duration.ofMillis(1000);
    private static final Duration SEARCH_LISTEN_DURATION = Duration.ofSeconds(5);
    private static final Duration CACHE_PURGE_INTERVAL = Duration.ofSeconds(300);
    private static final String SEARCH_REQUEST_BODY_FORMAT = "SEARCH * SDDP/1.0\r\nHost: \"%s:%d\"\r\n";
    private static final String SEARCH_RESPONSE_HEADER = "SDDP/1.0 200 OK";
    private static final String NOTIFY_ALIVE_HEADER = "NOTIFY ALIVE SDDP/1.0";
    private static final String NOTIFY_IDENTIFY_HEADER = "NOTIFY IDENTIFY SDDP/1.0";
    private static final String NOTIFY_OFFLINE_HEADER = "NOTIFY OFFLINE SDDP/1.0";
    private final Logger logger = LoggerFactory.getLogger(SddpDiscoveryService.class);
    private final Set<SddpDevice> foundDevicesCache = ConcurrentHashMap.newKeySet();
    private final Set<SddpDiscoveryParticipant> discoveryParticipants = ConcurrentHashMap.newKeySet();
    private final Set<SddpDeviceParticipant> deviceParticipants = ConcurrentHashMap.newKeySet();
    private boolean closing = false;
    private ScheduledExecutorService longRunningTaskExecutor;
    private @Nullable Future<?> listenBackgroundMulticastTask = null;
    private @Nullable Future<?> listenActiveScanUnicastTask = null;
    private @Nullable ScheduledFuture<?> purgeExpiredDevicesTask = null;
    public SddpDiscoveryService(final @Nullable Map<String, Object> configProperties, //
            final @Reference NetworkAddressService networkAddressService, //
        super((int) SEARCH_LISTEN_DURATION.getSeconds());
        ScheduledThreadPoolExecutor executor = new ScheduledThreadPoolExecutor(2, ThreadFactoryBuilder.create()
                .withName("SDDP-discovery").withDaemonThreads(true).withUncaughtExceptionHandler((t, e) -> {
                    logger.debug("SDDP discovery service encountered an unexpected exception", e);
                }).build());
        executor.allowCoreThreadTimeOut(true);
        this.longRunningTaskExecutor = executor;
        super.activate(configProperties); // note: this starts listenBackgroundMulticastTask
        purgeExpiredDevicesTask = scheduler.scheduleWithFixedDelay(() -> purgeExpiredDevices(),
                CACHE_PURGE_INTERVAL.getSeconds(), CACHE_PURGE_INTERVAL.getSeconds(), TimeUnit.SECONDS);
    public void addSddpDeviceParticipant(SddpDeviceParticipant participant) {
        deviceParticipants.add(participant);
        foundDevicesCache.stream().filter(d -> !d.isExpired()).forEach(d -> participant.deviceAdded(d));
        startScan();
    protected void addSddpDiscoveryParticipant(SddpDiscoveryParticipant participant) {
        discoveryParticipants.add(participant);
        foundDevicesCache.stream().filter(d -> !d.isExpired()).forEach(d -> {
            DiscoveryResult result = participant.createResult(d);
     * Cancel the given task.
    private void cancelTask(@Nullable Future<?> task) {
        if (task != null) {
            task.cancel(true);
        deactivate();
     * Optionally create an {@link SddpDevice} object from UDP packet data if the data is good.
    public Optional<SddpDevice> createSddpDevice(String data) {
        if (!data.isBlank()) {
            List<String> lines = data.lines().toList();
            if (lines.size() > 1) {
                String statement = lines.getFirst().strip();
                boolean offline = statement.startsWith(NOTIFY_OFFLINE_HEADER);
                if (offline || statement.startsWith(NOTIFY_ALIVE_HEADER) || statement.startsWith(NOTIFY_IDENTIFY_HEADER)
                        || statement.startsWith(SEARCH_RESPONSE_HEADER)) {
                    Map<String, String> headers = new HashMap<>();
                    for (int i = 1; i < lines.size(); i++) {
                        String[] header = lines.get(i).split(":", 2);
                        if (header.length > 1) {
                            headers.put(header[0].strip(), header[1].strip());
                    return Optional.of(new SddpDevice(headers, offline));
        closing = true;
        foundDevicesCache.clear();
        discoveryParticipants.clear();
        deviceParticipants.clear();
        super.deactivate(); // note: this cancels and nulls listenBackgroundMulticastTask
        cancelTask(listenActiveScanUnicastTask);
        listenActiveScanUnicastTask = null;
        cancelTask(purgeExpiredDevicesTask);
        purgeExpiredDevicesTask = null;
        longRunningTaskExecutor.shutdownNow();
        discoveryParticipants.forEach(p -> supportedThingTypes.addAll(p.getSupportedThingTypeUIDs()));
     * Continue to listen for incoming SDDP multicast messages until the thread is externally interrupted.
    private void listenBackGroundMulticast() {
        MulticastSocket socket = null;
        NetworkInterface networkInterface = null;
            networkInterface = NetworkInterface
                    .getByInetAddress(InetAddress.getByName(networkAddressService.getPrimaryIpv4HostAddress()));
                logger.debug("listenBackGroundMulticast() starting on interface '{}'",
                        networkInterface.getDisplayName());
            socket = new MulticastSocket(SDDP_PORT);
            socket.joinGroup(SDDP_GROUP, networkInterface);
            socket.setSoTimeout((int) SOCKET_TIMOUT.toMillis());
            DatagramPacket packet = null;
            byte[] buffer = new byte[READ_BUFFER_SIZE];
            // loop listen for responses
                    if (packet == null) {
                        packet = new DatagramPacket(buffer, buffer.length);
                    socket.receive(packet);
                    processPacket(packet);
                    packet = null;
                    // socket.receive() will time out every 1 second so the thread won't block
            if (!closing) {
                logger.warn("listenBackGroundMulticast error '{}'", e.getMessage());
            if (socket != null && networkInterface != null) {
                    socket.leaveGroup(SDDP_GROUP, networkInterface);
                        logger.warn("listenBackGroundMulticast() error '{}'", e.getMessage());
     * Send a single outgoing SEARCH 'ping' and then continue to listen for incoming SDDP unicast responses until the
     * loop time elapses or the thread is externally interrupted.
    private void listenActiveScanUnicast() {
        // get a free port number
        int port;
        try (ServerSocket portFinder = new ServerSocket(0)) {
            port = portFinder.getLocalPort();
            logger.warn("listenActiveScanUnicast() port finder error '{}'", e.getMessage());
        try (DatagramSocket socket = new DatagramSocket(port)) {
            String ipAddress = networkAddressService.getPrimaryIpv4HostAddress();
            NetworkInterface networkInterface = NetworkInterface.getByInetAddress(InetAddress.getByName(ipAddress));
                logger.debug("listenActiveScanUnicast() starting on '{}:{}' on interface '{}'", ipAddress, port,
            socket.setOption(StandardSocketOptions.IP_MULTICAST_IF, networkInterface);
            DatagramPacket packet;
            byte[] buffer;
            // send search request
            String search = String.format(SEARCH_REQUEST_BODY_FORMAT, ipAddress, port);
            buffer = search.getBytes(StandardCharsets.UTF_8);
            packet = new DatagramPacket(buffer, buffer.length, new InetSocketAddress(SDDP_IP_ADDRESS, SDDP_PORT));
            socket.send(packet);
            logger.trace("Packet sent to '{}:{}' content:\r\n{}", SDDP_IP_ADDRESS, SDDP_PORT, search);
            final Instant listenDoneTime = Instant.now().plus(SEARCH_LISTEN_DURATION);
            buffer = new byte[READ_BUFFER_SIZE];
            while (Instant.now().isBefore(listenDoneTime) && !Thread.currentThread().isInterrupted()) {
                    // receive will time out every 1 second so the thread won't block
                logger.warn("listenActiveScanUnicast() error '{}'", e.getMessage());
     * If the network interfaces change then cancel and recreate all pending tasks.
    public synchronized void onChanged(List<CidrAddress> added, List<CidrAddress> removed) {
        Future<?> multicastTask = listenBackgroundMulticastTask;
        if (multicastTask != null && !multicastTask.isDone()) {
            multicastTask.cancel(true);
            listenBackgroundMulticastTask = longRunningTaskExecutor.submit(() -> listenBackGroundMulticast());
        Future<?> unicastTask = listenActiveScanUnicastTask;
        if (unicastTask != null && !unicastTask.isDone()) {
            unicastTask.cancel(true);
            listenActiveScanUnicastTask = longRunningTaskExecutor.submit(() -> listenActiveScanUnicast());
     * Process the {@link DatagramPacket} content by trying to create an {@link SddpDevice} and eventually adding it to
     * the foundDevicesCache, and if so, then notifying all listeners.
     * @param packet a datagram packet that arrived over the network.
    private synchronized void processPacket(DatagramPacket packet) {
        String content = new String(packet.getData(), 0, packet.getLength(), StandardCharsets.UTF_8);
            logger.trace("Packet received from '{}:{}' content:\r\n{}", packet.getAddress().getHostAddress(),
                    packet.getPort(), content);
        Optional<SddpDevice> deviceOptional = createSddpDevice(content);
        if (deviceOptional.isPresent()) {
            SddpDevice device = deviceOptional.get();
            foundDevicesCache.remove(device);
            if (device.isExpired()) {
                // device created from a NOTIFY OFFLINE announcement
                discoveryParticipants.forEach(p -> {
                    DiscoveryResult discoveryResult = p.createResult(device);
                    if (discoveryResult != null) {
                        thingRemoved(discoveryResult.getThingUID());
                deviceParticipants.forEach(f -> f.deviceRemoved(device));
                // device created from a NOTIFY ALIVE announcement or SEARCH response
                foundDevicesCache.add(device);
                        thingDiscovered(discoveryResult, FrameworkUtil.getBundle(p.getClass()));
                deviceParticipants.forEach(f -> f.deviceAdded(device));
                logger.debug("processPacket() foundDevices={}, deviceParticipants={}, discoveryParticipants={}",
                        foundDevicesCache.size(), deviceParticipants.size(), discoveryParticipants.size());
     * Purge expired devices and notify all listeners.
    private synchronized void purgeExpiredDevices() {
        Set<SddpDevice> devices = new HashSet<>(foundDevicesCache);
        devices.stream().filter(d -> d.isExpired()).forEach(d -> {
                ThingUID thingUID = p.getThingUID(d);
            deviceParticipants.forEach(f -> f.deviceRemoved(d));
        foundDevicesCache.addAll(devices.stream().filter(d -> !d.isExpired()).collect(Collectors.toSet()));
    public void removeSddpDeviceParticipant(SddpDeviceParticipant participant) {
        deviceParticipants.remove(participant);
    public void removeSddpDiscoveryParticipant(SddpDiscoveryParticipant participant) {
        discoveryParticipants.remove(participant);
        Future<?> task = listenBackgroundMulticastTask;
        if (task == null || task.isDone()) {
     * Schedule to send one single SDDP SEARCH request, and listen for responses.
        Future<?> task = listenActiveScanUnicastTask;
        cancelTask(listenBackgroundMulticastTask);
        listenBackgroundMulticastTask = null;
