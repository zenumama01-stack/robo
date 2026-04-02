package org.openhab.core.config.core.internal.net;
import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import org.openhab.core.net.CidrAddress;
import org.openhab.core.net.NetUtil;
import org.openhab.core.util.StringUtils;
 * Provides a list of IPv4 addresses of the local machine and shows the user which interface belongs to which IP address
public class NetworkConfigOptionProvider implements ConfigOptionProvider {
    static final URI CONFIG_URI = URI.create("system:network");
    static final String PARAM_PRIMARY_ADDRESS = "primaryAddress";
    static final String PARAM_BROADCAST_ADDRESS = "broadcastAddress";
    static final String NETWORK_INTERFACE = "network-interface";
        if (CONFIG_URI.equals(uri)) {
                case PARAM_PRIMARY_ADDRESS:
                    Stream<CidrAddress> ipv4Addresses = NetUtil.getAllInterfaceAddresses().stream()
                            .filter(a -> a.getAddress() instanceof Inet4Address);
                    return ipv4Addresses.map(a -> new ParameterOption(a.toString(), a.toString())).toList();
                case PARAM_BROADCAST_ADDRESS:
                    List<String> broadcastAddrList = new ArrayList<>(NetUtil.getAllBroadcastAddresses());
                    broadcastAddrList.add("255.255.255.255");
                    return broadcastAddrList.stream().distinct().map(a -> new ParameterOption(a, a)).toList();
        } else if (NETWORK_INTERFACE.equals(context)) {
                Enumeration<NetworkInterface> networkInterfaces = NetworkInterface.getNetworkInterfaces();
                while (networkInterfaces.hasMoreElements()) {
                    NetworkInterface networkInterface = networkInterfaces.nextElement();
                    if (networkInterface.isUp()) {
                        options.add(new ParameterOption(networkInterface.getName(),
                                getNetworkInterfaceLabel(networkInterface)));
            } catch (SocketException e) {
    private String getNetworkInterfaceLabel(NetworkInterface networkInterface) {
        StringBuilder result = new StringBuilder(Objects
                .requireNonNull(StringUtils.capitalizeByWhitespace(networkInterface.getName().replace('_', ' '))));
        // Sort IPv4 before IPv6
        List<InetAddress> addresses = networkInterface.inetAddresses().sorted((ia1, ia2) -> {
            return (ia1 instanceof Inet4Address) == (ia2 instanceof Inet4Address) ? 0
                    : ia1 instanceof Inet4Address ? -1 : 1;
        if (!addresses.isEmpty()) {
            result.append(" (").append(addresses.getFirst().getHostAddress()).append(')');
        return result.toString();
