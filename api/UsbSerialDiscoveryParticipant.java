 * A {@link UsbSerialDiscoveryParticipant} that is registered as a component is picked up by the
 * {@link UsbSerialDiscoveryService} and can thus contribute {@link DiscoveryResult}s from
 * scans for USB devices with an associated serial port.
public interface UsbSerialDiscoveryParticipant {
     * Defines the list of thing types that this participant can identify.
     * Creates a discovery result for a USB device with corresponding serial port.
     * @param deviceInformation information about the USB device and the corresponding serial port
     * @return the according discovery result or <code>null</code> if the device is not
    DiscoveryResult createResult(UsbSerialDeviceInformation deviceInformation);
     * Returns the thing UID for a USB device with corresponding serial port.
     * @return a thing UID or <code>null</code> if the device is not supported
    ThingUID getThingUID(UsbSerialDeviceInformation deviceInformation);
