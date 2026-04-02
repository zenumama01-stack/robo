import org.openhab.core.io.transport.modbus.exception.ModbusSlaveIOException;
public class ModbusSlaveIOExceptionImpl extends ModbusSlaveIOException {
    private static final long serialVersionUID = -8910463902857643468L;
    public ModbusSlaveIOExceptionImpl(ModbusIOException e) {
        this.error = e;
    public ModbusSlaveIOExceptionImpl(IOException e) {
        return String.format("Modbus IO Error with cause=%s, EOF=%s, message='%s', cause2=%s",
                error.getClass().getSimpleName(), error instanceof ModbusIOException mioe ? mioe.isEOF() : "?",
                error.getMessage(), error.getCause());
        return String.format("ModbusSlaveIOException(cause=%s, EOF=%s, message='%s', cause2=%s)",
