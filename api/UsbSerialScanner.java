 * Implementations of this interface scan for serial ports provided by USB devices.
public interface UsbSerialScanner {
     * Performs a single scan for serial ports provided by USB devices.
     * @return A collection containing all scan results.
     * @throws IOException if an I/O issue prevented the scan. Note that implementors are free to swallow I/O issues
     *             that occur when trying to read the information about a single USB device or serial port, so that
     *             information about other devices can still be retrieved. (Such issues should nevertheless be logged by
     *             implementors.)
    Set<UsbSerialDeviceInformation> scan() throws IOException;
     * {@link UsbSerialScanner}s might be able to perform scans only on certain platforms, or with proper configuration.
     * {@link UsbSerialScanner}s can indicate whether they are able to perform scans using this method.
     * @return <code>true</code> if able to perform scans, and <code>false</code> otherwise.
    boolean canPerformScans();
