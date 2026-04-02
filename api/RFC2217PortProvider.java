package org.openhab.core.io.transport.serial.rxtx.rfc2217.internal;
import gnu.io.rfc2217.TelnetSerialPort;
public class RFC2217PortProvider implements SerialPortProvider {
    private static final String PROTOCOL = "rfc2217";
    public @Nullable SerialPortIdentifier getPortIdentifier(URI portName) {
        TelnetSerialPort telnetSerialPort = new TelnetSerialPort();
        telnetSerialPort.setName(portName.toString());
        return new SerialPortIdentifierImpl(telnetSerialPort, portName);
        return Stream.of(new ProtocolType(PathType.NET, PROTOCOL));
