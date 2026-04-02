 * Some general filename and extension utilities.
public class AudioStreamUtils {
    public static final String EXTENSION_SEPARATOR = ".";
     * Gets the base name of a filename.
     * @param filename the filename to query
     * @return the base name of the file or an empty string if none exists or {@code null} if the filename is
     *         {@code null}
    public static String getBaseName(String filename) {
        final int index = filename.lastIndexOf(EXTENSION_SEPARATOR);
        if (index == -1) {
            return filename.substring(0, index);
     * Gets the extension of a filename.
     * @param filename the filename to retrieve the extension of
     * @return the extension of the file or an empty string if none exists or {@code null} if the filename is
    public static String getExtension(String filename) {
            return filename.substring(index + 1);
     * Checks if the extension of a filename matches the given.
     * @param filename the filename to check the extension of
     * @param extension the extension to check for
     * @return {@code true} if the filename has the specified extension
    public static boolean isExtension(String filename, String extension) {
        return !extension.isEmpty() && getExtension(filename).equals(extension);
