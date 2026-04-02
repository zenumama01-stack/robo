 * {@link ConversionException} generic exception for errors which occurs during conversion.
public class ConversionException extends Exception {
     * Constructs a new ConversionException with no detail message.
    public ConversionException() {
     * Constructs a new ConversionException with the specified detail message.
     * @param message the detail message describing the conversion error
    public ConversionException(String message) {
     * Constructs a new ConversionException with the specified detail message and cause.
     * @param cause the underlying cause of the conversion error
    public ConversionException(String message, Throwable cause) {
     * Constructs a new ConversionException with the specified cause.
    public ConversionException(Throwable cause) {
