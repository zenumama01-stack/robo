 * {@link ThingStatus} defines possible statuses of a {@link ThingStatusInfo}.
 * @author Simon Kaufmann - added UNKNOWN, removed numbers
public enum ThingStatus {
    UNINITIALIZED,
    INITIALIZING,
    UNKNOWN,
    ONLINE,
    OFFLINE,
    REMOVING,
    REMOVED
