import org.openhab.core.io.transport.modbus.internal.ModbusSlaveErrorResponseExceptionImpl;
public class ModbusSlaveErrorResponseExceptionImplTest {
    public void testKnownCode1() {
        assertEquals("Slave responded with error=1 (ILLEGAL_FUNCTION)",
                new ModbusSlaveErrorResponseExceptionImpl(new ModbusSlaveException(1)).getMessage());
    public void testKnownCode2() {
        assertEquals("Slave responded with error=2 (ILLEGAL_DATA_ACCESS)",
                new ModbusSlaveErrorResponseExceptionImpl(new ModbusSlaveException(2)).getMessage());
    public void testUnknownCode() {
        assertEquals("Slave responded with error=99 (unknown error code)",
                new ModbusSlaveErrorResponseExceptionImpl(new ModbusSlaveException(99)).getMessage());
