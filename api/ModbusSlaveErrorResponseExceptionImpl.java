import org.openhab.core.io.transport.modbus.exception.ModbusSlaveErrorResponseException;
public class ModbusSlaveErrorResponseExceptionImpl extends ModbusSlaveErrorResponseException {
    private static final long serialVersionUID = 6334580162425192133L;
    private int rawCode;
    private Optional<KnownExceptionCode> exceptionCode;
    public ModbusSlaveErrorResponseExceptionImpl(ModbusSlaveException e) {
        rawCode = e.getType();
        exceptionCode = KnownExceptionCode.tryFromExceptionCode(rawCode);
        return rawCode;
        return String.format("Slave responded with error=%d (%s)", rawCode,
                exceptionCode.map(Enum::name).orElse("unknown error code"));
        return String.format("ModbusSlaveErrorResponseException(error=%d)", rawCode);
