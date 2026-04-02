 * Common base class for ip based endpoints. Endpoint differentiates different modbus slaves only by the ip address
 * (string) and port name.
public abstract class ModbusIPSlaveEndpoint implements ModbusSlaveEndpoint {
    private String address;
    public ModbusIPSlaveEndpoint(String address, int port) {
        this.address = address;
    public String getAddress() {
    public int getPort() {
        // differentiate different protocols using the class name, and after that use address and port
        return Objects.hash(getClass().getName(), address, port);
        return "ModbusIPSlaveEndpoint [address=" + address + ", port=" + port + "]";
            // different protocol -> not equal!
        ModbusIPSlaveEndpoint rhs = (ModbusIPSlaveEndpoint) obj;
        return Objects.equals(address, rhs.address) && port == rhs.port;
