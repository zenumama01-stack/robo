import org.openhab.core.config.discovery.usbserial.internal.UsbSerialDiscoveryService;
 * Interface for implementations for discovering serial ports provided by a USB device. An implementation of this
 * interface is required by the {@link UsbSerialDiscoveryService}.
public interface UsbSerialDiscovery {
     * Executes a single scan for serial ports provided by USB devices; informs listeners about all discovered devices
     * (including those discovered in a previous scan).
    void doSingleScan();
     * Starts scanning for serial ports provided by USB devices in the background; informs listeners about newly
     * discovered devices. Should return fast.
    void startBackgroundScanning();
     * Stops scanning for serial ports provided by USB devices in the background. Should return fast.
    void stopBackgroundScanning();
     * Registers an {@link UsbSerialDiscoveryListener} that is then notified about discovered USB serial ports.
     * Previously found devices will be notified during registration.
    void registerDiscoveryListener(UsbSerialDiscoveryListener listener);
     * Unregisters an {@link UsbSerialDiscoveryListener}.
    void unregisterDiscoveryListener(UsbSerialDiscoveryListener listener);
