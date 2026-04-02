import org.openhab.core.io.transport.modbus.ModbusWriteCallback;
import org.openhab.core.io.transport.modbus.ModbusWriteRequestBlueprint;
import org.openhab.core.io.transport.modbus.WriteTask;
 * Simple implementation for Modbus write requests
public class BasicWriteTask implements WriteTask {
    private ModbusWriteRequestBlueprint request;
    private ModbusWriteCallback resultCallback;
    private ModbusFailureCallback<ModbusWriteRequestBlueprint> failureCallback;
    public BasicWriteTask(ModbusSlaveEndpoint endpoint, ModbusWriteRequestBlueprint request,
            ModbusWriteCallback resultCallback, ModbusFailureCallback<ModbusWriteRequestBlueprint> failureCallback) {
    public ModbusWriteCallback getResultCallback() {
    public ModbusFailureCallback<ModbusWriteRequestBlueprint> getFailureCallback() {
        return "BasicWriteTask [endpoint=" + endpoint + ", request=" + request + ", resultCallback=" + resultCallback
                + ", failureCallback=" + failureCallback + "]";
