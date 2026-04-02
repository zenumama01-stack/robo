 * Listener interface for {@link UsbSerialDiscovery}s.
public interface UsbSerialDiscoveryListener {
     * Called when a new serial port provided by a USB device is discovered.
    void usbSerialDeviceDiscovered(UsbSerialDeviceInformation usbSerialDeviceInformation);
     * Called when a serial port provided by a USB device has been removed.
    void usbSerialDeviceRemoved(UsbSerialDeviceInformation usbSerialDeviceInformation);
