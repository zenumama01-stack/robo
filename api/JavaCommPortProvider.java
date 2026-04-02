package org.openhab.core.io.transport.serial.internal;
import java.util.Spliterator;
import java.util.Spliterators;
import javax.comm.CommPortIdentifier;
import org.openhab.core.io.transport.serial.ProtocolType;
import org.openhab.core.io.transport.serial.ProtocolType.PathType;
import org.openhab.core.io.transport.serial.SerialPortProvider;
 * @author Matthias Steigenberger - Initial contribution
@Component(service = SerialPortProvider.class)
public class JavaCommPortProvider implements SerialPortProvider {
    private final Logger logger = LoggerFactory.getLogger(JavaCommPortProvider.class);
    public @Nullable SerialPortIdentifier getPortIdentifier(URI port) {
        CommPortIdentifier ident;
            ident = CommPortIdentifier.getPortIdentifier(port.getPath());
        } catch (javax.comm.NoSuchPortException e) {
            logger.debug("No SerialPortIdentifier found for: {}", port.getPath());
        return new SerialPortIdentifierImpl(ident);
    public Stream<ProtocolType> getAcceptedProtocols() {
        return Stream.of(new ProtocolType(PathType.LOCAL, "javacomm"));
    public Stream<SerialPortIdentifier> getSerialPortIdentifiers() {
        final Enumeration<CommPortIdentifier> ids = CommPortIdentifier.getPortIdentifiers();
        return StreamSupport.stream(new SplitIteratorForEnumeration<>(ids), false)
                .filter(id -> id.getPortType() == CommPortIdentifier.PORT_SERIAL)
                .map(sid -> new SerialPortIdentifierImpl(sid));
    private static class SplitIteratorForEnumeration<T> extends Spliterators.AbstractSpliterator<T> {
        private final Enumeration<T> e;
        public SplitIteratorForEnumeration(final Enumeration<T> e) {
            super(Long.MAX_VALUE, Spliterator.ORDERED);
            this.e = e;
        public boolean tryAdvance(Consumer<? super T> action) {
            if (e.hasMoreElements()) {
                action.accept(e.nextElement());
        public void forEachRemaining(Consumer<? super T> action) {
            while (e.hasMoreElements()) {
