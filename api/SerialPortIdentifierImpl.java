import javax.comm.CommPort;
import org.openhab.core.io.transport.serial.PortInUseException;
import org.openhab.core.io.transport.serial.SerialPort;
 * Specific serial port identifier implementation.
public class SerialPortIdentifierImpl implements SerialPortIdentifier {
    final CommPortIdentifier id;
     * @param id the underlying comm port identifier implementation
    public SerialPortIdentifierImpl(final CommPortIdentifier id) {
        final String name = id.getName();
        return name != null ? name : "";
    public SerialPort open(String owner, int timeout) throws PortInUseException {
            final CommPort cp = id.open(owner, timeout);
            if (cp instanceof javax.comm.SerialPort port) {
                return new SerialPortImpl(port);
                        String.format("We expect a serial port instead of '%s'", cp.getClass()));
        } catch (javax.comm.PortInUseException e) {
                throw new PortInUseException(message, e);
                throw new PortInUseException(e);
    public boolean isCurrentlyOwned() {
        return id.isCurrentlyOwned();
    public @Nullable String getCurrentOwner() {
        return id.getCurrentOwner();
import org.openhab.core.io.transport.serial.rxtx.RxTxSerialPort;
 * Specific serial port identifier implementation for RFC2217.
    final TelnetSerialPort id;
    public SerialPortIdentifierImpl(final TelnetSerialPort id, URI uri) {
            id.getTelnetClient().setConnectTimeout(timeout);
            id.getTelnetClient().connect(uri.getHost(), uri.getPort());
            return new RxTxSerialPort(id);
                    String.format("Unable to establish remote connection to serial port %s", uri), e);
        // Check if the socket is not available for use, if true interpret as being owned.
        return !id.getTelnetClient().isAvailable();
        // Unknown who owns a socket connection. Therefore return null.
import gnu.io.CommPort;
            if (cp instanceof gnu.io.SerialPort port) {
                return new RxTxSerialPort(port);
        } catch (gnu.io.PortInUseException e) {
