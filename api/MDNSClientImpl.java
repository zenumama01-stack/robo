import java.net.Inet6Address;
 * This class starts the JmDNS and implements interface to register and unregister services.
@Component(immediate = true, service = MDNSClient.class)
public class MDNSClientImpl implements MDNSClient, NetworkAddressChangeListener {
    private final Logger logger = LoggerFactory.getLogger(MDNSClientImpl.class);
    private final Map<InetAddress, JmDNS> jmdnsInstances = new ConcurrentHashMap<>();
    private final Set<ServiceDescription> activeServices = ConcurrentHashMap.newKeySet();
    public MDNSClientImpl(final @Reference NetworkAddressService networkAddressService) {
    private Set<InetAddress> getAllInetAddresses() {
        final Set<InetAddress> addresses = new HashSet<>();
        Enumeration<NetworkInterface> itInterfaces;
            itInterfaces = NetworkInterface.getNetworkInterfaces();
        } catch (final SocketException e) {
            return addresses;
        while (itInterfaces.hasMoreElements()) {
            final NetworkInterface iface = itInterfaces.nextElement();
                if (!iface.isUp() || iface.isLoopback() || iface.isPointToPoint()) {
            } catch (final SocketException ex) {
            InetAddress primaryIPv4HostAddress = null;
            if (networkAddressService.isUseOnlyOneAddress()
                    && networkAddressService.getPrimaryIpv4HostAddress() != null) {
                final Enumeration<InetAddress> itAddresses = iface.getInetAddresses();
                while (itAddresses.hasMoreElements()) {
                    final InetAddress address = itAddresses.nextElement();
                    if (address.getHostAddress().equals(networkAddressService.getPrimaryIpv4HostAddress())) {
                        primaryIPv4HostAddress = address;
            boolean ipv4addressAdded = false;
            boolean ipv6addressAdded = false;
                if (address.isLoopbackAddress() || address.isLinkLocalAddress()
                        || (!networkAddressService.isUseIPv6() && address instanceof Inet6Address)) {
                if (networkAddressService.isUseOnlyOneAddress()) {
                    // add only one address per interface and family
                    if (address instanceof Inet4Address) {
                        if (!ipv4addressAdded) {
                            if (primaryIPv4HostAddress != null) {
                                // use configured primary address instead of first one
                                addresses.add(primaryIPv4HostAddress);
                                addresses.add(address);
                            ipv4addressAdded = true;
                    } else if (address instanceof Inet6Address) {
                        if (!ipv6addressAdded) {
                            ipv6addressAdded = true;
    public Set<JmDNS> getClientInstances() {
        return new HashSet<>(jmdnsInstances.values());
        start();
    private void start() {
        for (InetAddress address : getAllInetAddresses()) {
            createJmDNSByAddress(address);
        for (ServiceDescription description : activeServices) {
                registerServiceInternal(description);
                logger.warn("Exception while registering service {}", description, e);
        activeServices.clear();
    public void addServiceListener(String type, ServiceListener listener) {
        jmdnsInstances.values().forEach(jmdns -> jmdns.addServiceListener(type, listener));
    public void removeServiceListener(String type, ServiceListener listener) {
        jmdnsInstances.values().forEach(jmdns -> jmdns.removeServiceListener(type, listener));
    public void registerService(ServiceDescription description) throws IOException {
        activeServices.add(description);
    private void registerServiceInternal(ServiceDescription description) throws IOException {
        for (JmDNS instance : jmdnsInstances.values()) {
            logger.debug("Registering new service {} at {}:{} ({})", description.serviceType,
                    instance.getInetAddress().getHostAddress(), description.servicePort, instance.getName());
            // Create one ServiceInfo object for each JmDNS instance
            ServiceInfo serviceInfo = ServiceInfo.create(description.serviceType, description.serviceName,
                    description.servicePort, 0, 0, description.serviceProperties);
            instance.registerService(serviceInfo);
    public void unregisterService(ServiceDescription description) {
        activeServices.remove(description);
        unregisterServiceInternal(description);
    private void unregisterServiceInternal(ServiceDescription description) {
                logger.debug("Unregistering service {} at {}:{} ({})", description.serviceType,
                logger.debug("Unregistering service {} ({})", description.serviceType, instance.getName());
            instance.unregisterService(serviceInfo);
    public void unregisterAllServices() {
            instance.unregisterAllServices();
    public ServiceInfo[] list(String type) {
        ServiceInfo[] services = new ServiceInfo[0];
            services = concatenate(services, instance.list(type));
    public ServiceInfo[] list(String type, Duration timeout) {
            services = concatenate(services, instance.list(type, timeout.toMillis()));
        for (JmDNS jmdns : jmdnsInstances.values()) {
            closeQuietly(jmdns);
            logger.debug("mDNS service has been stopped ({})", jmdns.getName());
        jmdnsInstances.clear();
    private void closeQuietly(JmDNS jmdns) {
            jmdns.close();
     * Concatenate two arrays of ServiceInfo
     * @param a: the first array
     * @param b: the second array
     * @return an array of ServiceInfo
    private ServiceInfo[] concatenate(ServiceInfo[] a, ServiceInfo[] b) {
        int aLen = a.length;
        int bLen = b.length;
        ServiceInfo[] c = new ServiceInfo[aLen + bLen];
        System.arraycopy(a, 0, c, 0, aLen);
        System.arraycopy(b, 0, c, aLen, bLen);
    private void createJmDNSByAddress(InetAddress address) {
            JmDNS jmdns = JmDNS.create(address, null);
            jmdnsInstances.put(address, jmdns);
            logger.debug("mDNS service has been started ({} for IP {})", jmdns.getName(), address.getHostAddress());
            logger.debug("JmDNS instantiation failed ({})!", address.getHostAddress());
        logger.debug("ip address change: added {}, removed {}", added, removed);
        Set<InetAddress> filteredAddresses = getAllInetAddresses();
        // First check if there is really a jmdns instance to remove or add
        boolean changeRequired = false;
        for (InetAddress address : jmdnsInstances.keySet()) {
            if (!filteredAddresses.contains(address)) {
                changeRequired = true;
        if (!changeRequired) {
            for (InetAddress address : filteredAddresses) {
                JmDNS jmdns = jmdnsInstances.get(address);
                if (jmdns == null) {
            logger.debug("mDNS services already OK for these ip addresses");
                JmDNS jmdns = jmdnsInstances.remove(address);
                if (jmdns != null) {
                    logger.debug("mDNS service has been stopped ({} for IP {})", jmdns.getName(),
                            address.getHostAddress());
                logger.debug("mDNS service was already started ({} for IP {})", jmdns.getName(),
