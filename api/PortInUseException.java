package org.openhab.core.io.transport.serial;
 * Exception that marks that a port is currently already in use.
public class PortInUseException extends Exception {
    private static final long serialVersionUID = -2709480420743139383L;
    public PortInUseException(String message, Exception cause) {
    public PortInUseException(Exception cause) {
