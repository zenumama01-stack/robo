import net.wimpi.modbus.util.SerialParameters;
 * Serial endpoint. Endpoint differentiates different modbus slaves only by the serial port.
 * port.
 * Endpoint contains SerialParameters which should be enough to establish the connection.
public class ModbusSerialSlaveEndpoint implements ModbusSlaveEndpoint {
    private SerialParameters serialParameters;
    public ModbusSerialSlaveEndpoint(String portName, int baudRate, int flowControlIn, int flowControlOut, int databits,
            int stopbits, int parity, String encoding, boolean echo, int receiveTimeoutMillis) {
        this(new SerialParameters(portName, baudRate, flowControlIn, flowControlOut, databits, stopbits, parity,
                encoding, echo, receiveTimeoutMillis));
    public ModbusSerialSlaveEndpoint(String portName, int baudRate, String flowControlIn, String flowControlOut,
            int databits, String stopbits, String parity, String encoding, boolean echo, int receiveTimeoutMillis) {
        SerialParameters parameters = new SerialParameters();
        parameters.setPortName(portName);
        parameters.setBaudRate(baudRate);
        parameters.setFlowControlIn(flowControlIn);
        parameters.setFlowControlOut(flowControlOut);
        parameters.setDatabits(databits);
        parameters.setStopbits(stopbits);
        parameters.setParity(parity);
        parameters.setEncoding(encoding);
        parameters.setEcho(echo);
        parameters.setReceiveTimeoutMillis(receiveTimeoutMillis);
        this.serialParameters = parameters;
    private ModbusSerialSlaveEndpoint(SerialParameters serialParameters) {
        this.serialParameters = serialParameters;
    public SerialParameters getSerialParameters() {
        return serialParameters;
    public <R> R accept(ModbusSlaveEndpointVisitor<R> factory) {
        return factory.visit(this);
    public String getPortName() {
        return serialParameters.getPortName();
        // hashcode & equal is determined purely by port name
        return serialParameters.getPortName().hashCode();
        // equals is determined purely by port name
        ModbusSerialSlaveEndpoint rhs = (ModbusSerialSlaveEndpoint) obj;
        return Objects.equals(getPortName(), rhs.getPortName());
        return "ModbusSerialSlaveEndpoint [getPortName()=" + getPortName() + "]";
