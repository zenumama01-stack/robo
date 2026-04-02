 * Interface that provides access to configured network addresses
public interface NetworkAddressService {
     * Returns the user configured primary IPv4 address of the system
     * @return IPv4 address as a String in format xxx.xxx.xxx.xxx or
     *         <code>null</code> if there is no interface or an error occurred
    String getPrimaryIpv4HostAddress();
     * Returns the user configured broadcast address, or the broadcast address of the user configured primary IPv4 if
     * not provided
     * @return IPv4 broadcast address as a String in format xxx.xxx.xxx or
     *         <code>null</code> if no broadcast address is found or an error occurred
    String getConfiguredBroadcastAddress();
    boolean isUseOnlyOneAddress();
    boolean isUseIPv6();
     * Adds a {@link NetworkAddressChangeListener} that is notified about changes.
     * @param listener The listener
    void addNetworkAddressChangeListener(NetworkAddressChangeListener listener);
     * Removes a {@link NetworkAddressChangeListener} so that it is no longer notified about changes.
    void removeNetworkAddressChangeListener(NetworkAddressChangeListener listener);
