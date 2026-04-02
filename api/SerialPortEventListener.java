 * Interface of a serial port event listener.
public interface SerialPortEventListener {
     * Notify about a serial event.
     * @param event the event
    void serialEvent(SerialPortEvent event);
