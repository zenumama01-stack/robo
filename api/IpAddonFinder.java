package org.openhab.core.config.discovery.addon.ip;
import static org.openhab.core.config.discovery.addon.AddonFinderConstants.*;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetSocketAddress;
import java.net.InterfaceAddress;
import java.net.SocketAddress;
import java.net.SocketTimeoutException;
import java.net.StandardProtocolFamily;
import java.net.StandardSocketOptions;
import java.net.UnknownHostException;
import java.nio.channels.DatagramChannel;
import java.nio.channels.SelectionKey;
import java.nio.channels.Selector;
import java.util.HexFormat;
import java.util.IllegalFormatException;
import org.openhab.core.config.discovery.addon.AddonFinder;
import org.openhab.core.config.discovery.addon.BaseAddonFinder;
import org.openhab.core.net.NetworkAddressChangeListener;
import org.openhab.core.net.NetworkAddressService;
 * This is a {@link IpAddonFinder} for finding suggested add-ons by sending IP packets to the
 * network and collecting responses.
 * This finder is intended to detect devices on the network which do not announce via UPnP
 * or mDNS. Some devices respond to queries to defined multicast addresses and ports and thus
 * can be detected by sending a single frame on the IP network.
 * Be aware of possible side effects of sending packets to unknown devices in the network!
 * This is why the IP finder is not intended for large scale network scanning, e.g. using
 * large port or IP ranges.
 * <strong>Configuration</strong>
 * The following parameters can be used to configure frames to be sent to the network:
 * <table border="1">
 * <td><b>discovery-parameter</b></td>
 * <td><b>values</b></td>
 * <td><b>comment</b></td>
 * <td>{@code type}</td>
 * <td>ipMulticast</td>
 * <td>no other options implemented</td>
 * <td>{@code destIp}</td>
 * <td>destination IP address</td>
 * <td>{@code destPort}</td>
 * <td>destination port</td>
 * <td>{@code listenPort}</td>
 * <td>port to use for listening to responses (optional)</td>
 * <td>privileged ports ({@code <1024}) not allowed</td>
 * <td>{@code request}</td>
 * <td>description of request frame as hex bytes separated by spaces (e.g. 0x01 0x02 ...)</td>
 * <td>dynamic replacement of variables $srcIp, $srcPort and $uuid, no others implemented yet
 * <td>{@code requestPlain}</td>
 * <td>description of request frame as plaintext string</td>
 * <td>dynamic replacement of variables $srcIp, $srcPort and $uuid, no others implemented yet;
 * standard backslash sequences will be translated, and in addition to {@code \}, there are five
 * XML special characters which need to be escaped:
 * <pre>{@code
 * & - &amp;
 * < - &lt;
 * > - &gt;
 * " - &quot;
 * ' - &apos;
 * }</pre>
 * <td>{@code timeoutMs}</td>
 * <td>timeout to wait for a answers</td>
 * <td>{@code fmtMac}</td>
 * <td>format specifier string for mac address</td>
 * <td>e.g. '%02X', '%02X:', '%02x-'</td>
 * <td><b>dynamic replacement</b> (in {@code request*})</td>
 * <td><b>value</b></td>
 * <td>{@code $srcIp}</td>
 * <td>source IP address</td>
 * <td>{@code $srcPort}</td>
 * <td>source port</td>
 * <td>{@code $srcMac}</td>
 * <td>source mac address</td>
 * <td>{@code $uuid}</td>
 * <td>String returned by {@code java.util.UUID.randomUUID()}</td>
 * Packets are sent out on every available network interface.
 * There is currently only one match-property defined: {@code response}.
 * It allows a regex match, but currently only ".*" is supported.
 * <strong>Limitations</strong>
 * The {@link IpAddonFinder} is still under active development.
 * There are limitations:
 * <li>Currently every returned frame is considered as success, regex matching is not implemented.
 * <li>Frames are sent only on startup (or if an {@link org.openhab.core.addon.AddonInfoProvider}
 * calls {@link #setAddonCandidates(List)}), no background scanning.
 * @apiNote The {@link IpAddonFinder} is still under active development, it has initially
 *          been developed to detect KNX installations and will be extended. Configuration parameters
 *          and supported features may still change.
 * @implNote On activation, a thread is spawned which handles the detection. Scan runs once,
 *           no continuous background scanning.
 * @author Holger Friedrich - Initial contribution
 * @author Jacob Laursen - Added support for broadcast-based scanning
@Component(service = AddonFinder.class, name = IpAddonFinder.SERVICE_NAME)
public class IpAddonFinder extends BaseAddonFinder implements NetworkAddressChangeListener {
    public static final String SERVICE_TYPE = SERVICE_TYPE_IP;
    public static final String SERVICE_NAME = SERVICE_NAME_IP;
    private static final String TYPE_IP_BROADCAST = "ipBroadcast";
    private static final String TYPE_IP_MULTICAST = "ipMulticast";
    private static final String MATCH_PROPERTY_RESPONSE = "response";
    private static final String PARAMETER_DEST_IP = "destIp";
    private static final String PARAMETER_DEST_PORT = "destPort";
    private static final String PARAMETER_LISTEN_PORT = "listenPort";
    private static final String PARAMETER_REQUEST = "request";
    private static final String PARAMETER_REQUEST_PLAIN = "requestPlain";
    private static final String PARAMETER_SRC_IP = "srcIp";
    private static final String PARAMETER_SRC_PORT = "srcPort";
    private static final String PARAMETER_SRC_MAC = "srcMac";
    private static final String PARAMETER_MAC_FORMAT = "fmtMac";
    private static final String PARAMETER_TIMEOUT_MS = "timeoutMs";
    private static final String REPLACEMENT_UUID = "uuid";
    private final Logger logger = LoggerFactory.getLogger(IpAddonFinder.class);
    private final NetworkAddressService networkAddressService;
    private final Set<AddonService> addonServices = new CopyOnWriteArraySet<>();
    private @Nullable Future<?> scanJob = null;
    Set<AddonInfo> suggestions = new HashSet<>();
    public IpAddonFinder(final @Reference NetworkAddressService networkAddressService) {
        logger.trace("IpAddonFinder::IpAddonFinder");
        // start of scan will be triggered by setAddonCandidates to ensure addonCandidates are available
        this.networkAddressService = networkAddressService;
        this.networkAddressService.addNetworkAddressChangeListener(this);
        logger.trace("IpAddonFinder::deactivate");
        networkAddressService.removeNetworkAddressChangeListener(this);
        stopScan();
    public void setAddonCandidates(List<AddonInfo> candidates) {
        logger.debug("IpAddonFinder::setAddonCandidates({})", candidates.size());
        super.setAddonCandidates(candidates);
        startScan(20);
    protected void addAddonService(AddonService featureService) {
        this.addonServices.add(featureService);
    protected void removeAddonService(AddonService featureService) {
        this.addonServices.remove(featureService);
    public void onChanged(List<CidrAddress> added, List<CidrAddress> removed) {
        // Nothing to do
    public void onPrimaryAddressChanged(@Nullable String oldPrimaryAddress, @Nullable String newPrimaryAddress) {
        startScan(0);
    private void startScan(long delayInSeconds) {
        // The setAddonCandidates() method is called for each info provider.
        // In order to do the scan only once, but on the full set of candidates, we have to delay the execution.
        // At the same time we must make sure that a scheduled scan is rescheduled - or (after more than our delay) is
        // executed once more.
        logger.trace("Scheduling new IP scan");
        scanJob = scheduler.schedule(this::scan, delayInSeconds, TimeUnit.SECONDS);
    private void stopScan() {
        Future<?> tmpScanJob = scanJob;
        if (tmpScanJob != null) {
            if (!tmpScanJob.isDone()) {
                logger.trace("Trying to cancel IP scan");
                tmpScanJob.cancel(true);
            scanJob = null;
    private void scan() {
        logger.trace("IpAddonFinder::scan started");
        for (AddonInfo candidate : addonCandidates) {
            for (AddonDiscoveryMethod method : candidate.getDiscoveryMethods().stream()
                    .filter(method -> SERVICE_TYPE.equals(method.getServiceType())).toList()) {
                logger.trace("Checking candidate: {}", candidate.getUID());
                // skip scanning if already installed
                if (isAddonInstalled(candidate.getUID())) {
                    logger.trace("Skipping {}, already installed", candidate.getUID());
                Map<String, String> parameters = method.getParameters().stream()
                        .collect(Collectors.toMap(AddonParameter::getName, AddonParameter::getValue));
                Map<String, String> matchProperties = method.getMatchProperties().stream()
                        .collect(Collectors.toMap(AddonMatchProperty::getName, AddonMatchProperty::getRegex));
                // parse standard set of parameters
                String type = Objects.toString(parameters.get("type"), "");
                String request = Objects.requireNonNull(Objects.toString(parameters.get(PARAMETER_REQUEST), ""));
                String requestPlain = Objects
                        .requireNonNull(Objects.toString(parameters.get(PARAMETER_REQUEST_PLAIN), ""));
                // xor
                if (!("".equals(request) ^ "".equals(requestPlain))) {
                    logger.warn("{}: discovery-parameter '{}' or '{}' required", candidate.getUID(), PARAMETER_REQUEST,
                            PARAMETER_REQUEST_PLAIN);
                String response = Objects
                        .requireNonNull(Objects.toString(matchProperties.get(MATCH_PROPERTY_RESPONSE), ""));
                int timeoutMs;
                    timeoutMs = Integer.parseInt(Objects.toString(parameters.get(PARAMETER_TIMEOUT_MS)));
                    logger.warn("{}: discovery-parameter '{}' cannot be parsed", candidate.getUID(),
                            PARAMETER_TIMEOUT_MS);
                InetAddress destIp;
                    destIp = InetAddress.getByName(parameters.get(PARAMETER_DEST_IP));
                } catch (UnknownHostException e) {
                    logger.warn("{}: discovery-parameter '{}' cannot be parsed", candidate.getUID(), PARAMETER_DEST_IP);
                int destPort;
                    destPort = Integer.parseInt(Objects.toString(parameters.get(PARAMETER_DEST_PORT)));
                            PARAMETER_DEST_PORT);
                int listenPort = 0; // default, pick a non-privileged port
                if (parameters.get(PARAMETER_LISTEN_PORT) != null) {
                        listenPort = Integer.parseInt(Objects.toString(parameters.get(PARAMETER_LISTEN_PORT)));
                                PARAMETER_LISTEN_PORT);
                    // do not allow privileged ports
                    if (listenPort < 1024) {
                        logger.warn("{}: discovery-parameter '{}' not allowed, privileged port", candidate.getUID(),
                String macFormat = parameters.getOrDefault(PARAMETER_MAC_FORMAT, "%02X:");
                if (!macFormatValid(macFormat)) {
                    logger.warn("{}: discovery-parameter '{}' invalid format specifier", candidate.getUID(), macFormat);
                // handle known types
                    switch (Objects.toString(type)) {
                        case TYPE_IP_BROADCAST:
                            scanBroadcast(candidate, request, requestPlain, response, timeoutMs, destPort, macFormat);
                        case TYPE_IP_MULTICAST:
                            scanMulticast(candidate, request, requestPlain, response, timeoutMs, listenPort, destIp,
                                    destPort, macFormat);
                            logger.warn("{}: discovery-parameter type \"{}\" is unknown", candidate.getUID(), type);
                } catch (ParseException | NumberFormatException none) {
        logger.trace("IpAddonFinder::scan completed");
    private void scanBroadcast(AddonInfo candidate, String request, String requestPlain, String response, int timeoutMs,
            int destPort, String macFormat) throws ParseException {
        if (request.isEmpty() && requestPlain.isEmpty()) {
            logger.warn("{}: match-property request and requestPlain \"{}\" is unknown", candidate.getUID(),
                    TYPE_IP_BROADCAST);
        if (!request.isEmpty() && !requestPlain.isEmpty()) {
            logger.warn("{}: match-properties request and requestPlain \"{}\" are both present", candidate.getUID(),
        if (response.isEmpty()) {
            logger.warn("{}: match-property response \"{}\" is unknown", candidate.getUID(), TYPE_IP_BROADCAST);
        String broadcastAddress = networkAddressService.getConfiguredBroadcastAddress();
        if (broadcastAddress == null || broadcastAddress.isBlank()) {
            logger.debug("Unable to resolve broadcast address");
        InetAddress bAddr;
            bAddr = InetAddress.getByName(broadcastAddress);
            logger.debug("Unable to resolve broadcast address: {}", e.getMessage());
        InterfaceAddress sourceAddress = NetUtil.getSameSubnetInterfaceAddress(bAddr);
        if (sourceAddress == null) {
            logger.debug("Unable to find a suitable interface address for broadcast address \"{}\"", broadcastAddress);
        logger.debug("Starting broadcast scan with address {}", broadcastAddress);
        try (DatagramSocket socket = new DatagramSocket(0, sourceAddress.getAddress())) {
            socket.setBroadcast(true);
            socket.setSoTimeout(timeoutMs);
            byte[] sendBuffer = requestPlain.isEmpty() ? buildRequestArray(socket.getLocalSocketAddress(), request)
                    : buildRequestArrayPlain(socket.getLocalSocketAddress(), requestPlain, macFormat);
            DatagramPacket sendPacket = new DatagramPacket(sendBuffer, sendBuffer.length,
                    InetAddress.getByName(broadcastAddress), destPort);
            socket.send(sendPacket);
            // wait for responses
            while (!Thread.currentThread().isInterrupted()) {
                byte[] discoverReceive = buildByteArray(response);
                byte[] receiveBuffer = new byte[discoverReceive.length];
                DatagramPacket receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);
                    socket.receive(receivePacket);
                } catch (SocketTimeoutException e) {
                    break; // leave the endless loop
                byte[] data = receivePacket.getData();
                if (Arrays.equals(data, discoverReceive)) {
                    suggestions.add(candidate);
                    logger.debug("Suggested add-on found: {}", candidate.getUID());
            logger.debug("{}: network error", candidate.getUID(), e);
    private byte[] buildByteArray(String input) {
        ByteArrayOutputStream requestFrame = new ByteArrayOutputStream();
        StringTokenizer parts = new StringTokenizer(input);
        while (parts.hasMoreTokens()) {
            String token = parts.nextToken();
            int i = Integer.decode(token);
            requestFrame.write((byte) i);
        return requestFrame.toByteArray();
    private void scanMulticast(AddonInfo candidate, String request, String requestPlain, String response, int timeoutMs,
            int listenPort, @Nullable InetAddress destIp, int destPort, String macFormat) throws ParseException {
        List<String> ipAddresses = NetUtil.getAllInterfaceAddresses().stream()
                .filter(a -> a.getAddress() instanceof Inet4Address).map(a -> a.getAddress().getHostAddress()).toList();
        for (String localIp : ipAddresses) {
            try (DatagramChannel channel = (DatagramChannel) DatagramChannel.open(StandardProtocolFamily.INET)
                    .setOption(StandardSocketOptions.SO_REUSEADDR, true)
                    .bind(new InetSocketAddress(localIp, listenPort))
                    .setOption(StandardSocketOptions.IP_MULTICAST_TTL, 64).configureBlocking(false);
                    Selector selector = Selector.open()) {
                byte[] requestArray = "".equals(requestPlain)
                        ? buildRequestArray(channel.getLocalAddress(), Objects.toString(request))
                        : buildRequestArrayPlain(channel.getLocalAddress(), Objects.toString(requestPlain), macFormat);
                if (logger.isTraceEnabled()) {
                    InetSocketAddress sock = (InetSocketAddress) channel.getLocalAddress();
                    String id = candidate.getUID();
                    logger.trace("{}: probing {} -> {}:{}", id, localIp, destIp != null ? destIp.getHostAddress() : "",
                            destPort);
                    if (!"".equals(requestPlain)) {
                        logger.trace("{}: \'{}\'", id, new String(requestArray));
                    logger.trace("{}: {}", id, HexFormat.of().withDelimiter(" ").formatHex(requestArray));
                    logger.trace("{}: listening on {}:{} for {} ms", id, sock.getAddress().getHostAddress(),
                            sock.getPort(), timeoutMs);
                channel.send(ByteBuffer.wrap(requestArray), new InetSocketAddress(destIp, destPort));
                // listen to responses
                ByteBuffer buffer = ByteBuffer.wrap(new byte[50]);
                channel.register(selector, SelectionKey.OP_READ);
                selector.select(timeoutMs);
                Iterator<SelectionKey> it = selector.selectedKeys().iterator();
                switch (Objects.toString(response)) {
                    case ".*":
                        if (it.hasNext()) {
                            final SocketAddress source = ((DatagramChannel) it.next().channel()).receive(buffer);
                            logger.debug("Received return frame from {}",
                                    ((InetSocketAddress) source).getAddress().getHostAddress());
                            logger.trace("{}: no response received on {}", candidate.getUID(), localIp);
                        logger.warn("{}: match-property response \"{}\" is unknown", candidate.getUID(),
                                TYPE_IP_MULTICAST);
                        break; // end loop
    // build from plaintext string
    private byte[] buildRequestArrayPlain(SocketAddress address, String request, String macFormat)
            throws java.io.IOException, ParseException {
        InetSocketAddress sock = (InetSocketAddress) address;
        // replace first
        StringBuilder req = new StringBuilder(request);
        while ((p = req.indexOf("$" + PARAMETER_SRC_IP)) != -1) {
            req.replace(p, p + PARAMETER_SRC_IP.length() + 1, sock.getAddress().getHostAddress());
        while ((p = req.indexOf("$" + PARAMETER_SRC_PORT)) != -1) {
            req.replace(p, p + PARAMETER_SRC_PORT.length() + 1, "" + sock.getPort());
        while ((p = req.indexOf("$" + PARAMETER_SRC_MAC)) != -1) {
            req.replace(p, p + PARAMETER_SRC_MAC.length() + 1, macFormat(macFormat, macBytesFrom(sock)));
        while ((p = req.indexOf("$" + REPLACEMENT_UUID)) != -1) {
            req.replace(p, p + REPLACEMENT_UUID.length() + 1, UUID.randomUUID().toString());
        String reqUnEscaped = StringUtils.unEscapeXml(req.toString());
        return reqUnEscaped != null ? reqUnEscaped.translateEscapes().getBytes() : new byte[0];
    // build from hex string
    private byte[] buildRequestArray(SocketAddress address, String request) throws java.io.IOException, ParseException {
        StringTokenizer parts = new StringTokenizer(request);
            if (token.startsWith("$")) {
                switch (token) {
                    case "$" + PARAMETER_SRC_IP:
                        byte[] adr = sock.getAddress().getAddress();
                        requestFrame.write(adr);
                    case "$" + PARAMETER_SRC_PORT:
                        int dPort = sock.getPort();
                        requestFrame.write((byte) ((dPort >> 8) & 0xff));
                        requestFrame.write((byte) (dPort & 0xff));
                    case "$" + PARAMETER_SRC_MAC:
                        byte[] mac = macBytesFrom(sock);
                        requestFrame.write(mac);
                    case "$" + REPLACEMENT_UUID:
                        String uuid = UUID.randomUUID().toString();
                        requestFrame.write(uuid.getBytes());
                        logger.warn("Unknown token in request frame \"{}\"", token);
                        throw new ParseException(token, 0);
    public Set<AddonInfo> getSuggestedAddons() {
        logger.trace("IpAddonFinder::getSuggestedAddons {}/{}", suggestions.size(), addonCandidates.size());
    public String getServiceName() {
    private boolean isAddonInstalled(String addonId) {
        for (AddonService addonService : addonServices) {
            Addon addon = addonService.getAddon(addonId, null);
            if (addon != null && addon.isInstalled()) {
     * Get mac address bytes associated with the given Internet socket address
     * @param inetSocketAddress the Internet address
     * @return the mac address as an array of bytes
     * @throws SocketException if address is not on this PC, or no mac address is associated
    private byte[] macBytesFrom(InetSocketAddress inetSocketAddress) throws SocketException {
        NetworkInterface networkInterface = NetworkInterface.getByInetAddress(inetSocketAddress.getAddress());
        if (networkInterface == null) {
            throw new SocketException("No network interface");
        return networkInterface.getHardwareAddress();
     * Use the given format specifier to format an array of mac address bytes
     * @param format a standard format specifier; optionally ends with a delimiter e.g. {@code %02x:} or {@code %02X}
     * @param bytes the mac address as an array of bytes
     * @return e.g. '{@code 01:02:03:04:A5:B6:C7:D8}' or '{@code 01-02-03-04-a5-b6-c7-d8}' or '{@code 01020304A5B6C7D8}'
    private String macFormat(String format, byte[] bytes) {
        for (byte byt : bytes) {
            result.append(String.format(format, byt));
        boolean isDelimited = !Character.isLetterOrDigit(format.charAt(format.length() - 1));
        return (isDelimited ? result.substring(0, result.length() - 1) : result).toString();
     * Check if the given mac format specifier is valid. A valid specifier comprises two parts -- namely
     * 1) a numeric format specifier acceptable to the {@code String.format()} method, plus 2) a single
     * [optional] delimiter (i.e. a non alphanumeric) character. Examples are as follows:
     * <li>{@code %02X} produces {@code 01020304A5B6C7D8}</li>
     * <li>{@code %02x:} produces {@code 01:02:03:04:a5:b6:c7:d8} (lower case hex)</li>
     * <li>{@code %02X-} produces {@code 01-02-03-04-A5-B6-C7-D8} (upper case hex)</li>
     * <li>{@code %02X,} produces {@code 01,02,03,04,A5,B6,C7,D8}</li>
     * @return true if the format specifier is valid
    private boolean macFormatValid(String format) {
        // use String.format() to check first part validity
            String.format(format, (byte) 123);
        } catch (IllegalFormatException e) {
        // get position of numeric format letter e.g. the 'X' in '%02X-'
        int last = format.length() - 1;
        while (index <= last) {
            if (Character.isLetter(format.charAt(index))) {
        // check for zero or one character(s) after numeric format letter
        switch (last - index) {
                // check this character is non alphanumeric i.e. a delimiter
                return !Character.isLetterOrDigit(format.charAt(last));
