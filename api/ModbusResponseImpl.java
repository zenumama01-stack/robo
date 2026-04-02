import org.openhab.core.io.transport.modbus.ModbusResponse;
import net.wimpi.modbus.msg.ModbusMessage;
 * Basic implementation of {@link ModbusResponse}
public class ModbusResponseImpl implements ModbusResponse {
    public ModbusResponseImpl(ModbusMessage response) {
        this.responseFunctionCode = response.getFunctionCode();
        return responseFunctionCode;
        return String.format("ModbusResponseImpl(responseFC=%d)", responseFunctionCode);
