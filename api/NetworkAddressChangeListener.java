 * This is an interface for listeners who wants to be notified for the change of network address.
 * There are only network address adds, and removes; it makes no effort to correlate
 * which existing network is changed. Listeners should register themselves at the {@link NetworkAddressService}.
 * @see NetUtil
public interface NetworkAddressChangeListener {
     * When network address is changed, listeners will be notified by this method.
     * When a network interface changes from "up" to "down", it is considered as "removed".
     * When a "loopback" or "down" interface is added, the listeners are not notified.
     * @param added Unmodifiable list of recently added network addresses
     * @param removed Unmodifiable list of recently removed network addresses
    void onChanged(List<CidrAddress> added, List<CidrAddress> removed);
     * When the primary address is changed, listeners will be notified by this method. The primary address
     * will be in the form of a subnet ("xxx.xxx.xxx.xxx/yyy").
     * @param oldPrimaryAddress The old primary address (may be null if none specified)
     * @param newPrimaryAddress The new primary address (may be null if none specified)
    default void onPrimaryAddressChanged(@Nullable String oldPrimaryAddress, @Nullable String newPrimaryAddress) {
        // nothing to do for the default implementation
