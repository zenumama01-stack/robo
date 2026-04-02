import org.openhab.core.io.transport.modbus.endpoint.EndpointPoolConfiguration;
 * ModbusManager is the main interface for interacting with Modbus slaves
public interface ModbusManager {
     * Open communication interface to endpoint
     * @param endpoint endpoint pointing to modbus slave
     * @param configuration configuration for the endpoint. Use null to use default pool configuration
     * @return Communication interface for interacting with the slave
     * @throws IllegalArgumentException if there is already open communication interface with same endpoint but
     *             differing configuration
    ModbusCommunicationInterface newModbusCommunicationInterface(ModbusSlaveEndpoint endpoint,
            @Nullable EndpointPoolConfiguration configuration) throws IllegalArgumentException;
     * Get general configuration settings applied to a given endpoint
     * Note that default configuration settings are returned in case the endpoint has not been configured.
     * @param endpoint endpoint to query
     * @return general connection settings of the given endpoint
    EndpointPoolConfiguration getEndpointPoolConfiguration(ModbusSlaveEndpoint endpoint);
