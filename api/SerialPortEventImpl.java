import org.openhab.core.io.transport.serial.SerialPortEvent;
 * Specific serial port event implementation.
public class SerialPortEventImpl implements SerialPortEvent {
    private final javax.comm.SerialPortEvent event;
     * @param event the underlying event implementation
    public SerialPortEventImpl(final javax.comm.SerialPortEvent event) {
        this.event = event;
    public int getEventType() {
        return event.getEventType();
    public boolean getNewValue() {
        return event.getNewValue();
    private final gnu.io.SerialPortEvent event;
    public SerialPortEventImpl(final gnu.io.SerialPortEvent event) {
