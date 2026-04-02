package org.openhab.core.net;
 * The CIDR (Class-less interdomain routing) notation is an IP address
 * and additionally ends with a slash followed by the network prefix length number.
 * The toString() method will return a CIRDR representation, but the individual
 * address and prefix length can be accessed as well.
 * Java has a class that exactly provides this {@link java.net.InterfaceAddress}, but unfortunately
 * no public constructor exists.
public class CidrAddress {
    private final InetAddress address;
    public CidrAddress(InetAddress address, short networkPrefixLength) {
        this.prefix = networkPrefixLength;
        if (prefix == 0) {
            return address.getHostAddress();
            return address.getHostAddress() + "/" + prefix;
        if (!(o instanceof CidrAddress)) {
        CidrAddress c = (CidrAddress) o;
        return c.getAddress().equals(getAddress()) && c.getPrefix() == getPrefix();
        return Objects.hash(getAddress().hashCode(), getPrefix());
    public int getPrefix() {
    public InetAddress getAddress() {
