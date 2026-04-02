package org.openhab.core.io.transport.serial.rxtx;
import org.openhab.core.io.transport.serial.internal.SerialPortEventImpl;
import gnu.io.SerialPortEvent;
public class RxTxSerialPort implements SerialPort {
    private final gnu.io.SerialPort sp;
    public RxTxSerialPort(final gnu.io.SerialPort sp) {
        } catch (gnu.io.UnsupportedCommOperationException ex) {
            throw new UnsupportedCommOperationException();
        sp.addEventListener(new gnu.io.SerialPortEventListener() {
        } catch (gnu.io.UnsupportedCommOperationException e) {
