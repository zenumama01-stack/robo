 * Provides an exception class for openHAB to be used in case of configuration issues
public class ConfigurationException extends AbstractI18nException {
    public ConfigurationException(String message, @Nullable Object @Nullable... msgParams) {
    public ConfigurationException(String message, @Nullable Throwable cause, @Nullable Object @Nullable... msgParams) {
    public ConfigurationException(Throwable cause) {
