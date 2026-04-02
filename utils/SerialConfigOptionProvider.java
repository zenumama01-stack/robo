package org.openhab.core.config.serial.internal;
import org.openhab.core.io.transport.serial.SerialPortIdentifier;
import org.openhab.core.io.transport.serial.SerialPortManager;
 * This service provides serial port names as options for configuration parameters.
 * @author Wouter Born - Add discovered USB serial port names to serial port parameter options
public class SerialConfigOptionProvider implements ConfigOptionProvider, UsbSerialDiscoveryListener {
    static final String SERIAL_PORT = "serial-port";
    private final SerialPortManager serialPortManager;
     * Creates a new SerialConfigOptionProvider.
     * @param serialPortManager the serial port manager service used to retrieve available serial ports
    public SerialConfigOptionProvider(final @Reference SerialPortManager serialPortManager) {
        this.serialPortManager = serialPortManager;
     * Dynamically adds a USB serial discovery service.
     * This method is called by the OSGi framework when a new {@link UsbSerialDiscovery} service becomes available.
     * The discovery service is registered as a listener to receive notifications about USB serial device
     * additions and removals.
     * This method is synchronized to prevent race conditions with {@link #removeUsbSerialDiscovery(UsbSerialDiscovery)}
     * when services are dynamically bound and unbound.
     * @param usbSerialDiscovery the USB serial discovery service to add (must not be null)
    protected synchronized void addUsbSerialDiscovery(UsbSerialDiscovery usbSerialDiscovery) {
     * Dynamically removes a USB serial discovery service.
     * This method is called by the OSGi framework when a {@link UsbSerialDiscovery} service becomes unavailable.
     * The discovery service is unregistered as a listener and removed from the active discovery set.
     * <b>Note:</b> This method clears all previously discovered USB serial devices, not just those
     * discovered by this specific service. This ensures a clean state when discovery services are
     * dynamically removed and re-added, preventing stale device information.
     * This method is synchronized to prevent race conditions with {@link #addUsbSerialDiscovery(UsbSerialDiscovery)}
     * @param usbSerialDiscovery the USB serial discovery service to remove (must not be null)
     * Called when a USB serial device is discovered.
     * This method is invoked by {@link UsbSerialDiscovery} services when they detect a new USB serial device.
     * The discovered device is added to the internal cache and will be included in the parameter options
     * returned by {@link #getParameterOptions(URI, String, String, Locale)}.
     * @param usbSerialDeviceInformation information about the discovered USB serial device
        previouslyDiscovered.add(usbSerialDeviceInformation);
     * Called when a USB serial device is removed.
     * This method is invoked by {@link UsbSerialDiscovery} services when they detect that a USB serial device
     * has been disconnected. The device is removed from the internal cache and will no longer be included
     * in the parameter options.
     * @param usbSerialDeviceInformation information about the removed USB serial device
     * Provides serial port names as configuration parameter options.
     * This method is called by the configuration framework to populate serial port selection dropdowns
     * in the UI. It combines serial ports from both the {@link SerialPortManager} and any USB serial
     * devices discovered through {@link UsbSerialDiscovery} services.
     * @param uri the URI of the configuration (not used in this implementation)
     * @param param the parameter name (not used in this implementation)
     * @param context the parameter context; returns serial port options only if context equals "serial-port"
     * @param locale the locale for internationalization (not used in this implementation)
     * @return a collection of parameter options containing available serial port names,
     *         or {@code null} if the context is not "serial-port"
        if (SERIAL_PORT.equals(context)) {
            return Stream
                    .concat(serialPortManager.getIdentifiers().map(SerialPortIdentifier::getName),
                            previouslyDiscovered.stream().map(UsbSerialDeviceInformation::getSerialPort))
                    .filter(serialPortName -> serialPortName != null && !serialPortName.isEmpty()) //
                    .map(serialPortName -> new ParameterOption(serialPortName, serialPortName)) //
