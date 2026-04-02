import java.io.Serial;
 * This is an exception that can be thrown by {@link MarketplaceAddonHandler}s if some operation fails.
public class MarketplaceHandlerException extends Exception {
    @Serial
    private static final long serialVersionUID = -5652014141471618161L;
     * Main constructor
     * @param message A message describing the issue
    public MarketplaceHandlerException(String message, @Nullable Throwable cause) {
        super(message, cause);
