 * Some utility functions related to network interfaces etc.
 * @author Mark Herwege - Added methods to find broadcast address(es)
 * @author Stefan Triller - Converted to OSGi service with primary ipv4 conf
 * @author Gary Tse - Network address change listener
 * @author Tim Roberts - Added primary address change to network address change listener
 * @author Leo Siepel - Added methods to improve support for network scanning
@Component(configurationPid = "org.openhab.network", property = { "service.pid=org.openhab.network",
        "service.config.description.uri=system:network", "service.config.label=Network Settings",
        "service.config.category=system" })
public class NetUtil implements NetworkAddressService {
    private static final String PRIMARY_ADDRESS = "primaryAddress";
    private static final String BROADCAST_ADDRESS = "broadcastAddress";
    private static final String POLL_INTERVAL = "pollInterval";
    private static final String USE_ONLY_ONE_ADDRESS = "useOnlyOneAddress";
    private static final String USE_IPV6 = "useIPv6";
    private static final Logger LOGGER = LoggerFactory.getLogger(NetUtil.class);
     * Default network interface poll interval 60 seconds.
    public static final int POLL_INTERVAL_SECONDS = 60;
    private static final Pattern IPV4_PATTERN = Pattern
            .compile("^(([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.){3}([01]?\\d\\d?|2[0-4]\\d|25[0-5])$");
    private @Nullable String primaryAddress;
    private @Nullable String configuredBroadcastAddress;
    private boolean useOnlyOneAddress;
    private boolean useIPv6;
    // must be initialized before activate due to OSGi reference
    private Set<NetworkAddressChangeListener> networkAddressChangeListeners = ConcurrentHashMap.newKeySet();
    private Collection<CidrAddress> lastKnownInterfaceAddresses = List.of();
    private @Nullable ScheduledFuture<?> networkInterfacePollFuture = null;
    public NetUtil(final @Reference SafeCaller safeCaller) {
    protected void activate(Map<String, Object> props) {
        lastKnownInterfaceAddresses = List.of();
        modified(props);
        networkAddressChangeListeners = ConcurrentHashMap.newKeySet();
        if (networkInterfacePollFuture instanceof ScheduledFuture<?> future) {
            networkInterfacePollFuture = null;
        String primaryAddressConf = (String) config.get(PRIMARY_ADDRESS);
        String oldPrimaryAddress = primaryAddress;
        if (primaryAddressConf == null || primaryAddressConf.isEmpty() || !isValidIPConfig(primaryAddressConf)) {
            // if none is specified we return the default one for backward compatibility
            primaryAddress = getFirstLocalIPv4Address();
            primaryAddress = primaryAddressConf;
        notifyPrimaryAddressChange(oldPrimaryAddress, primaryAddress);
        String broadcastAddressConf = (String) config.get(BROADCAST_ADDRESS);
        if (broadcastAddressConf == null || broadcastAddressConf.isEmpty() || !isValidIPConfig(broadcastAddressConf)) {
            // if none is specified we return the one matching the primary ip
            configuredBroadcastAddress = getPrimaryBroadcastAddress();
            configuredBroadcastAddress = broadcastAddressConf;
        useOnlyOneAddress = getConfigParameter(config, USE_ONLY_ONE_ADDRESS, false);
        useIPv6 = getConfigParameter(config, USE_IPV6, true);
        Object pollIntervalSecondsObj = null;
        int pollIntervalSeconds = POLL_INTERVAL_SECONDS;
            pollIntervalSecondsObj = config.get(POLL_INTERVAL);
            if (pollIntervalSecondsObj != null) {
                pollIntervalSeconds = Integer.parseInt(pollIntervalSecondsObj.toString());
            LOGGER.warn("Cannot parse value {} from key {}, will use default {}", pollIntervalSecondsObj, POLL_INTERVAL,
                    pollIntervalSeconds);
        scheduleToPollNetworkInterface(pollIntervalSeconds);
    public @Nullable String getPrimaryIpv4HostAddress() {
        String primaryIP;
        if (primaryAddress instanceof String address) {
            String[] addrString = address.split("/");
            if (addrString.length > 1) {
                String ip = getIPv4inSubnet(addrString[0], addrString[1]);
                if (ip == null) {
                    // an error has occurred, using first interface like nothing has been configured
                    LOGGER.warn("Invalid address '{}', will use first interface instead.", address);
                    primaryIP = getFirstLocalIPv4Address();
                    primaryIP = ip;
                primaryIP = addrString[0];
            // we do not seem to have any network interfaces
            primaryIP = null;
        return primaryIP;
     * Use only one address per interface and family (IPv4 and IPv6). If set listeners should bind only to one address
     * per interface and family.
     * @return use only one address per interface and family
    public boolean isUseOnlyOneAddress() {
        return useOnlyOneAddress;
     * Use IPv6. If not set, IPv6 addresses should be completely ignored by listeners.
     * @return use IPv6
    public boolean isUseIPv6() {
        return useIPv6;
    // These are NOT OSGi service injections, but listeners have to register themselves at this service.
    // This is required in order to avoid cyclic dependencies, see https://github.com/eclipse/smarthome/issues/6073
    public void addNetworkAddressChangeListener(NetworkAddressChangeListener listener) {
        networkAddressChangeListeners.add(listener);
    public void removeNetworkAddressChangeListener(NetworkAddressChangeListener listener) {
        networkAddressChangeListeners.remove(listener);
    private @Nullable String getFirstLocalIPv4Address() {
            String hostAddress = null;
            final Enumeration<NetworkInterface> interfaces = NetworkInterface.getNetworkInterfaces();
            while (interfaces.hasMoreElements()) {
                final NetworkInterface current = interfaces.nextElement();
                if (!current.isUp() || current.isLoopback() || current.isVirtual() || current.isPointToPoint()) {
                final Enumeration<InetAddress> addresses = current.getInetAddresses();
                while (addresses.hasMoreElements()) {
                    final InetAddress currentAddr = addresses.nextElement();
                    if (currentAddr.isLoopbackAddress() || (currentAddr instanceof Inet6Address)) {
                    if (hostAddress != null) {
                        LOGGER.warn("Found multiple local interfaces - ignoring {}", currentAddr.getHostAddress());
                        hostAddress = currentAddr.getHostAddress();
            return hostAddress;
        } catch (SocketException ex) {
            LOGGER.error("Could not retrieve network interface: {}", ex.getMessage(), ex);
     * Get all broadcast addresses on the current host
     * @return list of broadcast addresses, empty list if no broadcast addresses found
    public static List<String> getAllBroadcastAddresses() {
        List<String> broadcastAddresses = new LinkedList<>();
            final Enumeration<NetworkInterface> networkInterfaces = NetworkInterface.getNetworkInterfaces();
                final NetworkInterface networkInterface = networkInterfaces.nextElement();
                final List<InterfaceAddress> interfaceAddresses = networkInterface.getInterfaceAddresses();
                for (InterfaceAddress interfaceAddress : interfaceAddresses) {
                    final InetAddress addr = interfaceAddress.getAddress();
                    if (addr instanceof Inet4Address && !addr.isLinkLocalAddress() && !addr.isLoopbackAddress()) {
                        InetAddress broadcast = interfaceAddress.getBroadcast();
                        if (broadcast != null) {
                            broadcastAddresses.add(broadcast.getHostAddress());
            LOGGER.error("Could not find broadcast address: {}", ex.getMessage(), ex);
        return broadcastAddresses;
     * Finds the local interface address that is on the same subnet as the specified IP address, if one exists.
     * @param address the IP address for which to find an interface address on the same subnet.
     * @return The matching {@link InterfaceAddress} or {@code null} if no such local interface address exists.
    public static @Nullable InterfaceAddress getSameSubnetInterfaceAddress(InetAddress address) {
        byte[] addrBytes = address.getAddress();
        Iterator<InterfaceAddress> ias;
            ias = NetworkInterface.networkInterfaces().filter(ni -> {
                    return !ni.isLoopback() && !ni.isPointToPoint() && ni.isUp();
            }).flatMap(ni -> ni.getInterfaceAddresses().stream()).filter(ia -> {
                InetAddress addr = ia.getAddress();
                return addr != null && !addr.isAnyLocalAddress() && !addr.isLinkLocalAddress()
                        && !addr.isLoopbackAddress();
            }).iterator();
        InterfaceAddress ia;
        short prefix;
        byte[] iaBytes;
        byte mask;
        InetAddress addr;
        while (ias.hasNext()) {
            ia = ias.next();
            addr = ia.getAddress();
            if (addr == null) {
            iaBytes = addr.getAddress();
            if (iaBytes.length != addrBytes.length) {
            prefix = ia.getNetworkPrefixLength();
            while (prefix > (short) 0) {
                if (prefix > (short) 8) {
                    if (addrBytes[i] != iaBytes[i]) {
                    mask = (byte) ((byte) 0xff << (8 - prefix));
                    if ((addrBytes[i] & mask) == (iaBytes[i] & mask)) {
                        return ia;
                prefix -= (short) 8;
    public @Nullable String getConfiguredBroadcastAddress() {
        String broadcastAddr;
        if (configuredBroadcastAddress != null) {
            broadcastAddr = configuredBroadcastAddress;
            broadcastAddr = null;
        return broadcastAddr;
    private @Nullable String getPrimaryBroadcastAddress() {
        String primaryIp = getPrimaryIpv4HostAddress();
        String broadcastAddress = null;
        if (primaryIp != null) {
                Short prefix = getAllInterfaceAddresses().stream()
                        .filter(a -> a.getAddress().getHostAddress().equals(primaryIp)).map(CidrAddress::getPrefix)
                        .findFirst().get().shortValue();
                broadcastAddress = getIpv4NetBroadcastAddress(primaryIp, prefix);
                LOGGER.error("Invalid IP address parameter: {}", ex.getMessage(), ex);
        if (broadcastAddress == null) {
            // an error has occurred, using broadcast address of first interface instead
            broadcastAddress = getFirstIpv4BroadcastAddress();
                    "Could not find broadcast address of primary IP, using broadcast address {} of first interface instead",
                    broadcastAddress);
        return broadcastAddress;
    private static @Nullable String getFirstIpv4BroadcastAddress() {
        final List<String> broadcastAddresses = getAllBroadcastAddresses();
        if (!broadcastAddresses.isEmpty()) {
            return broadcastAddresses.getFirst();
     * Gets every IPv4+IPv6 Address on each Interface except the loopback interface.
     * The Address format is in the CIDR notation which is ip/prefix-length e.g. 129.31.31.1/24.
     * Example to get a list of only IPv4 addresses in string representation:
     * List<String> l = getAllInterfaceAddresses().stream().filter(a->a.getAddress() instanceof
     * Inet4Address).map(a->a.getAddress().getHostAddress()).toList();
     * down, or loopback interfaces are skipped.
     * @return The collected IPv4 and IPv6 Addresses
    public static Collection<CidrAddress> getAllInterfaceAddresses() {
        Collection<CidrAddress> interfaceIPs = new ArrayList<>();
        Enumeration<NetworkInterface> en;
            en = NetworkInterface.getNetworkInterfaces();
            LOGGER.error("Could not find interface IP addresses: {}", ex.getMessage(), ex);
            return interfaceIPs;
        while (en.hasMoreElements()) {
            NetworkInterface networkInterface = en.nextElement();
                if (!networkInterface.isUp() || networkInterface.isLoopback()) {
            } catch (SocketException ignored) {
            for (InterfaceAddress cidr : networkInterface.getInterfaceAddresses()) {
                // NetworkInterface.getInterfaceAddresses() should return only non-null addresses
                final InetAddress address = Objects.requireNonNull(cidr.getAddress());
                interfaceIPs.add(new CidrAddress(address, cidr.getNetworkPrefixLength()));
     * Converts a netmask in bits into a string representation
     * i.e. 24 bits -> 255.255.255.0
     * @param prefixLength bits of the netmask
     * @return string representation of netmask (i.e. 255.255.255.0)
    public static String networkPrefixLengthToNetmask(int prefixLength) {
        if (prefixLength > 32 || prefixLength < 1) {
            throw new IllegalArgumentException("Network prefix length is not within bounds");
        int ipv4Netmask = 0xFFFFFFFF;
        ipv4Netmask <<= (32 - prefixLength);
        byte[] octets = new byte[] { (byte) (ipv4Netmask >>> 24), (byte) (ipv4Netmask >>> 16),
                (byte) (ipv4Netmask >>> 8), (byte) ipv4Netmask };
        String result = "";
        for (int i = 0; i < 4; i++) {
            result += octets[i] & 0xff;
            if (i < 3) {
                result += ".";
     * Get the network address a specific ip address is in
     * @param ipAddressString IPv4 address of the device (i.e. 192.168.5.1)
     * @param netMask netmask in bits (i.e. 24)
     * @return network a device is in (i.e. 192.168.5.0)
     * @throws IllegalArgumentException if parameters are wrong
    public static String getIpv4NetAddress(String ipAddressString, short netMask) {
        String errorString = "IP '" + ipAddressString + "' is not a valid IPv4 address";
        if (!isValidIPConfig(ipAddressString)) {
            throw new IllegalArgumentException(errorString);
        if (netMask < 1 || netMask > 32) {
            throw new IllegalArgumentException("Netmask '" + netMask + "' is out of bounds (1-32)");
        String subnetMaskString = networkPrefixLengthToNetmask(netMask);
        String[] netMaskOctets = subnetMaskString.split("\\.");
        String[] ipv4AddressOctets = ipAddressString.split("\\.");
        String netAddress = "";
                netAddress += Integer.parseInt(ipv4AddressOctets[i]) & Integer.parseInt(netMaskOctets[i]);
                    netAddress += ".";
        return netAddress;
     * Get the network broadcast address of the subnet a specific ip address is in
     * @param prefix network prefix in bits (i.e. 24)
     * @return network broadcast address of the network the device is in (i.e. 192.168.5.255)
    public static String getIpv4NetBroadcastAddress(String ipAddressString, short prefix) {
        if (prefix < 1 || prefix > 32) {
            throw new IllegalArgumentException("Prefix '" + prefix + "' is out of bounds (1-32)");
            byte[] addr = InetAddress.getByName(ipAddressString).getAddress();
            byte[] netmask = InetAddress.getByName(networkPrefixLengthToNetmask(prefix)).getAddress();
            byte[] broadcast = new byte[] { (byte) (~netmask[0] | addr[0]), (byte) (~netmask[1] | addr[1]),
                    (byte) (~netmask[2] | addr[2]), (byte) (~netmask[3] | addr[3]) };
            return InetAddress.getByAddress(broadcast).getHostAddress();
        } catch (UnknownHostException ex) {
    private @Nullable String getIPv4inSubnet(String ipAddress, String subnetMask) {
                for (InterfaceAddress ifAddr : current.getInterfaceAddresses()) {
                    InetAddress addr = ifAddr.getAddress();
                    if (addr.isLoopbackAddress() || (addr instanceof Inet6Address)) {
                    String ipv4AddressOnInterface = addr.getHostAddress();
                    String subnetStringOnInterface = getIpv4NetAddress(ipv4AddressOnInterface,
                            ifAddr.getNetworkPrefixLength()) + "/" + ifAddr.getNetworkPrefixLength();
                    String configuredSubnetString = getIpv4NetAddress(ipAddress, Short.parseShort(subnetMask)) + "/"
                            + subnetMask;
                    // use first IP within this subnet
                    if (subnetStringOnInterface.equals(configuredSubnetString)) {
                        return ipv4AddressOnInterface;
     * Checks if the given String is a valid IPv4 Address
     * or IPv4 address in CIDR notation
     * @param ipAddress in format xxx.xxx.xxx.xxx or xxx.xxx.xxx.xxx/xx
     * @return true if it is a valid address
    public static boolean isValidIPConfig(String ipAddress) {
        if (ipAddress.contains("/")) {
            String[] parts = ipAddress.split("/");
            boolean ipMatches = IPV4_PATTERN.matcher(parts[0]).matches();
            int netMask = Integer.parseInt(parts[1]);
            boolean netMaskMatches = false;
            if (netMask > 0 || netMask < 32) {
                netMaskMatches = true;
            if (ipMatches && netMaskMatches) {
            return IPV4_PATTERN.matcher(ipAddress).matches();
    private void scheduleToPollNetworkInterface(int intervalInSeconds) {
        networkInterfacePollFuture = scheduledExecutorService.scheduleWithFixedDelay(
                this::pollAndNotifyNetworkInterfaceAddress, 1, intervalInSeconds, TimeUnit.SECONDS);
    private void pollAndNotifyNetworkInterfaceAddress() {
        Collection<CidrAddress> newInterfaceAddresses = getAllInterfaceAddresses();
        if (networkAddressChangeListeners.isEmpty()) {
            // no listeners listening, just update
            lastKnownInterfaceAddresses = newInterfaceAddresses;
        // Look for added addresses to notify
        List<CidrAddress> added = newInterfaceAddresses.stream()
                .filter(newInterfaceAddr -> !lastKnownInterfaceAddresses.contains(newInterfaceAddr)).toList();
        // Look for removed addresses to notify
        List<CidrAddress> removed = lastKnownInterfaceAddresses.stream()
                .filter(lastKnownInterfaceAddr -> !newInterfaceAddresses.contains(lastKnownInterfaceAddr)).toList();
        if (!added.isEmpty() || !removed.isEmpty()) {
                LOGGER.debug("added {} network interfaces: {}", added.size(), Arrays.deepToString(added.toArray()));
                LOGGER.debug("removed {} network interfaces: {}", removed.size(),
                        Arrays.deepToString(removed.toArray()));
            notifyListeners(added, removed);
    private void notifyListeners(List<CidrAddress> added, List<CidrAddress> removed) {
        // Prevent listeners changing the list
        List<CidrAddress> unmodifiableAddedList = Collections.unmodifiableList(added);
        List<CidrAddress> unmodifiableRemovedList = Collections.unmodifiableList(removed);
        // notify each listener with a timeout of 15 seconds.
        // SafeCaller prevents bad listeners running too long or throws runtime exceptions
        for (NetworkAddressChangeListener listener : networkAddressChangeListeners) {
            NetworkAddressChangeListener safeListener = safeCaller.create(listener, NetworkAddressChangeListener.class)
                    .withTimeout(15000)
                    .onException(exception -> LOGGER.debug("NetworkAddressChangeListener exception", exception))
            safeListener.onChanged(unmodifiableAddedList, unmodifiableRemovedList);
    private void notifyPrimaryAddressChange(@Nullable String oldPrimaryAddress, @Nullable String newPrimaryAddress) {
        if (!Objects.equals(oldPrimaryAddress, newPrimaryAddress)) {
                NetworkAddressChangeListener safeListener = safeCaller
                        .create(listener, NetworkAddressChangeListener.class).withTimeout(15000)
                safeListener.onPrimaryAddressChanged(oldPrimaryAddress, newPrimaryAddress);
    private boolean getConfigParameter(Map<String, Object> parameters, String parameter, boolean defaultValue) {
            return boolean1;
            return Boolean.parseBoolean(string);
     * For all network interfaces (except loopback) all IPv4 addresses are returned.
     * This list can for example, be used to scan the network for available devices.
     * @return A full list of IP {@link InetAddress} (except network and broadcast)
    public static List<InetAddress> getFullRangeOfAddressesToScan() {
        List<InetAddress> addressesToScan = List.of();
        List<CidrAddress> ipV4InterfaceAddresses = NetUtil.getAllInterfaceAddresses().stream()
                .filter(a -> a.getAddress() instanceof Inet4Address).collect(Collectors.toList());
        for (CidrAddress i : ipV4InterfaceAddresses) {
            addressesToScan.addAll(getAddressesRangeByCidrAddress(i, i.getPrefix()));
        return addressesToScan;
     * For the given {@link CidrAddress} all IPv4 addresses are returned.
     * This list can, for example, be used to scan the network for available devices.
     * @param iFaceAddress The {@link CidrAddress} of the network interface
     * @param maxAllowedPrefixLength Control the maximum allowed prefix length of the network (e.g. 24 for class C).
     *            iFaceAddress's with a larger prefix are ignored and return an empty result.
    public static List<InetAddress> getAddressesRangeByCidrAddress(CidrAddress iFaceAddress,
            int maxAllowedPrefixLength) {
        if (!(iFaceAddress.getAddress() instanceof Inet4Address) || iFaceAddress.getPrefix() < maxAllowedPrefixLength) {
        List<byte[]> addresses = getAddressesInSubnet(iFaceAddress.getAddress().getAddress(), iFaceAddress.getPrefix());
        if (addresses.size() > 2) {
            addresses.removeFirst(); // remove network address
            addresses.removeLast(); // remove broadcast address
        return addresses.stream().map(m -> {
                return InetAddress.getByAddress(m);
        }).filter(f -> f != null).sorted((a, b) -> {
            byte[] aOct = a.getAddress();
            byte[] bOct = b.getAddress();
            int r = 0;
            for (int i = 0; i < aOct.length && i < bOct.length; i++) {
                r = Integer.compare(aOct[i] & 0xff, bOct[i] & 0xff);
                if (r != 0) {
     * Calculate each IP address within a subnet
     * @param address IPv4 address in byte array form (i.e. 127.0.0.1 = 01111111 00000000 00000000 00000001)
     * @param maskLength Network mask length (i.e. the number after the forward-slash, '/', in CIDR notation)
     * @return A list of all possible IP addresses in byte array form
    private static List<byte[]> getAddressesInSubnet(byte[] address, int maskLength) {
        byte[] lowestAddress = address.clone();
        for (int bit = maskLength; bit < 32; bit++) {
            lowestAddress[bit / 8] &= ~(1 << (bit % 8));
        int lowestAddressAsLong = ByteBuffer.wrap(lowestAddress).getInt(); // big-endian by default
        byte[] highestAddress = address.clone();
            highestAddress[bit / 8] |= ~(1 << (bit % 8));
        int highestAddressAsLong = ByteBuffer.wrap(highestAddress).getInt();
        List<byte[]> addresses = new ArrayList<byte[]>();
        for (int i = lowestAddressAsLong; i <= highestAddressAsLong; i++) {
            ByteBuffer dbuf = ByteBuffer.allocate(4);
            dbuf.putInt(i);
            addresses.add(dbuf.array());
