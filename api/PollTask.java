 * Poll task represents Modbus read request
 * Must be hashable. HashCode and equals should be defined such that no two poll tasks are registered that are
 * equal.
 * @see ModbusCommunicationInterface#registerRegularPoll
public interface PollTask extends
        TaskWithEndpoint<ModbusReadRequestBlueprint, ModbusReadCallback, ModbusFailureCallback<ModbusReadRequestBlueprint>> {
    default int getMaxTries() {
        return getRequest().getMaxTries();
