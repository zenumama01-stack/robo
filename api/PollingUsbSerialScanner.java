import static java.lang.Long.parseLong;
import org.openhab.core.config.discovery.usbserial.linuxsysfs.internal.DeltaUsbSerialScanner.Delta;
 * A {@link UsbSerialDiscovery} that implements background discovery by doing repetitive scans using a
 * {@link UsbSerialScanner}, pausing a configurable amount of time between subsequent scans.
@Component(configurationPid = "discovery.usbserial.linuxsysfs.pollingscanner")
public class PollingUsbSerialScanner implements UsbSerialDiscovery {
    private final Logger logger = LoggerFactory.getLogger(PollingUsbSerialScanner.class);
    private static final String THREAD_NAME = "usb-serial-discovery-linux-sysfs";
    public static final String PAUSE_BETWEEN_SCANS_IN_SECONDS_ATTRIBUTE = "pauseBetweenScansInSeconds";
    private static final Duration DEFAULT_PAUSE_BETWEEN_SCANS = Duration.ofSeconds(15);
    private Duration pauseBetweenScans = DEFAULT_PAUSE_BETWEEN_SCANS;
    private final DeltaUsbSerialScanner deltaUsbSerialScanner;
    private final Set<UsbSerialDiscoveryListener> discoveryListeners = new CopyOnWriteArraySet<>();
    private @Nullable ScheduledFuture<?> backgroundScanningJob;
    public PollingUsbSerialScanner(Map<String, Object> config, final @Reference UsbSerialScanner usbSerialScanner) {
        if (config.containsKey(PAUSE_BETWEEN_SCANS_IN_SECONDS_ATTRIBUTE)) {
            pauseBetweenScans = Duration
                    .ofSeconds(parseLong(config.get(PAUSE_BETWEEN_SCANS_IN_SECONDS_ATTRIBUTE).toString()));
        deltaUsbSerialScanner = new DeltaUsbSerialScanner(usbSerialScanner);
        scheduler = Executors.newSingleThreadScheduledExecutor(
                ThreadFactoryBuilder.create().withName(THREAD_NAME).withDaemonThreads(true).build());
        scheduler.shutdown();
    protected synchronized void modified(Map<String, Object> config) {
            if (backgroundScanningJob != null) {
                stopBackgroundScanning();
                startBackgroundScanning();
     * Performs a single scan for newly added and removed devices.
    public void doSingleScan() {
        singleScanInternal(true);
     * Starts repeatedly scanning for newly added and removed USB devices using the configured {@link UsbSerialScanner}
     * (where the duration between two subsequent scans is configurable).
     * This repeated scanning can be stopped using {@link #stopBackgroundScanning()}.
    public synchronized void startBackgroundScanning() {
        if (backgroundScanningJob == null) {
            if (deltaUsbSerialScanner.canPerformScans()) {
                backgroundScanningJob = scheduler.scheduleWithFixedDelay(() -> {
                    singleScanInternal(false);
                }, 0, pauseBetweenScans.getSeconds(), TimeUnit.SECONDS);
                logger.debug("Scheduled USB-Serial background discovery every {} seconds",
                        pauseBetweenScans.getSeconds());
                        "Do not start background scanning, as the configured USB-Serial scanner cannot perform scans on this system");
     * Stops repeatedly scanning for newly added and removed USB devices. This can be restarted using
     * {@link #startBackgroundScanning()}.
    public synchronized void stopBackgroundScanning() {
        logger.debug("Stopping USB-Serial background discovery");
        ScheduledFuture<?> currentBackgroundScanningJob = backgroundScanningJob;
        if (currentBackgroundScanningJob != null && !currentBackgroundScanningJob.isCancelled()) {
            if (currentBackgroundScanningJob.cancel(true)) {
                backgroundScanningJob = null;
                logger.debug("Stopped USB-serial background discovery");
    public void registerDiscoveryListener(UsbSerialDiscoveryListener listener) {
        discoveryListeners.add(listener);
        for (UsbSerialDeviceInformation deviceInfo : deltaUsbSerialScanner.getLastScanResult()) {
            listener.usbSerialDeviceDiscovered(deviceInfo);
    public void unregisterDiscoveryListener(UsbSerialDiscoveryListener listener) {
        discoveryListeners.remove(listener);
    private void singleScanInternal(boolean announceUnchangedDevices) {
            Delta<UsbSerialDeviceInformation> delta = deltaUsbSerialScanner.scan();
            announceAddedDevices(delta.getAdded());
            announceRemovedDevices(delta.getRemoved());
            if (announceUnchangedDevices) {
                announceAddedDevices(delta.getUnchanged());
            logger.debug("A {} prevented a scan for USB serial devices: {}", e.getClass().getSimpleName(),
    private void announceAddedDevices(Set<UsbSerialDeviceInformation> deviceInfos) {
        for (UsbSerialDeviceInformation deviceInfo : deviceInfos) {
            for (UsbSerialDiscoveryListener listener : discoveryListeners) {
    private void announceRemovedDevices(Set<UsbSerialDeviceInformation> deviceInfos) {
                listener.usbSerialDeviceRemoved(deviceInfo);
