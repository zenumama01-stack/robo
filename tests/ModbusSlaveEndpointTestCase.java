public class ModbusSlaveEndpointTestCase {
    public void testEqualsSameTcp() {
        ModbusTCPSlaveEndpoint e1 = new ModbusTCPSlaveEndpoint("127.0.0.1", 500, false);
        ModbusTCPSlaveEndpoint e2 = new ModbusTCPSlaveEndpoint("127.0.0.1", 500, false);
        assertEquals(e1, e2);
    public void testEqualsSameSerial2() {
        ModbusSerialSlaveEndpoint e1 = new ModbusSerialSlaveEndpoint("port1", 9600, SerialPort.FLOWCONTROL_NONE,
                SerialPort.FLOWCONTROL_NONE, SerialPort.DATABITS_8, SerialPort.STOPBITS_1, SerialPort.PARITY_NONE,
                Modbus.DEFAULT_SERIAL_ENCODING, true, 500);
        ModbusSerialSlaveEndpoint e2 = new ModbusSerialSlaveEndpoint("port1", 9600, SerialPort.FLOWCONTROL_NONE,
     * even though different echo parameter & baud rate, the endpoints are considered the same due to same port
    public void testEqualsSameSerial3() {
                Modbus.DEFAULT_SERIAL_ENCODING, false, 500);
        assertEquals(e1.hashCode(), e2.hashCode());
    public void testEqualsDifferentSerial() {
        ModbusSerialSlaveEndpoint e2 = new ModbusSerialSlaveEndpoint("port2", 9600, SerialPort.FLOWCONTROL_NONE,
        assertNotEquals(e1, e2);
        assertNotEquals(e1.hashCode(), e2.hashCode());
    public void testEqualsDifferentTCPPort() {
        ModbusTCPSlaveEndpoint e2 = new ModbusTCPSlaveEndpoint("127.0.0.1", 501, false);
    public void testEqualsDifferentTCPHost() {
        ModbusTCPSlaveEndpoint e2 = new ModbusTCPSlaveEndpoint("127.0.0.2", 501, false);
    public void testEqualsDifferentProtocol() {
        ModbusUDPSlaveEndpoint e2 = new ModbusUDPSlaveEndpoint("127.0.0.1", 500);
    public void testEqualsDifferentProtocol2() {
     * TCP slaves pointing to same host & port are considered equal even rtu encodinng differs.
     * Thus ensures correct connection pooling and connection sharing
    public void testEqualsSameTcpDifferentEncoding() {
        ModbusTCPSlaveEndpoint e2 = new ModbusTCPSlaveEndpoint("127.0.0.1", 500, true);
