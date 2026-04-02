import java.util.TooManyListenersException;
import javax.comm.SerialPortEvent;
import org.openhab.core.io.transport.serial.SerialPortEventListener;
import org.openhab.core.io.transport.serial.UnsupportedCommOperationException;
 * Specific serial port implementation.
 * @author Kai Kreuzer - added further methods
 * @author Vita Tucek - added further methods
public class SerialPortImpl implements SerialPort {
    private final javax.comm.SerialPort sp;
     * @param sp the underlying serial port implementation
    public SerialPortImpl(final javax.comm.SerialPort sp) {
        this.sp = sp;
        sp.close();
    public void setSerialPortParams(int baudrate, int dataBits, int stopBits, int parity)
            throws UnsupportedCommOperationException {
            sp.setSerialPortParams(baudrate, dataBits, stopBits, parity);
        } catch (javax.comm.UnsupportedCommOperationException ex) {
            throw new UnsupportedCommOperationException(ex);
    public @Nullable InputStream getInputStream() throws IOException {
        return sp.getInputStream();
    public @Nullable OutputStream getOutputStream() throws IOException {
        return sp.getOutputStream();
    public void addEventListener(SerialPortEventListener listener) throws TooManyListenersException {
        sp.addEventListener(new javax.comm.SerialPortEventListener() {
            public void serialEvent(final @Nullable SerialPortEvent event) {
                listener.serialEvent(new SerialPortEventImpl(event));
    public void removeEventListener() {
        sp.removeEventListener();
    public void notifyOnDataAvailable(boolean enable) {
        sp.notifyOnDataAvailable(enable);
    public void notifyOnBreakInterrupt(boolean enable) {
        sp.notifyOnBreakInterrupt(enable);
    public void notifyOnFramingError(boolean enable) {
        sp.notifyOnFramingError(enable);
    public void notifyOnOverrunError(boolean enable) {
        sp.notifyOnOverrunError(enable);
    public void notifyOnParityError(boolean enable) {
        sp.notifyOnParityError(enable);
    public void setRTS(boolean enable) {
        sp.setRTS(enable);
    public void enableReceiveTimeout(int timeout) throws UnsupportedCommOperationException {
            throw new IllegalArgumentException(String.format("timeout must be non negative (is: %d)", timeout));
            sp.enableReceiveTimeout(timeout);
    public void disableReceiveTimeout() {
        sp.disableReceiveTimeout();
        return sp.getName();
    public void setFlowControlMode(int flowcontrolRtsctsOut) throws UnsupportedCommOperationException {
            sp.setFlowControlMode(flowcontrolRtsctsOut);
        } catch (javax.comm.UnsupportedCommOperationException e) {
            throw new UnsupportedCommOperationException(e);
    public void enableReceiveThreshold(int i) throws UnsupportedCommOperationException {
            sp.enableReceiveThreshold(i);
    public int getBaudRate() {
        return sp.getBaudRate();
    public int getDataBits() {
        return sp.getDataBits();
    public int getStopBits() {
        return sp.getStopBits();
    public int getParity() {
        return sp.getParity();
    public void notifyOnOutputEmpty(boolean enable) {
        sp.notifyOnOutputEmpty(enable);
    public void notifyOnCTS(boolean enable) {
        sp.notifyOnCTS(enable);
    public void notifyOnDSR(boolean enable) {
        sp.notifyOnDSR(enable);
    public void notifyOnRingIndicator(boolean enable) {
        sp.notifyOnRingIndicator(enable);
    public void notifyOnCarrierDetect(boolean enable) {
        sp.notifyOnCarrierDetect(enable);
    public int getFlowControlMode() {
        return sp.getFlowControlMode();
    public boolean isRTS() {
        return sp.isRTS();
    public void setDTR(boolean state) {
        sp.setDTR(state);
    public boolean isDTR() {
        return sp.isDTR();
    public boolean isCTS() {
        return sp.isCTS();
    public boolean isDSR() {
        return sp.isDSR();
    public boolean isCD() {
        return sp.isCD();
    public boolean isRI() {
        return sp.isRI();
    public void sendBreak(int duration) {
        sp.sendBreak(duration);
