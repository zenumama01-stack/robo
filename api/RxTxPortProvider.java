import gnu.io.CommPortIdentifier;
import gnu.io.NoSuchPortException;
 * @author Wouter Born - Fix serial ports missing when ports are added to system property
 * @author Gwendal Roulleau - Workaround for long path issue by resolving symlink
public class RxTxPortProvider implements SerialPortProvider {
    private final Logger logger = LoggerFactory.getLogger(RxTxPortProvider.class);
        String portPathAsString = port.getPath();
            // Resolving symbolic link is needed because of a bug with nrjavaserial
            // Until a new release with pull request #230 is included in openHAB,
            // we keep resolving symbolic link here
            Path portPath = Path.of(portPathAsString);
            if (Files.isSymbolicLink(portPath)) {
                portPathAsString = portPath.toRealPath().toString();
            CommPortIdentifier ident = SerialPortUtil.getPortIdentifier(portPathAsString);
        } catch (NoSuchPortException | IOException e) {
            logger.debug("No SerialPortIdentifier found for: {}", portPathAsString, e);
        return Stream.of(new ProtocolType(PathType.LOCAL, "rxtx"));
        Stream<CommPortIdentifier> scanIds = SerialPortUtil.getPortIdentifiersUsingScan();
        Stream<CommPortIdentifier> propIds = SerialPortUtil.getPortIdentifiersUsingProperty();
        return Stream.concat(scanIds, propIds).filter(distinctByKey(CommPortIdentifier::getName))
    private static <T> Predicate<T> distinctByKey(Function<? super T, Object> keyExtractor) {
        Map<Object, String> seen = new ConcurrentHashMap<>();
        return t -> seen.put(keyExtractor.apply(t), "") == null;
