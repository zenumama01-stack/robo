 * This is an {@link Exception} implementation for automation objects that retain some additional information.
public class ValidationException extends Exception {
     * Keeps information about the type of the automation object for validation - module type, template or rule.
    private final ObjectType type;
     * Keeps information about the UID of the automation object for validation - module type, template or rule.
    private final @Nullable String uid;
     * Creates a new instance with the specified type, UID and message.
     * @param type the {@link ObjectType} to use.
     * @param uid the UID to use, if any.
     * @param message The detail message.
    public ValidationException(ObjectType type, @Nullable String uid, @Nullable String message) {
     * Creates a new instance with the specified type, UID and cause.
     * @param cause the {@link Throwable} that caused this {@link Exception}.
    public ValidationException(ObjectType type, @Nullable String uid, @Nullable Throwable cause) {
     * Creates a new instance with the specified type, UID, message and cause.
    public ValidationException(ObjectType type, @Nullable String uid, @Nullable String message,
     * @param enableSuppression whether or not suppression is enabled or disabled.
     * @param writableStackTrace whether or not the stack trace should be writable.
            @Nullable Throwable cause, boolean enableSuppression, boolean writableStackTrace) {
        super(message, cause, enableSuppression, writableStackTrace);
            sb.append(' ').append(uid);
        sb.append("] ").append(super.getMessage());
    public enum ObjectType {
        MODULE_TYPE,
        TEMPLATE,
        RULE
