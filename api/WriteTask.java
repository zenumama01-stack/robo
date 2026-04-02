 * Poll task represents Modbus write request
 * Unlike {@link PollTask}, this does not have to be hashable.
public interface WriteTask extends
        TaskWithEndpoint<ModbusWriteRequestBlueprint, ModbusWriteCallback, ModbusFailureCallback<ModbusWriteRequestBlueprint>> {
