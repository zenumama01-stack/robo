 * Visitor for ModbusSlaveEndpoint
 * @param <R> return type from visit
public interface ModbusSlaveEndpointVisitor<R> {
    R visit(ModbusTCPSlaveEndpoint endpoint);
    R visit(ModbusSerialSlaveEndpoint endpoint);
    R visit(ModbusUDPSlaveEndpoint endpoint);
