package org.openhab.core.io.transport.modbus.exception;
 * Exception for connection issues
public class ModbusConnectionException extends ModbusTransportException {
    private static final long serialVersionUID = -6171226761518661925L;
    private ModbusSlaveEndpoint endpoint;
     * @param endpoint endpoint associated with this exception
    public ModbusConnectionException(ModbusSlaveEndpoint endpoint) {
        this.endpoint = endpoint;
     * Get endpoint associated with this connection error
     * @return endpoint with the error
    public ModbusSlaveEndpoint getEndpoint() {
        return String.format("Error connecting to endpoint %s", endpoint);
        return String.format("ModbusConnectionException(Error connecting to endpoint=%s)", endpoint);
