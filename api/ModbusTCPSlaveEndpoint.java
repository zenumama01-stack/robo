 * Endpoint for TCP slaves
public class ModbusTCPSlaveEndpoint extends ModbusIPSlaveEndpoint {
    private boolean rtuEncoded;
    public ModbusTCPSlaveEndpoint(String address, int port, boolean rtuEncoded) {
        super(address, port);
        this.rtuEncoded = rtuEncoded;
    public boolean getRtuEncoded() {
        return rtuEncoded;
